using CloudStorage.Data.Contexts;
using CloudStorage.Data.Models;
using CloudStorage.WebApi.DTOs;
using CloudStorage.WebApi.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Directory = CloudStorage.Data.Models.Directory;

namespace CloudStorage.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly CloudStorageContext _context;
        private readonly JwtProvider _jwt;

        public UserController(CloudStorageContext context, JwtProvider jwt)
        {
            _context = context;
            _jwt = jwt;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(LoginDto model)
        {
            var exists = await _context.Users.FirstOrDefaultAsync(x => x.Login == model.Login);
            if (exists != null)
                return BadRequest("This login is busy");

            var user = new User
            {
                Login = model.Login,
                StorageVolume = 10,
            };
            user.SetPassword(model.Password);

            var dir = new Directory
            {
                Name = $"Root_{user.Login}"
            };

            user.RootDir = dir;

            CloudProvider.CreateUserDir(user.Login);

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var token = _jwt.CreateToken(user);
            return Ok(token);
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult Valid() => Ok("Valid");
    }
}
