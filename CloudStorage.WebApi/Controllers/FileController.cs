using CloudStorage.Data.Contexts;
using CloudStorage.WebApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Directory = CloudStorage.Data.Models.Directory;

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
