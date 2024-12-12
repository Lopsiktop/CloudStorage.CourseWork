using CloudStorage.Data.Models;

namespace CloudStorage.WebApi.DTOs;

public record ReturnHistoryDto(int Id, ActionType Type, ReturnFileDto? File, ReturnDirDto? Dir, string Text, DateTime Date);