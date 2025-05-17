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
    public class UserController : BaseApiController
    {
        private readonly CloudStorageContext _context;
        private readonly JwtProvider _jwt;

        public UserController(CloudStorageContext context, JwtProvider jwt)
        {
            _context = context;
            _jwt = jwt;
        }

        [HttpGet("Me"), Authorize]
        public async Task<IActionResult> GetMe()
        {
            var id = GetIdByJwt();
            if (id == null)
                return Unauthorized();

            var user = await _context.Users.Include(x => x.RootDir)
                .ThenInclude(x => x.FileDirectories)
                .Include(x => x.RootDir).ThenInclude(x => x.InverseParent)
                .Include(x => x.TrashDir)
                .AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

            return Ok(
                new ReturnUserDto(0, 
                    new ReturnRootDirDto(
                        user.RootDir.Id, 
                        user.RootDir.Name,
                        user.RootDir.FileDirectories.Select(x => new ReturnFileDto(x.Id, x.Name, x.Size, x.CreationTime ?? DateTime.MinValue)), 
                        user.RootDir.InverseParent.Select(x => new ReturnDirDto(x.Id, x.Name, x.CreationTime ?? DateTime.MinValue))
                    ),
                    new ReturnDirDto(user.TrashDir.Id, user.TrashDir.Name, user.TrashDir.CreationTime ?? DateTime.MinValue)
                )
            );
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
                IsBan = 0,
                IsAdmin = 0
            };
            user.SetPassword(model.Password);

            var dir = new Directory
            {
                Name = $"Root_{user.Login}"
            };

            var trash = new Directory
            {
                Name = $"Trash_{user.Login}"
            };

            user.RootDir = dir;
            user.TrashDir = trash;

            CloudProvider.CreateUserDir(user.Login);
            CloudProvider.CreateTrashDir(user.Login);

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var token = _jwt.CreateToken(user);
            return Ok(new UserDto(token, user.IsAdmin == 1, user.IsBan == 1));
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDto model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Login == model.Login);
            if (user == null)
                return BadRequest("Wrong login or password");

            if (!user.Verify(model.Password))
                return BadRequest("Wrong login or password");

            var token = _jwt.CreateToken(user);
            return Ok(new UserDto(token, user.IsAdmin == 1, user.IsBan == 1));
        }

        [HttpGet("Ban"), Authorize]
        public async Task<IActionResult> CheckBan()
        {
            var user = await _context.Users.FindAsync(GetIdByJwt());
            return Ok(new UserDto("", user.IsAdmin == 1, user.IsBan == 1));
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult Valid() => Ok("Valid");
    }
}
