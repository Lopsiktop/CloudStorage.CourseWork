namespace CloudStorage.Client.Models;

public record ReturnDirSizeDto(int DirId, string DirName, decimal Size, DateTime CreationTime);