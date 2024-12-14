using CloudStorage.Data.Contexts;
using CloudStorage.Data.Models;
using CloudStorage.WebApi.DTOs;
using CloudStorage.WebApi.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Directory = CloudStorage.Data.Models.Directory;
using File = CloudStorage.Data.Models.File;

namespace CloudStorage.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FileController : BaseApiController
{
    private readonly CloudStorageContext _context;

    public FileController(CloudStorageContext context)
    {
        _context = context;
    }

    [HttpGet("Download/{id}")]
    public async Task<IActionResult> DownloadFile(int id)
    {
        var user = await _context.Users.FindAsync(GetIdByJwt());
        if (user == null)
            return BadRequest();

        var file = await _context.Files.FindAsync(id);
        if (file == null)
            return BadRequest();

        var rootId = await GetRootDirId(file.DirectoryId, _context);
        if (user.RootDirId != rootId)
            return BadRequest("Данный файл не ваш");

        var dirPath = await GetPath(file.DirectoryId, _context);
        var filePath = CloudProvider.GetFilePath(dirPath, file.Name);

        var stream = System.IO.File.OpenRead(filePath);

        await AddHistoryAction(user, ActionType.Downloaded, _context, $"Файл \"{file.Name}\" был скачан", fileId: file.Id);
        return File(stream, "application/octet-stream", file.Name);
    }

    [HttpPost("Rename"), Authorize]
    public async Task<IActionResult> RenameFile(RenameFileDto model)
    {
        var user = await _context.Users.FindAsync(GetIdByJwt());
        if (user == null)
            return BadRequest();

        var file = await _context.Files.FindAsync(model.FileId);
        if (file == null)
            return BadRequest();

        var rootId = await GetRootDirId(file.DirectoryId, _context);
        if (user.RootDirId != rootId)
            return BadRequest("Данный файл не ваш");

        if (file.Name == model.Name)
            return BadRequest("Вы не можете поменять имя на то что уже стоит");

        var path = await GetPath(file.DirectoryId, _context);
        
        try
        {
            CloudProvider.RenameFile(path, file.Name, model.Name);
        }
        catch
        {
            return BadRequest("Неправильное название файла");
        }

        var oldName = file.Name;
        file.Name = model.Name;
        await _context.SaveChangesAsync();

        await AddHistoryAction(user, ActionType.Renamed, _context, $"Файл \"{oldName}\" был переименован на \"{model.Name}\"", fileId: file.Id);
        return Ok(new ReturnFileDto(file.Id, file.Name, file.Size));
    }

    [HttpDelete("{fileId}"), Authorize]
    public async Task<IActionResult> DeleteFile(int fileId)
    {
        var file = await _context.Files.FindAsync(fileId);
        if (file == null)
            return BadRequest("Данный файл несуществует");

        var user = await _context.Users.FindAsync(GetIdByJwt());
        if(user == null)
            return BadRequest("Ошибка авторизации");

        var rootId = await GetRootDirId(file.DirectoryId, _context);
        if(rootId != user.RootDirId && rootId != user.TrashDirId)
            return BadRequest("Вы не можете использовать чужой файл");

        var path = await GetPath(file.DirectoryId, _context);
        CloudProvider.DeleteFile(path, file.TrashName);

        _context.Files.Remove(file);
        await _context.SaveChangesAsync();

        await AddHistoryAction(user, ActionType.DeletedForever, _context, $"Файл \"{file.Name}\" был удален навсегда");
        return NoContent();
    }

    [HttpPost, Authorize]
    public async Task<IActionResult> LoadFile([FromForm] FilesDto model)
    {
        var user = await _context.Users.FindAsync(GetIdByJwt());
        if (user == null)
            return BadRequest();

        var rootId = await GetRootDirId(model.DirId, _context);
        if (user.RootDirId != rootId)
            return BadRequest("Данная папка не ваша");

        //todo: limit size of one file (if it would need for course work or diplom)
        //todo: check disk space (if it would need for course work or diplom)

        var exists = await _context.Files.FirstOrDefaultAsync(x => x.DirectoryId == model.DirId && x.Name == model.File.FileName);
        if (exists != null)
            return BadRequest("Файл с таким названием уже существует");

        var dir = await _context.Directories.FindAsync(model.DirId);
        if (dir == null)
            return BadRequest("Данной папки не существует");

        var dirPath = await GetPath(model.DirId, _context);
        var path = await CloudProvider.LoadFileAsync(dirPath, model.File);
        if (path != null)
        {
            var file = new File { Directory = dir, Name = model.File.FileName, Size = model.File.Length };
            
            await _context.Files.AddAsync(file);
            await _context.SaveChangesAsync();

            await AddHistoryAction(user, ActionType.Added, _context, $"Файл \"{file.Name}\" был загружен на диск", fileId: file.Id);
            return Ok(new ReturnFileDto(file.Id, file.Name, file.Size));
        }

        return BadRequest("Не удалось загрузить файл");
    }

    [HttpGet("GetFilesByDirId/{dirId}"), Authorize]
    public async Task<IActionResult> GetFilesByDirId(int dirId)
    {
        var dir = await _context.Directories.Include(x => x.FileDirectories).FirstOrDefaultAsync(x => x.Id == dirId);
        if (dir == null)
            return BadRequest("Данная папка несуществует");

        var rootId = await GetRootDirId(dirId, _context);
        var user = await _context.Users.FindAsync(GetIdByJwt());
        if (user.RootDirId != rootId && user.TrashDirId != rootId)
            return BadRequest("Данная папка не ваша");

        var files = dir.FileDirectories.Select(x => new ReturnFileDto(x.Id, x.Name, x.Size));

        return Ok(files);
    }

    [HttpPost("Bin/{fileId}")]
    public async Task<IActionResult> MoveToBin(int fileId)
    {
        var file = await _context.Files.FindAsync(fileId);
        if (file == null)
            return BadRequest("Данный файл несуществует");

        var user = await _context.Users.Include(x => x.TrashDir).FirstOrDefaultAsync(x => x.Id == GetIdByJwt());
        if (user == null)
            return BadRequest("Ошибка авторизации");

        var rootId = await GetRootDirId(file.DirectoryId, _context);
        if (rootId != user.RootDirId)
            return BadRequest("Вы не можете использовать чужой файл");

        var dirPath = await GetPath(file.DirectoryId, _context);
        var path = CloudProvider.GetFilePath(dirPath, file.Name);
        var name = CloudProvider.MoveFileToBin(user.TrashDir!.Name, path);

        file.TrashName = name;
        file.OldDirId = file.DirectoryId;
        file.DirectoryId = user.TrashDirId;
        await _context.SaveChangesAsync();

        await AddHistoryAction(user, ActionType.MovedToTrash, _context, $"Файл \"{file.Name}\" был удален в корзину", fileId: file.Id);
        return NoContent();
    }

    [HttpPost("Refresh/{fileId}")]
    public async Task<IActionResult> RefreshFromBin(int fileId)
    {
        var file = await _context.Files.FindAsync(fileId);
        if (file == null)
            return BadRequest("Данный файл несуществует");

        var user = await _context.Users.Include(x => x.TrashDir).FirstOrDefaultAsync(x => x.Id == GetIdByJwt());
        if (user == null)
            return BadRequest("Ошибка авторизации");

        var rootId = await GetRootDirId(file.DirectoryId, _context);
        if (rootId != user.RootDirId && rootId != user.TrashDirId)
            return BadRequest("Вы не можете использовать чужой файл");

        if (file.TrashName == null || file.OldDirId == null)
            return BadRequest();

        var dirPath = await GetPath(file.DirectoryId, _context);
        var path = CloudProvider.GetFilePath(dirPath, file.TrashName);

        var returnDirPath = await GetPath(file.OldDirId, _context);
        var returnPath = CloudProvider.GetFilePath(returnDirPath, file.Name);

        //if the folder that contained this folder is deleted now, then we should restore this folder to root folder
        var checkRootId = await GetRootDirId(file.OldDirId, _context);
        if (checkRootId == user.TrashDirId)
        {
            returnDirPath = await GetPath(user.RootDirId, _context);
            returnPath = CloudProvider.GetFilePath(returnDirPath, file.Name);
        }

        var name = CloudProvider.ReturnFileFromBin(path, returnPath);

        if (name != null)
            file.Name = name;

        file.TrashName = null;

        if (checkRootId == user.TrashDirId)
            file.DirectoryId = user.RootDirId;
        else
            file.DirectoryId = file.OldDirId ?? 0;

        file.OldDirId = null;
        await _context.SaveChangesAsync();

        await AddHistoryAction(user, ActionType.Restored, _context, $"Файл \"{file.Name}\" был восстановлен из корзины", fileId: file.Id);
        return NoContent();
    }
}
