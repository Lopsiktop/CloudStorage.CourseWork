namespace CloudStorage.WebApi.DTOs;

public record ReturnDirSizeDto(int DirId, string DirName, decimal Size, DateTime CreationTime);