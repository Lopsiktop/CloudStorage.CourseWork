using CloudStorage.Data.Contexts;
using CloudStorage.WebApi.DTOs;
using CloudStorage.WebApi.Utils;
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

        [HttpGet("GetStructure"), Authorize]
        public async Task<IActionResult> GetAllFolders()
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == GetIdByJwt());
            var dirs = await GetAllDirectories(user.RootDirId);

            return Ok(dirs);
        }

        protected async Task<List<DirStructureDto>> GetAllDirectories(int dirId)
        {
            var dir = await _context.Directories.FindAsync(dirId);
            var dirs = await _context.Directories.Where(x => x.ParentId == dir.Id).ToListAsync();
            var list = new List<DirStructureDto>();

            foreach (var d in dirs)
            {
                list.Add(new DirStructureDto(d.Id, d.Name, await GetAllDirectories(d.Id)));
            }

            return list;
        }

        protected async Task<string> GetPath(int? dirId)
        {
            if (dirId == null)
                return "";
            var dir = await _context.Directories.FindAsync(dirId);
            if (dir == null)
                return "";

            return Path.Combine(await GetPath(dir.ParentId), dir.Name);
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

            var path = await GetPath(dir.Id);
            var created = CloudProvider.CreateFolder(path);
            if (!created)
                return BadRequest("Не удалось создать папку");

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