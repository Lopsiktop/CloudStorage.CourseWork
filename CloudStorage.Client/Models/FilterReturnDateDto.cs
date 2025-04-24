namespace CloudStorage.Client.Models;

public record FilterReturnDateDto(List<ReturnFileDateDto> Files, List<ReturnDirDateDto> Dirs);
