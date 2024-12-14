namespace CloudStorage.WebApi.DTOs;

public record UserDto(string Token, bool IsAdmin, bool IsBan);