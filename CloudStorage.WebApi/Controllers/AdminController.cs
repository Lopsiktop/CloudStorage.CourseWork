using CloudStorage.Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloudStorage.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = "Admin")]
public class AdminController : BaseApiController
{
    private readonly CloudStorageContext _context;

    public AdminController(CloudStorageContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Valid() => Ok("Valid Admin");
}
