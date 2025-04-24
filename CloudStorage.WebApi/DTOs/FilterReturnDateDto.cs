namespace CloudStorage.WebApi.DTOs;

public record FilterReturnDateDto(List<ReturnFileDateDto> Files, List<ReturnDirDateDto> Dirs);
