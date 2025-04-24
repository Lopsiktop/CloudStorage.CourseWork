namespace CloudStorage.Client.Models;

public record FilterReturnDto(List<ReturnFileDto> Files, List<ReturnDirDto> Dirs);
