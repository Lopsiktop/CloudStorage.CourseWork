using CloudStorage.Data.Contexts;
using CloudStorage.Data.Models;
using CloudStorage.WebApi.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

    private string RandomString(int length)
    {
        var random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    [HttpPost("CreateFileLink")]
    [Authorize]
    public async Task<IActionResult> CreateFileLink(int fileId)
    {
        var userId = GetIdByJwt();

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return BadRequest("Ошибка авторизации");

        var file = await _context.Files.FindAsync(fileId);
        if (file == null)
            return BadRequest("Нет такого файла");

        var root = await GetRootDirId(file.DirectoryId, _context);
        if (root != user.RootDirId)
            return BadRequest("Недостаточно прав");

        var exist = await _context.Links.FirstOrDefaultAsync(x => x.FileId == file.Id);
        if (exist != null)
            return BadRequest("Ссылка на данный файл уже существует!");

        var link = new Link
        {
            FileId = file.Id,
            Code = RandomString(10),
        };

        await _context.Links.AddAsync(link);
        await _context.SaveChangesAsync();

        var baseUrl = Environment.GetEnvironmentVariable("ASPNETCORE_URLS")!.Split(";").First();
        var url = baseUrl + $"/api/Link/Page/{link.Code}";

        return Ok(url);
    }

    [HttpPost("CreateFolderLink")]
    [Authorize]
    public async Task<IActionResult> CreateFolderLink(int dirId)
    {
        var userId = GetIdByJwt();

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return BadRequest("Ошибка авторизации");

        var dir = await _context.Directories.FindAsync(dirId);
        if (dir == null)
            return BadRequest("Нет такой папки");

        var root = await GetRootDirId(dirId, _context);
        if (root != user.RootDirId)
            return BadRequest("Недостаточно прав");

        var exist = await _context.Links.FirstOrDefaultAsync(x => x.DirId == dirId);
        if (exist != null)
            return BadRequest("Ссылка на данную папку уже существует!");

        var link = new Link
        {
            DirId = dirId,
            Code = RandomString(10),
        };

        await _context.Links.AddAsync(link);
        await _context.SaveChangesAsync();

        var baseUrl = Environment.GetEnvironmentVariable("ASPNETCORE_URLS")!.Split(";").First();
        var url = baseUrl + $"/api/Link/Page/{link.Code}";

        return Ok(url);
    }

    [HttpGet("Page/{code}")]
    public async Task<IActionResult> GetPageForDownload(string code)
    {
        var link = await _context.Links.FirstOrDefaultAsync(x => x.Code == code);
        if (link == null)
            return BadRequest("Неверная ссылка");

        var header = "Не удалось найти файл";

        if (link.FileId != null)
        {
            var file = await _context.Files.FindAsync(link.FileId);
            if (file != null)
                header = "Файл " + file.Name;
        }
        else if (link.DirId != null)
        {
            var dir = await _context.Directories.FindAsync(link.DirId);
            if (dir != null)
                header = "Папка " + dir.Name;
        }

        var html = "<!DOCTYPE html>\r\n<html lang=\"en\">\r\n    <head>\r\n        <meta charset=\"UTF-8\" />\r\n        <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\" />\r\n        <title>Download File</title>\r\n        <style>\r\n            body {\r\n                font-family: Arial, sans-serif;\r\n                background-color: #f4f4f4;\r\n                display: flex;\r\n                justify-content: center;\r\n                align-items: center;\r\n                height: 100vh;\r\n                margin: 0;\r\n            }\r\n            .container {\r\n                text-align: center;\r\n                background-color: #ffffff;\r\n                padding: 20px;\r\n                border-radius: 8px;\r\n                box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);\r\n            }\r\n            h1 {\r\n                margin-bottom: 20px;\r\n                word-break: break-all;\r\n                max-width: 500px;\r\n            }\r\n            a.download-button {\r\n                display: inline-block;\r\n                padding: 10px 20px;\r\n                background-color: #007bff;\r\n                color: white;\r\n                text-decoration: none;\r\n                border-radius: 5px;\r\n                transition: background-color 0.3s ease;\r\n            }\r\n            a.download-button:hover {\r\n                background-color: #0056b3;\r\n            }\r\n        </style>\r\n    </head>\r\n    <body>\r\n        <div class=\"container\">\r\n            <h1>\r\n[header]\r\n</h1>\r\n            <a\r\n                href=\"http://127.0.0.1:5044/api/Link/[code]\"\r\n                class=\"download-button\"\r\n                >Скачать</a\r\n            >\r\n        </div>\r\n    </body>\r\n</html>\r\n";
        html = html.Replace("[header]", header);
        html = html.Replace("[code]", code);

        return Content(html, "text/html");
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> DownloadByCode(string code)
    {
        var link = await _context.Links.FirstOrDefaultAsync(x => x.Code == code);
        if (link == null)
            return BadRequest("Неверная ссылка");

        if (link.FileId != null)
        {
            var file = await _context.Files.FindAsync(link.FileId);
            if (file == null)
                return BadRequest("Файл не существует");

            var dirPath = await GetPath(file.DirectoryId, _context);
            var filePath = CloudProvider.GetFilePath(dirPath, file.Name);

            var stream = System.IO.File.OpenRead(filePath);

            return File(stream, "application/octet-stream", file.Name);
        } 
        else if (link.DirId != null)
        {
            var dir = await _context.Directories.FindAsync(link.DirId);
            if (dir == null)
                return BadRequest("Папка не существует");

            var dirPath = await GetPath(dir.Id, _context);
            var folderPath = CloudProvider.GetFolderPath(dirPath);
            var archive = CloudProvider.CreateArchive(folderPath);

            var stream = System.IO.File.OpenRead(archive);

            return File(stream, "application/octet-stream", dir.Name + ".zip");
        }

        return Ok(code);
    }
}
