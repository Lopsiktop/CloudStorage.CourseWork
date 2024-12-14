using CloudStorage.Data.Contexts;
using CloudStorage.WebApi.DTOs;
using CloudStorage.WebApi.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloudStorage.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = "Admin")]
public class AdminController : BaseApiController
{
    private readonly CloudStorageContext _context;
    private readonly JwtProvider _jwt;

    public AdminController(CloudStorageContext context, JwtProvider jwt)
    {
        _context = context;
        _jwt = jwt;
    }

    [HttpGet("Users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.Users.Select(x => new AdminUserDto(x.Id, x.Login, x.IsAdmin == 1, x.IsBan == 1)).ToListAsync();
        return Ok(users);
    }

    [HttpGet("User/{id}")]
    public async Task<IActionResult> GetUserToken(int id)
    {
        var user = await _context.Users.FindAsync(id);
        var token = _jwt.CreateToken(user);
        return Ok(token);
    }

    [HttpGet]
    public IActionResult Valid() => Ok("Valid Admin");
}
