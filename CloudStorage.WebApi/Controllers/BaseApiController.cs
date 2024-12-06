using CloudStorage.Data.Contexts;
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
}
