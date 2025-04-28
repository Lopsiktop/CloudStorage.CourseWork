using CloudStorage.Client.Models;
using CloudStorage.Client.UI.UIHelpers;
using CloudStorage.Client.Utils;
using CloudStorage.Client.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace CloudStorage.Client.ViewModels;

public partial class RootViewModel : ObservableValidator
{
    public ObservableCollection<ItemNode> Nodes { get; set; } = new ObservableCollection<ItemNode>();
    public ObservableCollection<ReturnDirDto> Dirs { get; set; } = new ObservableCollection<ReturnDirDto>();
    public ObservableCollection<ReturnFileDto> Files { get; set; } = new ObservableCollection<ReturnFileDto>();

    private List<ReturnDirDto> DirSteps = new List<ReturnDirDto>();
    private List<ReturnDirDto> PrevSteps = new List<ReturnDirDto>();

    [ObservableProperty]
    private ItemNode _TreeValue;

    [ObservableProperty]
    private ReturnDirDto _CurrentDir;

    [ObservableProperty]
    private bool _IsLoading;

    [ObservableProperty]
    private bool _IsDragging;

    [ObservableProperty]
    private bool _IsAdmin;

    public ReturnFileDto OldFile { get; set; }
    public ReturnDirDto OldDir { get; set; }

    #region Search

    private int _SearchType;

    public int SearchType
    {
        get => _SearchType;
        set
        {
            SetProperty(ref _SearchType, value);

            if (value == 0)
            {
                TextFieldVisibility = Visibility.Visible;
                DateFieldVisibility = Visibility.Collapsed;
            }
            else if (value == 1)
            {
                TextFieldVisibility = Visibility.Collapsed;
                DateFieldVisibility = Visibility.Visible;
            }
        }
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
    private string _SearchField;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
    private Visibility _TextFieldVisibility = Visibility.Visible;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
    private Visibility _DateFieldVisibility = Visibility.Collapsed;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
    private DateTime _DateFrom = DateTime.Now;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
    private DateTime _DateTo = DateTime.Now;

    [RelayCommand(CanExecute = nameof(CanSearchMethodExecute))]
    private async void Search()
    {
        Result<FilterReturnDto> result = new Result<FilterReturnDto>("Не удалось выполнить поиск (code: 50)");

        if (SearchType == 0)
            result = await ApiHelper.SearchByField(SearchField, CurrentDir.DirId);
        else if (SearchType == 1)
            result = await ApiHelper.SearchByDates(DateFrom, DateTo, CurrentDir.DirId);
        
        if (result.IsError)
        {
            await Notify.ShowAsync("Ошибка", result.Error, NotifyType.Error);
            return;
        }

        Dirs.Clear();
        Files.Clear();

        foreach (var dir in result.Value.Dirs)
            Dirs.Add(dir);

        foreach (var file in result.Value.Files)
            Files.Add(file);
    }

    private bool CanSearchMethodExecute()
    {
        if (SearchType == 0 && string.IsNullOrWhiteSpace(_SearchField))
            return false;

        if (SearchType == 1 && DateFrom > DateTo)
            return false;

        return true;
    }

    #endregion

    #region Sort

    [RelayCommand]
    private async void Sort(string sortType)
    {
        switch (sortType)
        {
            case "0":
                // by name down
                Dirs = Dirs.OrderBy(x => x.DirName).ToObservableCollection();
                Files = Files.OrderBy(x => x.Name).ToObservableCollection();
                break;
            case "1":
                // by name up
                Dirs = Dirs.OrderByDescending(x => x.DirName).ToObservableCollection();
                Files = Files.OrderByDescending(x => x.Name).ToObservableCollection();
                break;
            case "2":
                // by date down
                var result1 = await ApiHelper.SortByDate(Dirs.Select(x => x.DirId).ToArray(),
                    Files.Select(x => x.Id).ToArray());

                if (result1.IsError)
                {
                    await Notify.ShowAsync("Ошибка", result1.Error, NotifyType.Error);
                    return;
                }

                Dirs = result1.Value.Dirs.OrderByDescending(x => x.CreationTime).Select(x => new ReturnDirDto(x.DirId, x.DirName)).ToObservableCollection();
                Files = result1.Value.Files.OrderByDescending(x => x.CreationTime).Select(x => new ReturnFileDto(x.Id, x.Name, x.Size)).ToObservableCollection();

                break;
            case "3":
                // by date up
                var result2 = await ApiHelper.SortByDate(Dirs.Select(x => x.DirId).ToArray(),
                    Files.Select(x => x.Id).ToArray());

                if (result2.IsError)
                {
                    await Notify.ShowAsync("Ошибка", result2.Error, NotifyType.Error);
                    return;
                }

                Dirs = result2.Value.Dirs.Select(x => new ReturnDirDto(x.DirId, x.DirName)).ToObservableCollection();
                Files = result2.Value.Files.Select(x => new ReturnFileDto(x.Id, x.Name, x.Size)).ToObservableCollection();

                break;
            case "4":
                // by size down
                var result3 = await ApiHelper.SortBySize(Dirs.Select(x => x.DirId).ToArray());
                if (result3.IsError)
                {
                    await Notify.ShowAsync("Ошибка", result3.Error, NotifyType.Error);
                    return;
                }

                Dirs = result3.Value.OrderByDescending(x => x.Size).Select(x => new ReturnDirDto(x.DirId, x.DirName)).ToObservableCollection();
                Files = Files.OrderByDescending(x => x.Size).ToObservableCollection();

                break;
            case "5":
                // by size up
                var result4 = await ApiHelper.SortBySize(Dirs.Select(x => x.DirId).ToArray());
                if (result4.IsError)
                {
                    await Notify.ShowAsync("Ошибка", result4.Error, NotifyType.Error);
                    return;
                }

                Dirs = result4.Value.Select(x => new ReturnDirDto(x.DirId, x.DirName)).ToObservableCollection();
                Files = Files.OrderBy(x => x.Size).ToObservableCollection();
                break;
        }

        OnPropertyChanged(nameof(Dirs));
        OnPropertyChanged(nameof(Files));
    }

    #endregion

    public RootViewModel()
    {
        var node = new ItemNode { Name = "Диск", Type = NodeType.Disk };
        var trash = new ItemNode { Name = "Корзина", Type = NodeType.Trash };
        Nodes.Add(node);
        Nodes.Add(trash);
        DirSteps.Clear();
        IsAdmin = UserHandler.IsAdmin;
    }

    private async Task<bool> IsBusy()
    {
        var can = await LostFocusEditable();
        if (!can)
            return true;

        return false;
    }

    public async Task MoveDirToBin(ReturnDirDto dir)
    {
        var confirm = Confirm.ShowConfimation($"Вы точно хотите удалить папку \"{dir.DirName}\" со всем ее содержимым?", "Удалить", "Нет");
        if (!confirm)
            return;

        var result = await ApiHelper.MoveDirToBin(dir.DirId);
        if (result.IsError)
        {
            await Notify.ShowAsync("Ошибка", result.Error, NotifyType.Error);
            return;
        }

        await RefreshStructure();
        Dirs.Remove(dir);
    }

    public async Task MoveFileToBin(ReturnFileDto file)
    {
        var confirm = Confirm.ShowConfimation($"Вы точно хотите удалить файл \"{file.Name}\"?", "Удалить", "Нет");
        if (!confirm)
            return;

        var result = await ApiHelper.MoveFileToBin(file.Id);
        if (result.IsError)
        {
            await Notify.ShowAsync("Ошибка", result.Error, NotifyType.Error);
            return;
        }

        Files.Remove(file);
    }

    public async Task DeleteFile(ReturnFileDto file)
    {
        var confirm = Confirm.ShowConfimation($"Вы точно хотите удалить файл \"{file.Name}\" навсегда?", "Удалить", "Нет");
        if (!confirm)
            return;

        var result = await ApiHelper.DeleteFile(file.Id);
        if (result.IsError)
        {
            await Notify.ShowAsync("Ошибка", result.Error, NotifyType.Error);
            return;
        }

        Files.Remove(file);
    }

    public async Task DeleteDirectory(ReturnDirDto dir)
    {
        var confirm = Confirm.ShowConfimation($"Вы точно хотите удалить папку \"{dir.DirName}\" со всем ее содержимым навсегда?", "Удалить", "Нет");
        if (!confirm)
            return;

        var result = await ApiHelper.DeleteDirectory(dir.DirId);
        if (result.IsError)
        {
            await Notify.ShowAsync("Ошибка", result.Error, NotifyType.Error);
            return;
        }

        Dirs.Remove(dir);
    }

    public async Task RenameDirectory(ReturnDirDto dir)
    {
        if (OldDir.DirName == dir.DirName)
        {
            dir.IsEditing = false;
            return;
        }

        var result = await ApiHelper.RenameDirectory(dir.DirId, dir.DirName);
        if (result.IsError)
        {
            await Notify.ShowAsync("Ошибка", result.Error, NotifyType.Error);
            return;
        }

        dir.IsEditing = false;
    }

    public async Task RenameFile(ReturnFileDto file)
    {
        if (OldFile.Name == file.Name)
        {
            file.IsEditing = false;
            return;
        }

        var result = await ApiHelper.RenameFile(file.Id, file.Name);
        if (result.IsError)
        {
            await Notify.ShowAsync("Ошибка", result.Error, NotifyType.Error);
            return;
        }

        file.IsEditing = false;
    }

    public async Task LoadFiles(string[] files)
    {
        var result = await ApiHelper.LoadFiles(files[0], CurrentDir.DirId);
        if (result.IsError)
        {
            await Notify.ShowAsync("Ошибка", result.Error, NotifyType.Error);
            return;
        }

        Files.Add(result.Value);
        await Notify.ShowAsync("Успех", "Файл успешно добавлен", NotifyType.Success);
    }

    public async Task<bool> LostFocusEditable()
    {
        var dir = Dirs.FirstOrDefault(x => x.IsEditing);
        if(dir != null)
        {
            if (string.IsNullOrWhiteSpace(dir.DirName))
            {
                await Notify.ShowAsync("Ошибка", "Вы должны указать имя для папки", NotifyType.Error);
                return false;
            }
            
            if(dir.DirId == -1)
            {
                dir.IsEditing = false;
                var create = await ApiHelper.CreateDirectory(new CreateDirDto(dir.DirName, CurrentDir.DirId));
                if (create.IsError)
                {
                    Dirs.Remove(dir);
                    await Notify.ShowAsync("Ошибка", create.Error, NotifyType.Error);
                    return true;
                }

                var value = create.Value;
                dir.DirId = value.DirId;

                await RefreshStructure();
                await Notify.ShowAsync("Успех", "Папка успешно создана", NotifyType.Success, 2);
                return true;
            }
            else
            {
                await RenameDirectory(dir);
            }
        }

        var file = Files.FirstOrDefault(x => x.IsEditing);
        if(file != null)
        {
            //editing
            if(file.Id != -1)
            {
                await RenameFile(file);
            }
        }

        return true;
    }

    [RelayCommand]
    private async void CreateDir()
    {
        if (await IsBusy())
            return;

        Dirs.Add(new ReturnDirDto(-1, "") { IsEditing = true });
    }

    public async Task TreeValueChanged()
    {
        if (await IsBusy())
            return;

        var dir = new ReturnDirDto(TreeValue.DirId, TreeValue.Name);
        await LoadDir(dir);
    }

    [RelayCommand(CanExecute = nameof(CanMoveBack))]
    private async void MoveBack()
    {
        if (await IsBusy())
            return;

        var prev = DirSteps[DirSteps.Count - 2];
        PrevSteps.Add(DirSteps.Last());
        DirSteps.RemoveAt(DirSteps.Count - 1);

        MoveBackCommand.NotifyCanExecuteChanged();
        MoveForwardCommand.NotifyCanExecuteChanged();

        await LoadDir(prev, false);
    }

    private bool CanMoveBack() => DirSteps.Count > 1;

    [RelayCommand(CanExecute = nameof(CanMoveForward))]
    private async void MoveForward()
    {
        if (await IsBusy())
            return;

        var last = PrevSteps.Last();
        PrevSteps.RemoveAt(PrevSteps.Count - 1);

        MoveBackCommand.NotifyCanExecuteChanged();
        MoveForwardCommand.NotifyCanExecuteChanged();

        await LoadDir(last, true);
    }

    private bool CanMoveForward() => PrevSteps.Count > 0;

    [RelayCommand]
    private async void Logout()
    {
        if (await IsBusy())
            return;

        SessionHandler.LogoutSession();
        WindowUtils.ReturnRootWindow();
    }

    public async Task FolderDoubleClick(ReturnDirDto folder)
    {
        if (await IsBusy())
            return;

        await LoadDir(folder);
    }

    public ItemNode? GetNode(ItemNode node, int id)
    {
        if (node.DirId == id)
            return node;

        foreach (var item in node.Nodes)
        {
            var newNode = GetNode(item, id);
            if (newNode != null)
                return newNode;
        }

        return null;
    }

    public async Task RefreshStructure()
    {
        var node = Nodes.First();
        node.Nodes.Clear();
        var structure = await ApiHelper.GetStructure();
        if (!structure.IsError)
        {
            foreach (var dir in structure.Value)
                AddFolder(node, dir);
        }
    }

    public async Task Loaded()
    {
        IsLoading = true;
        var node = Nodes.First();

        var result = await ApiHelper.GetMeAsync();
        if (!result.IsError)
        {
            node.Nodes.Clear();
            DirSteps.Clear();
            Dirs.Clear();
            Files.Clear();

            node.DirId = result.Value.RootDir.DirId;
            Nodes[1].DirId = result.Value.TrashDir.DirId;
            var root = new ReturnDirDto(node.DirId, node.Name);
            DirSteps.Add(root);
            CurrentDir = root;

            await RefreshStructure();

            foreach (var dir in result.Value.RootDir.Dirs)
                Dirs.Add(dir);

            foreach (var file in result.Value.RootDir.Files)
                Files.Add(file);
        }

        IsLoading = false;
    }

    public async Task LoadDir(ReturnDirDto dirt, bool step = true)
    {
        var dirId = dirt.DirId;
        var dirsResponse = await ApiHelper.GetDirsByFolderId(dirId);
        var foldersResponse = await ApiHelper.GetFilesByFolderId(dirId);

        if (!dirsResponse.IsError)
        {
            Dirs.Clear();

            foreach (var dir in dirsResponse.Value)
                Dirs.Add(dir);
        }

        if (!foldersResponse.IsError)
        {
            Files.Clear();

            foreach (var file in foldersResponse.Value)
                Files.Add(file);
        }

        if(step)
            DirSteps.Add(dirt);

        MoveBackCommand.NotifyCanExecuteChanged();
        CurrentDir = dirt;
    }

    private void AddFolder(ItemNode node, DirStructureDto dir)
    {
        var newNode = new ItemNode { DirId = dir.DirId, Name = dir.DirName, Type = NodeType.Folder };
        node.Nodes.Add(newNode);
        if (dir.Dirs.Count == 0)
            return;

        foreach (var item in dir.Dirs)
            AddFolder(newNode, item);
    }

    public async Task DownloadFile(ReturnFileDto file)
    {
        var dialog = new SaveFileDialog();
        dialog.FileName = file.Name;
        if(dialog.ShowDialog() == true)
        {
            var result = await ApiHelper.DownloadFile(file.Id, dialog.FileName);
            if (result.IsError)
            {
                await Notify.ShowAsync("Ошибка", result.Error, NotifyType.Error);
                return;
            }

            await Notify.ShowAsync("Успех", "Файл успешно загружен", NotifyType.Success);
        }
    }

    public async Task DownloadDirectory(ReturnDirDto folder)
    {
        var dialog = new SaveFileDialog();
        dialog.Filter = "Zip | *.zip";
        dialog.FileName = folder.DirName + ".zip";
        if (dialog.ShowDialog() == true)
        {
            var result = await ApiHelper.DownloadFolder(folder.DirId, dialog.FileName);
            if (result.IsError)
            {
                await Notify.ShowAsync("Ошибка", result.Error, NotifyType.Error);
                return;
            }

            await Notify.ShowAsync("Успех", "Папка успешно загружена", NotifyType.Success);
        }
    }

    public async Task RefreshFileFromTrash(ReturnFileDto parameter)
    {
        var result = await ApiHelper.RefreshFileFromBin(parameter.Id);
        if (result.IsError)
        {
            await Notify.ShowAsync("Ошибка", result.Error, NotifyType.Error);
            return;
        }

        Files.Remove(parameter);
        await Notify.ShowAsync("Успех", "Файл успешно восстановлен", NotifyType.Success, 2);
    }

    public async Task RefreshFolderFromTrash(ReturnDirDto parameter)
    {
        var result = await ApiHelper.RefreshDirectoryFromBin(parameter.DirId);
        if (result.IsError)
        {
            await Notify.ShowAsync("Ошибка", result.Error, NotifyType.Error);
            return;
        }

        await RefreshStructure();
        Dirs.Remove(parameter);
        await Notify.ShowAsync("Успех", "Папка успешно восстановлена", NotifyType.Success, 2);
    }

    [RelayCommand]
    private void ShowHistory()
    {
        var viewModel = new HistoryViewModel();
        WindowUtils.ShowDialogWindow<HistoryWindow>(viewModel, async () => await viewModel.Load());
    }

    public async Task FolderHistory(ReturnDirDto parameter)
    {
        var viewModel = new HistoryViewModel();
        WindowUtils.ShowDialogWindow<HistoryWindow>(viewModel, async () => await viewModel.Load(dirId: parameter.DirId));
    }

    public async Task FileHistory(ReturnFileDto parameter)
    {
        var viewModel = new HistoryViewModel();
        WindowUtils.ShowDialogWindow<HistoryWindow>(viewModel, async () => await viewModel.Load(fileId: parameter.Id));
    }

    public async Task FileProperties(ReturnFileDto parameter)
    {
        var prop = await ApiHelper.GetFileProperties(parameter.Id);
        if (prop.IsError)
        {
            await Notify.ShowAsync("Ошибка", prop.Error, NotifyType.Error);
            return;
        }

        Property.ShowProperty(new PropertyViewModel(prop.Value, parameter.Name));
    }

    public async Task FolderProperties(ReturnDirDto parameter)
    {
        var prop = await ApiHelper.GetFolderProperties(parameter.DirId);
        if (prop.IsError)
        {
            await Notify.ShowAsync("Ошибка", prop.Error, NotifyType.Error);
            return;
        }

        Property.ShowProperty(new PropertyViewModel(prop.Value, parameter.DirName));
    }

    public async Task ShareFolder(ReturnDirDto parameter)
    {
        var result = await ApiHelper.ShareFolderLink(parameter.DirId);
        if (result.IsError)
        {
            await Notify.ShowAsync("Ошибка", result.Error, NotifyType.Error);
            return;
        }

        Share.ShowUrl(result.Value.Url);
    }

    public async Task ShareFile(ReturnFileDto parameter)
    {
        var result = await ApiHelper.ShareFileLink(parameter.Id);
        if (result.IsError)
        {
            await Notify.ShowAsync("Ошибка", result.Error, NotifyType.Error);
            return;
        }

        Share.ShowUrl(result.Value.Url);
    }
}
