namespace CloudStorage.Client.Models;

public record FolderProperties(string Path, DateTime CreatedDate, decimal Size, int Folders, int Files);