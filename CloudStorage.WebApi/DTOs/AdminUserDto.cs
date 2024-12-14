namespace CloudStorage.WebApi.DTOs;

public record AdminUserDto(int Id, string Login, bool IsAdmin, bool IsBan);