using CloudStorage.Client.Models;
using CloudStorage.Client.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
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

    [ObservableProperty]
    private ItemNode _TreeValue;

    public RootViewModel()
    {
        var node = new ItemNode { Name = "Диск", Type = NodeType.Disk };
        Nodes.Add(node);
    }

    [RelayCommand]
    private void Logout()
    {
        SessionHandler.LogoutSession();
        WindowUtils.ReturnRootWindow();
    }

    public async Task Loaded()
    {
        var node = Nodes.First();

        var result = await ApiHelper.GetMeAsync();
        if (!result.IsError)
        {
            node.Nodes.Clear();
            Dirs.Clear();
            Files.Clear();

            node.DirId = result.Value.RootDir.DirId;

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
