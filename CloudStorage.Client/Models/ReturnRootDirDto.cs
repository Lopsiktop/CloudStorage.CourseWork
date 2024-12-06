namespace CloudStorage.Client.Models;

public record ReturnRootDirDto(int DirId, string DirName, IEnumerable<ReturnFileDto> Files, IEnumerable<ReturnDirDto> Dirs);