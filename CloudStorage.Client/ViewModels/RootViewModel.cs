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
    public ObservableCollection<ReturnDirDto> Dirs { get; set; } = new ObservableCollection<ReturnDirDto>()
    {
        new ReturnDirDto(0, "hello")
    };
    public ObservableCollection<ReturnFileDto> Files { get; set; } = new ObservableCollection<ReturnFileDto>();

    private List<int> DirSteps = new List<int>();

    [ObservableProperty]
    private ItemNode _TreeValue;

    public RootViewModel()
    {
        var node = new ItemNode { Name = "Диск", Type = NodeType.Disk };
        Nodes.Add(node);
        DirSteps.Clear();
    }

    [RelayCommand(CanExecute = nameof(CanMoveBack))]
    private async void MoveBack()
    {
        var prev = DirSteps[DirSteps.Count - 2];
        DirSteps.RemoveAt(DirSteps.Count - 1);
        MoveBackCommand.NotifyCanExecuteChanged();
        await LoadDir(prev, false);
    }

    private bool CanMoveBack() => DirSteps.Count > 1;

    [RelayCommand]
    private void MoveForward()
    {

    }

    [RelayCommand]
    private void Logout()
    {
        SessionHandler.LogoutSession();
        WindowUtils.ReturnRootWindow();
    }

    public async Task FolderDoubleClick(ReturnDirDto folder)
    {
        await LoadDir(folder.DirId);
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
        var node = Nodes.First();

        var result = await ApiHelper.GetMeAsync();
        if (!result.IsError)
        {
            node.Nodes.Clear();
            DirSteps.Clear();
            Dirs.Clear();
            Files.Clear();

            node.DirId = result.Value.RootDir.DirId;
            DirSteps.Add(node.DirId);

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
    }

    public async Task LoadDir(int dirId, bool step = true)
    {
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
            DirSteps.Add(dirId);
        MoveBackCommand.NotifyCanExecuteChanged();
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
