using CloudStorage.Data.Contexts;
using CloudStorage.WebApi.Utils;
using Microsoft.AspNetCore.Mvc;

namespace CloudStorage.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LinkController : BaseApiController
{
    private readonly CloudStorageContext _context;

    public LinkController(CloudStorageContext context)
    {
        _context = context;
    }

    [HttpGet("TestFile")]
    public async Task<IActionResult> UploadFile()
    {
        var dirPath = await GetPath(1, _context);
        var filePath = CloudProvider.GetFilePath(dirPath, "Язык_программирования_C_Брайан_У_Керниган,_Деннис_М_Ритчи_Z_Library.pdf");

        var stream = System.IO.File.OpenRead(filePath);

        return File(stream, "application/octet-stream", "Язык_программирования_C_Брайан_У_Керниган,_Деннис_М_Ритчи_Z_Library.pdf");
    }
}
