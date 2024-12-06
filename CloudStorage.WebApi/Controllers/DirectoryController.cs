using CloudStorage.Data.Contexts;
using CloudStorage.WebApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Directory = CloudStorage.Data.Models.Directory;

namespace CloudStorage.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DirectoryController : BaseApiController
    {
        private readonly CloudStorageContext _context;

        public DirectoryController(CloudStorageContext context)
        {
            _context = context;
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> CreateDir(CreateDirDto model)
        {
            var root = await _context.Directories.FindAsync(model.RootDirId);
            if(root == null)
                return BadRequest("Данная папка несуществует");

            var rootId = await GetRootDirId(model.RootDirId, _context);
            var user = await _context.Users.FindAsync(GetIdByJwt());
            if (user.RootDirId != rootId)
                return BadRequest("Данная папка не является вашей");

            var exists = await _context.Directories.FirstOrDefaultAsync(x => x.ParentId == model.RootDirId && x.Name == model.Name);
            if (exists != null)
                return BadRequest("Данное название папки занято");

            var dir = new Directory { Name = model.Name, ParentId = model.RootDirId };
            
            await _context.Directories.AddAsync(dir);
            await _context.SaveChangesAsync();
            return Ok(new ReturnDirDto(dir.Id, dir.Name));
        }

        [HttpGet("GetDirsByDirId/{dirId}"), Authorize]
        public async Task<IActionResult> GetDirsByDir(int dirId)
        {
            var dir = await _context.Directories.Include(x => x.InverseParent).FirstOrDefaultAsync(x => x.Id == dirId);
            if (dir == null)
                return BadRequest("Данная папка несуществует");

            var rootId = await GetRootDirId(dirId, _context);
            var user = await _context.Users.FindAsync(GetIdByJwt());
            if (user.RootDirId != rootId)
                return BadRequest("Данная папка не ваша");

            var dirs = dir.InverseParent.Select(x => new ReturnDirDto(x.Id, x.Name));
            return Ok(dirs);
        }
    }
}