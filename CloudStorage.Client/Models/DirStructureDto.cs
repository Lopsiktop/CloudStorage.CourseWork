namespace CloudStorage.Client.Models;

public record DirStructureDto(int DirId, string DirName, List<DirStructureDto> Dirs);