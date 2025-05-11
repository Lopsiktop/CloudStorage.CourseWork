using CloudStorage.Data.Contexts;
using CloudStorage.Data.Models;
using CloudStorage.WebApi.DTOs;
using CloudStorage.WebApi.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Directory = CloudStorage.Data.Models.Directory;
using File = CloudStorage.Data.Models.File;

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
            var name = CloudProvider.MoveDirectoryToBin(user.TrashDir!.Name, path);

            dir.TrashName = name;
            dir.OldDirId = dir.ParentId;
            dir.ParentId = user.TrashDirId;
            await _context.SaveChangesAsync();

            await AddHistoryAction(user, ActionType.MovedToTrash, _context, $"Папка \"{dir.Name}\" была удалена в корзину", dirId: dir.Id);
            return NoContent();
        }

        [HttpPost("Refresh/{dirId}")]
        public async Task<IActionResult> ReturnFromBin(int dirId)
        {
            var dir = await _context.Directories.FindAsync(dirId);
            if (dir == null)
                return BadRequest("Данная папка несуществует");

            var user = await _context.Users.Include(x => x.TrashDir).FirstOrDefaultAsync(x => x.Id == GetIdByJwt());
            if (user == null)
                return BadRequest("Ошибка авторизации");

            var rootId = await GetRootDirId(dirId, _context);
            if (rootId != user.RootDirId && rootId != user.TrashDirId)
                return BadRequest("Вы не можете использовать чужую папку");

            var dirPath = await GetPath(dir.Id, _context);
            dirPath = Path.Combine(dirPath.Remove(dirPath.Length - dir.Name.Length, dir.Name.Length), dir.TrashName);
            var path = CloudProvider.GetFolderPath(dirPath);

            var returnDirPath = await GetPath(dir.OldDirId, _context);
            var returnPath = Path.Combine(CloudProvider.GetFolderPath(returnDirPath), dir.Name);

            //if the folder that contained this folder is deleted now, then we should restore this folder to root folder
            var checkRootId = await GetRootDirId(dir.OldDirId, _context);
            if(checkRootId == user.TrashDirId)
            {
                returnDirPath = await GetPath(user.RootDirId, _context);
                returnPath = Path.Combine(CloudProvider.GetFolderPath(returnDirPath), dir.Name);
            }   

            var name = CloudProvider.ReturnDirectoryFromBin(path, returnPath);

            if (name != null)
                dir.Name = name;

            dir.TrashName = null;

            if (checkRootId == user.TrashDirId)
                dir.ParentId = user.RootDirId;
            else
                dir.ParentId = dir.OldDirId ?? 0;

            dir.OldDirId = null;
            await _context.SaveChangesAsync();

            await AddHistoryAction(user, ActionType.Restored, _context, $"Папка \"{dir.Name}\" была восстановлена из корзины", dirId: dir.Id);
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

            await AddHistoryAction(user, ActionType.Downloaded, _context, $"Папка \"{dir.Name}\" была скачана", dirId: dir.Id);
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

            var oldDir = dir.Name;
            dir.Name = model.Name;
            await _context.SaveChangesAsync();

            await AddHistoryAction(user, ActionType.Renamed, _context, $"Папка \"{oldDir}\" была переименована на \"{dir.Name}\"", dirId: dir.Id);
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
            if (rootId != user.RootDirId && rootId != user.TrashDirId)
                return BadRequest("Вы не можете использовать чужую папку");

            var path = await GetPath(dirId, _context);
            path = Path.Combine(path.Remove(path.Length - dir.Name.Length, dir.Name.Length), dir.TrashName);
            CloudProvider.DeleteDirectory(path);

            await DeleteChildren(dirId);

            _context.Directories.Remove(dir);
            await _context.SaveChangesAsync();

            await AddHistoryAction(user, ActionType.DeletedForever, _context, $"Папка \"{dir.Name}\" была удалена навсегда");
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
            if (root == null)
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

            await AddHistoryAction(user, ActionType.Created, _context, $"Папка \"{dir.Name}\" была создана", dirId: dir.Id);
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

        [HttpPost("Archive/{dirId}"), Authorize]
        public async Task<IActionResult> ArchiveFolder(int dirId)
        {
            var dir = await _context.Directories.Include(x => x.Parent).FirstOrDefaultAsync(x => x.Id == dirId);
            if (dir == null)
                return BadRequest("Данная папка несуществует");

            var rootId = await GetRootDirId(dir.Id, _context);
            var user = await _context.Users.FindAsync(GetIdByJwt());
            if (user.RootDirId != rootId && user.TrashDirId != rootId)
                return BadRequest("Данная папка не ваша");

            if (dir.Parent == null)
                return BadRequest("Данная операция невозможна");

            var dirPath = await GetPath(dir.Id, _context);
            var sourcePath = CloudProvider.GetFolderPath(dirPath);

            var archiveName = dir.Name + ".zip";
            var exists = await _context.Files.FirstOrDefaultAsync(x => x.DirectoryId == dir.ParentId && x.Name == archiveName);
            if (exists != null)
                return BadRequest($"Файл с названием \"{archiveName}\" уже существует!");

            //todo: check if file name already exists
            var destPath = await GetPath(dir.ParentId, _context);
            var destinationPath = CloudProvider.GetFolderPath(destPath);
            var archivePath = CloudProvider.GetFilePath(destinationPath, archiveName);

            var readyArchivePath = CloudProvider.CreateArchive(sourcePath, archivePath);
            
            var dbFile = new File { Name = archiveName, DirectoryId = (int)dir.ParentId! };

            await _context.Files.AddAsync(dbFile);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("Properties/{dirId}"), Authorize]
        public async Task<IActionResult> GetProperties(int dirId)
        {
            var dir = await _context.Directories.FirstOrDefaultAsync(x => x.Id == dirId);
            if (dir == null)
                return BadRequest("Данная папка несуществует");

            var rootId = await GetRootDirId(dir.Id, _context);
            var user = await _context.Users.FindAsync(GetIdByJwt());
            if (user.RootDirId != rootId && user.TrashDirId != rootId)
                return BadRequest("Данная папка не ваша");

            var dirPath = await GetPath(dir.Id, _context);
            var path = CloudProvider.GetFolderPath(dirPath);

            var creation = System.IO.Directory.GetCreationTime(path);
            var files = await _context.Files.Where(x => x.DirectoryId == dirId).CountAsync();
            var dirs = await _context.Directories.Where(x => x.ParentId == dirId).CountAsync();

            var size = await GetSize(dirId);

            return Ok(new FolderProperties(dirPath, creation, size, dirs, files));
        }

        private async Task<decimal> GetSize(int dirId)
        {
            decimal size = 0;
            var dir = await _context.Directories.Include(x => x.FileDirectories).FirstOrDefaultAsync(x => x.Id == dirId);

            foreach (var item in dir.FileDirectories)
                size += item.Size;

            var child = await _context.Directories.Where(x => x.ParentId == dirId).Select(x => x.Id).ToListAsync();
            foreach (var item in child)
                size += await GetSize(item);

            return size;
        }
    }
}