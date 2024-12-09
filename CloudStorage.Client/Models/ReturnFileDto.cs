using CommunityToolkit.Mvvm.ComponentModel;

namespace CloudStorage.Client.Models;

public partial class ReturnFileDto : ObservableObject
{
    public int Id { get; set; }

    [ObservableProperty]
    private string _Name;
    public decimal Size { get; set; }

    [ObservableProperty]
    private bool _IsEditing = false;

    public ReturnFileDto(int id, string name, decimal size)
    {
        Id = id;
        Name = name;
        Size = size;
    }
}
