namespace CloudStorage.WebApi.DTOs;

public record FilterReturnDto(List<ReturnFileDto> Files, List<ReturnDirDto> Dirs);
