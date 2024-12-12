namespace CloudStorage.Client.Models;

public record ReturnHistoryDto(int Id, ActionType Type, ReturnFileDto? File, ReturnDirDto? Dir, string Text, DateTime Date);