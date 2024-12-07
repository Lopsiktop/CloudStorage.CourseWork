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

    public RootViewModel()
    {
        var node = new ItemNode { Name = "Диск", Type = NodeType.Disk };
        Nodes.Add(node);
        DirSteps.Clear();
    }

    public async Task TreeValueChanged()
    {
        var dir = new ReturnDirDto(TreeValue.DirId, TreeValue.Name);
        await LoadDir(dir);
    }

    [RelayCommand(CanExecute = nameof(CanMoveBack))]
    private async void MoveBack()
    {
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
        var last = PrevSteps.Last();
        PrevSteps.RemoveAt(PrevSteps.Count - 1);

        MoveBackCommand.NotifyCanExecuteChanged();
        MoveForwardCommand.NotifyCanExecuteChanged();

        await LoadDir(last, true);
    }

    private bool CanMoveForward() => PrevSteps.Count > 0;

    [RelayCommand]
    private void Logout()
    {
        SessionHandler.LogoutSession();
        WindowUtils.ReturnRootWindow();
    }

    public async Task FolderDoubleClick(ReturnDirDto folder)
    {
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

            var structure = await ApiHelper.GetStructure();
            if (!structure.IsError)
            {
                foreach (var dir in structure.Value)
                    AddFolder(node, dir);
            }

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
