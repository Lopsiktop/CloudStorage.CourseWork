using CloudStorage.Data.Contexts;
using CloudStorage.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloudStorage.WebApi.Controllers;

public class BaseApiController : ControllerBase
{
    protected int? GetIdByJwt()
    {
        var id = User.Claims.FirstOrDefault(x => x.Type.Contains("nameidentifier"))?.Value;
        if (id == null)
            return null;

        return int.Parse(id);
    }

    protected async Task<int> GetRootDirId(int? dirId, CloudStorageContext context)
    {
        var dir = await context.Directories.FindAsync(dirId);
        if (dir.ParentId != null)
            return await GetRootDirId(dir.ParentId, context);
        else
            return dir.Id;
    }

    protected async Task<string> GetPath(int? dirId, CloudStorageContext context)
    {
        if (dirId == null)
            return "";
        var dir = await context.Directories.FindAsync(dirId);
        if (dir == null)
            return "";

        return Path.Combine(await GetPath(dir.ParentId, context), dir.Name);
    }

    protected async Task AddHistoryAction(User user, ActionType type, CloudStorageContext context, string text, int? dirId = null, int? fileId = null)
    {
        var history = new History()
        {
            UserId = user.Id,
            DirectoryId = dirId,
            FileId = fileId,
            ActionType = (int)type,
            Text = text,
            Date = DateTime.Now
        };

        await context.Histories.AddAsync(history);
        await context.SaveChangesAsync();
    }
}
