using CloudStorage.Data.Contexts;
using CloudStorage.WebApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Directory = CloudStorage.Data.Models.Directory;
using File = CloudStorage.Data.Models.File;

namespace CloudStorage.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class FilterController : BaseApiController
{
    private readonly CloudStorageContext _context;

    public FilterController(CloudStorageContext context)
    {
        _context = context;
    }

    [HttpGet("Search")]
    public async Task<IActionResult> Search(string searchField, int searchType, int dirId)
    {
        if (string.IsNullOrWhiteSpace(searchField))
            return BadRequest("Строка поиска не может быть пустой");

        var userId = GetIdByJwt();

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return BadRequest("Ошибка авторизации");

        var dir = await _context.Directories.FindAsync(dirId);
        if (dir == null)
            return BadRequest("Нет такой папки");

        var root = await GetRootDirId(dir.Id, _context);
        if (root != user.RootDirId)
            return BadRequest("Недостаточно прав");

        var dirs = await _GetAllDirs(dir.Id);
        var files = await _GetAllFiles(dirs, dir.Id);

        if (searchType == 0)
        {
            var sortedDirs = dirs.Where(x => x.Name.ToLower().Contains(searchField.ToLower()))
                .Select(x => new ReturnDirDto(x.Id, x.Name)).ToList();

            var sortedFiles = files.Where(x => x.Name.ToLower().Contains(searchField.ToLower()))
                .Select(x => new ReturnFileDto(x.Id, x.Name, x.Size)).ToList();

            return Ok(new FilterReturnDto(sortedFiles, sortedDirs));
        }

        return Ok();
    }

    private async Task<List<Directory>> _GetAllDirs(int dirId)
    {
        var dirs = await _context.Directories.Where(x => x.ParentId == dirId).ToListAsync();

        if (dirs.Count == 0)
            return dirs;

        foreach (var dir in dirs)
        {
            var newDirs = await _GetAllDirs(dir.Id);
            dirs.AddRange(newDirs);
        }

        return dirs;
    }

    private async Task<List<File>> _GetAllFiles(List<Directory> dirs, int rootDirId)
    {
        var files = new List<File>();

        foreach (var dir in dirs)
        {
            var dirFiles = await _context.Files.Where(x => x.DirectoryId == dir.Id).ToListAsync();
            files.AddRange(dirFiles);
        }

        var rootFiles = await _context.Files.Where(x => x.DirectoryId == rootDirId).ToListAsync();
        files.AddRange(rootFiles);

        return files;
    }
}
