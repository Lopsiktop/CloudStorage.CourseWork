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
                .ThenInclude(x => x.Files)
                .Include(x => x.RootDir).ThenInclude(x => x.InverseParent)
                .AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

            return Ok(
                new ReturnUserDto(user.StorageVolume, 
                    new ReturnRootDirDto(
                        user.RootDir.Id, 
                        user.RootDir.Name,
                        user.RootDir.Files.Select(x => new ReturnFileDto(x.Id, x.Name, x.Size)), 
                        user.RootDir.InverseParent.Select(x => new ReturnDirDto(x.Id, x.Name))
                    )
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

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDto model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Login == model.Login);
            if (user == null)
                return BadRequest("Wrong login or password");

            if (!user.Verify(model.Password))
                return BadRequest("Wrong login or password");

            var token = _jwt.CreateToken(user);
            return Ok(token);
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult Valid() => Ok("Valid");
    }
}
