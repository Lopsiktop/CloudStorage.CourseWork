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
[Authorize]
public class FilterController : BaseApiController
{
    private readonly CloudStorageContext _context;

    public FilterController(CloudStorageContext context)
    {
        _context = context;
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

    [HttpPost("SortFolderBySize")]
    public async Task<IActionResult> SortFoldersBySize(SizeDto dto)
    {
        var userId = GetIdByJwt();

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return BadRequest("Ошибка авторизации");

        var dirs = await _context.Directories.Where(x => dto.DirIds.Contains(x.Id)).ToListAsync();
        var list = new List<ReturnDirSizeDto>();

        foreach (var dir in dirs)
        {
            var size = await GetSize(dir.Id);
            list.Add(new ReturnDirSizeDto(dir.Id, dir.Name, size));
        }

        return Ok(list.OrderBy(x => x.Size).ToList());
    }

    [HttpPost("SortByDate")]
    public async Task<IActionResult> SortByDate(SortDto dto)
    {
        var userId = GetIdByJwt();

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return BadRequest("Ошибка авторизации");

        var dirs = await _context.Directories.Where(x => dto.DirIds.Contains(x.Id)).ToListAsync();
        var files = await _context.Files.Where(x => dto.FileIds.Contains(x.Id)).ToListAsync();

        var sortedDirs = new List<ReturnDirDateDto>();
        var sortedFiles = new List<ReturnFileDateDto>();

        foreach (var item in dirs)
        {
            var creation = item.CreationTime ?? DateTime.MinValue;
            sortedDirs.Add(new ReturnDirDateDto(item.Id, item.Name, creation));
        }

        foreach (var item in files)
        {
            var creation = item.CreationTime ?? DateTime.MinValue;
            sortedFiles.Add(new ReturnFileDateDto(item.Id, item.Name, item.Size, creation));
        }

        return Ok(new FilterReturnDateDto(
            sortedFiles.OrderBy(x => x.CreationTime).ToList(),
            sortedDirs.OrderBy(x => x.CreationTime).ToList()
            ));
    }

    [HttpGet("SearchByField")]
    public async Task<IActionResult> SearchByField(string searchField, int searchType, int dirId)
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

        var sortedDirs = dirs.Where(x => x.Name.ToLower().Contains(searchField.ToLower()))
                .Select(x => new ReturnDirDto(x.Id, x.Name)).ToList();

        var sortedFiles = files.Where(x => x.Name.ToLower().Contains(searchField.ToLower()))
            .Select(x => new ReturnFileDto(x.Id, x.Name, x.Size)).ToList();

        return Ok(new FilterReturnDto(sortedFiles, sortedDirs));
    }

    [HttpGet("SearchByDates")]
    public async Task<IActionResult> SearchByDates(string fromDate, string toDate, int dirId)
    {
        if (string.IsNullOrWhiteSpace(fromDate))
            return BadRequest("Строка даты");

        if (string.IsNullOrWhiteSpace(toDate))
            return BadRequest("Строка даты");

        var from = DateTime.Parse(fromDate);
        var to = DateTime.Parse(toDate);
        to = to.AddHours(23);
        to = to.AddMinutes(59);
        to = to.AddSeconds(59);

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

        var sortedDirs = new List<ReturnDirDto>();

        foreach (var item in dirs)
        {
            var creation = item.CreationTime ?? DateTime.MinValue;
            if (creation >= from && creation <= to)
                sortedDirs.Add(new ReturnDirDto(item.Id, item.Name));
        }

        var sortedFiles = new List<ReturnFileDto>();

        foreach (var item in files)
        {
            var creation = item.CreationTime ?? DateTime.MinValue;
            if (creation >= from && creation <= to)
                sortedFiles.Add(new ReturnFileDto(item.Id, item.Name, item.Size));
        }

        return Ok(new FilterReturnDto(sortedFiles, sortedDirs));
    }

    private async Task<List<Directory>> _GetAllDirs(int dirId)
    {
        var dirs = await _context.Directories.Where(x => x.ParentId == dirId).ToListAsync();

        if (dirs.Count == 0)
            return dirs;

        foreach (var dir in dirs.ToList())
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
