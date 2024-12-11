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

        [HttpPost("Bin/{dirId}")] 
        public async Task<IActionResult> MoveToBin(int dirId)
        {
            var dir = await _context.Directories.FindAsync(dirId);
            if (dir == null)
                return BadRequest("Данная папка несуществует");

            var user = await _context.Users.Include(x => x.TrashDir).FirstOrDefaultAsync(x => x.Id == GetIdByJwt());
            if (user == null)
                return BadRequest("Ошибка авторизации");

            var rootId = await GetRootDirId(dirId, _context);
            if (rootId != user.RootDirId)
                return BadRequest("Вы не можете использовать чужую папку");

            var dirPath = await GetPath(dir.Id, _context);
            var path = CloudProvider.GetFolderPath(dirPath);
            CloudProvider.MoveDirectoryToBin(user.TrashDir!.Name, path);

            dir.OldDirId = dir.ParentId;
            dir.ParentId = user.TrashDirId;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("Download/{id}")]
        public async Task<IActionResult> DownloadDirectory(int id)
        {
            var user = await _context.Users.FindAsync(GetIdByJwt());
            if (user == null)
                return BadRequest();

            var dir = await _context.Directories.FindAsync(id);
            if (dir == null)
                return BadRequest();

            var rootId = await GetRootDirId(dir.Id, _context);
            if (user.RootDirId != rootId)
                return BadRequest("Данный файл не ваш");

            var dirPath = await GetPath(dir.Id, _context);
            var folderPath = CloudProvider.GetFolderPath(dirPath);
            var archive = CloudProvider.CreateArchive(folderPath);

            var stream = System.IO.File.OpenRead(archive);

            return File(stream, "application/octet-stream", dir.Name + ".zip");
        }

        [HttpPost("Rename"), Authorize]
        public async Task<IActionResult> RenameDirectory(RenameDirectoryDto model)
        {
            var user = await _context.Users.FindAsync(GetIdByJwt());
            if (user == null)
                return BadRequest();

            var dir = await _context.Directories.FindAsync(model.DirId);
            if (dir == null)
                return BadRequest();

            var rootId = await GetRootDirId(dir.Id, _context);
            if (user.RootDirId != rootId)
                return BadRequest("Данный файл не ваш");

            if (dir.Name == model.Name)
                return BadRequest("Вы не можете поменять имя на то что уже стоит");

            var path = await GetPath(dir.Id, _context);

            try
            {
                CloudProvider.RenameDirectory(path, model.Name);
            }
            catch
            {
                return BadRequest("Неправильное название файла");
            }

            dir.Name = model.Name;
            await _context.SaveChangesAsync();

            return Ok(new ReturnDirDto(dir.Id, dir.Name));
        }

        [HttpDelete("{dirId}"), Authorize]
        public async Task<IActionResult> DeleteDirectory(int dirId)
        {
            var dir = await _context.Directories.FindAsync(dirId);
            if (dir == null)
                return BadRequest("Данная папка несуществует");

            var user = await _context.Users.FindAsync(GetIdByJwt());
            if (user == null)
                return BadRequest("Ошибка авторизации");

            var rootId = await GetRootDirId(dirId, _context);
            if (rootId != user.RootDirId)
                return BadRequest("Вы не можете использовать чужую папку");

            var path = await GetPath(dirId, _context);
            CloudProvider.DeleteDirectory(path);

            _context.Directories.Remove(dir);
            await _context.SaveChangesAsync();

            await DeleteChildren(dirId);

            return NoContent();
        }

        private async Task DeleteChildren(int dirId)
        {
            var children = await _context.Directories.Where(x => x.ParentId == dirId).ToListAsync();
            _context.Directories.RemoveRange(children);
            await _context.SaveChangesAsync();

            foreach (var child in children)
                await DeleteChildren(child.Id);
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

            var path = await GetPath(dir.Id, _context);
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
            if (user.RootDirId != rootId && user.TrashDirId != rootId)
                return BadRequest("Данная папка не ваша");

            var dirs = dir.InverseParent.Select(x => new ReturnDirDto(x.Id, x.Name));
            return Ok(dirs);
        }
    }
}