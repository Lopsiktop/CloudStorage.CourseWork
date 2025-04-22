using CloudStorage.Data.Contexts;
using Microsoft.AspNetCore.Mvc;

namespace CloudStorage.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FilterController : BaseApiController
{
    private readonly CloudStorageContext _context;

    public FilterController(CloudStorageContext context)
    {
        _context = context;
    }
}
