using CloudStorage.Client.Models;
using CloudStorage.Client.UI.UIHelpers;
using CloudStorage.Client.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
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

    public RootViewModel()
    {
        var node = new ItemNode { Name = "Диск", Type = NodeType.Disk };
        Nodes.Add(node);
        DirSteps.Clear();
    }

    private async Task<bool> IsBusy()
    {
        var can = await LostFocusEditable();
        if (!can)
            return true;

        return false;
    }

    public async Task DeleteFile(ReturnFileDto file)
    {
        var confirm = Confirm.ShowConfimation($"Вы точно хотите удалить файл \"{file.Name}\"?", "Удалить", "Нет");
        if (!confirm)
            return;

        var result = await ApiHelper.DeleteFile(file.Id);
        if (result.IsError)
        {
            await Notify.ShowAsync("Ошибка", result.Error, NotifyType.Error);
            return;
        }

        Files.Remove(file);
        await Notify.ShowAsync("Успех", "Файл успешно удален", NotifyType.Success, 2);
    }

    public async Task DeleteDirectory(ReturnDirDto dir)
    {
        var confirm = Confirm.ShowConfimation($"Вы точно хотите удалить папку \"{dir.DirName}\" со всем ее содержимым?", "Удалить", "Нет");
        if (!confirm)
            return;

        var result = await ApiHelper.DeleteDirectory(dir.DirId);
        if (result.IsError)
        {
            await Notify.ShowAsync("Ошибка", result.Error, NotifyType.Error);
            return;
        }

        Dirs.Remove(dir);
        await Notify.ShowAsync("Успех", "Папка успешно удалена", NotifyType.Success, 2);
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
            dir.IsEditing = false;
            
            if(dir.DirId == -1)
            {
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
                //todo: edit dir using api
            }
        }

        //todo: rename for file

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
}
