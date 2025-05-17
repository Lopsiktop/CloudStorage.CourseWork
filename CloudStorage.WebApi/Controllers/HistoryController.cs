using CloudStorage.Data.Contexts;
using CloudStorage.Data.Models;
using CloudStorage.WebApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloudStorage.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HistoryController : BaseApiController
{
    private readonly CloudStorageContext _context;

    public HistoryController(CloudStorageContext context)
    {
        _context = context;
    }

    [HttpGet("All"), Authorize]
    public async Task<IActionResult> GetAllHistory()
    {
        var user = await _context.Users.FindAsync(GetIdByJwt());
        if (user == null)
            return BadRequest();

        var history = await _context.Histories.Where(x => x.UserId == user.Id).Include(x => x.File).Include(x => x.Directory).OrderByDescending(x => x.Id)
            .Select(x => new ReturnHistoryDto(x.Id, (ActionType)x.ActionType, x.File != null ? new ReturnFileDto(x.File.Id, x.File.Name, x.File.Size, x.File.CreationTime ?? DateTime.MinValue) : null,
            x.Directory != null ? new ReturnDirDto(x.Directory.Id, x.Directory.Name, x.Directory.CreationTime ?? DateTime.MinValue) : null, x.Text, x.Date)).ToListAsync();

        return Ok(history);
    }

    [HttpGet("File/{id}"), Authorize]
    public async Task<IActionResult> GetFileHistory(int id)
    {
        var user = await _context.Users.FindAsync(GetIdByJwt());
        if (user == null)
            return BadRequest();

        var history = await _context.Histories.Where(x => x.UserId == user.Id && x.FileId == id)
            .Include(x => x.File).OrderByDescending(x => x.Id)
            .Select(x => new ReturnHistoryDto(x.Id, (ActionType)x.ActionType, new ReturnFileDto(x.File.Id, x.File.Name, x.File.Size, x.File.CreationTime ?? DateTime.MinValue),
            null, x.Text, x.Date)).ToListAsync();

        return Ok(history);
    }

    [HttpGet("Directory/{id}"), Authorize]
    public async Task<IActionResult> GetDirHistory(int id)
    {
        var user = await _context.Users.FindAsync(GetIdByJwt());
        if (user == null)
            return BadRequest();

        var history = await _context.Histories.Where(x => x.UserId == user.Id && x.DirectoryId == id)
            .Include(x => x.Directory).OrderByDescending(x => x.Id)
            .Select(x => new ReturnHistoryDto(x.Id, (ActionType)x.ActionType, null,
            new ReturnDirDto(x.Directory.Id, x.Directory.Name, x.Directory.CreationTime ?? DateTime.MinValue), x.Text, x.Date)).ToListAsync();

        return Ok(history);
    }
}
