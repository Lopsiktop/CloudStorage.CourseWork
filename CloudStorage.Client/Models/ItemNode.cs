using System.Collections.ObjectModel;

namespace CloudStorage.Client.Models;

public class ItemNode
{
    public int DirId { get; set; }
    public string Name { get; set; }
    public NodeType Type { get; set; }
    public DateTime CreationTime { get; set; }
    public ObservableCollection<ItemNode> Nodes { get; set; } = new ObservableCollection<ItemNode>();
}
