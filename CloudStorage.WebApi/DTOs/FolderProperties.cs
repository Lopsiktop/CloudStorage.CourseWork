namespace CloudStorage.WebApi.DTOs;

public record FolderProperties(string Path, DateTime CreatedDate, decimal Size, int Folders, int Files);