using System.ComponentModel.DataAnnotations;

namespace CloudStorage.WebApi.DTOs;

public class LoginDto
{
    [Required, MinLength(4)]
    public string Login { get; set; }
    [Required, MinLength(4)]
    public string Password { get; set; }    
}
