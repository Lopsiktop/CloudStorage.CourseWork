using CloudStorage.Data.Contexts;
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

    [HttpPost, Authorize]
    public async Task<IActionResult> LoadFile([FromForm] FilesDto model)
    {
        var user = await _context.Users.FindAsync(GetIdByJwt());
        if (user == null)
            return BadRequest();

        var rootId = await GetRootDirId(model.DirId, _context);
        if (user.RootDirId != rootId)
            return BadRequest("Данная папка не ваша");

        //todo: limit size of one file
        //todo: check disk space
        //todo: check if file exists with this name

        var dir = await _context.Directories.FindAsync(model.DirId);
        if (dir == null)
            return BadRequest("Данной папки не существует");

        var dirPath = await GetPath(model.DirId, _context);
        var path = await CloudProvider.LoadFileAsync(dirPath, model.File);
        if (path != null)
        {
            var file = new File { Directory = dir, Name = model.File.FileName, Path = path, Size = model.File.Length };
            
            await _context.Files.AddAsync(file);
            await _context.SaveChangesAsync();

            return Ok(new ReturnFileDto(file.Id, file.Name, file.Size));
        }

        return BadRequest("Не удалось загрузить файл");
    }

    [HttpGet("GetFilesByDirId/{dirId}"), Authorize]
    public async Task<IActionResult> GetFilesByDirId(int dirId)
    {
        var dir = await _context.Directories.Include(x => x.Files).FirstOrDefaultAsync(x => x.Id == dirId);
        if (dir == null)
            return BadRequest("Данная папка несуществует");

        var rootId = await GetRootDirId(dirId, _context);
        var user = await _context.Users.FindAsync(GetIdByJwt());
        if (user.RootDirId != rootId)
            return BadRequest("Данная папка не ваша");

        var files = dir.Files.Select(x => new ReturnFileDto(x.Id, x.Name, x.Size));
        return Ok(files);
    }
}
