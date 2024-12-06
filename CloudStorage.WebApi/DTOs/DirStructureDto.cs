namespace CloudStorage.WebApi.DTOs;

public record DirStructureDto(int DirId, string DirName, List<DirStructureDto> Dirs);