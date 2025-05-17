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

    [ObservableProperty]
    private DateTime _CreationTime;

    public ReturnFileDto(int id, string name, decimal size, DateTime creationTime)
    {
        Id = id;
        Name = name;
        Size = size;
        CreationTime = creationTime;
    }
}
