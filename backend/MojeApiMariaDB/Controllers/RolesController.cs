using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MojeApiMariaDB.Data;
using MojeApiMariaDB.Models;

namespace MojeApiMariaDB.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RolesController : ControllerBase
{
    private readonly AppDbContext _context;

    public RolesController(AppDbContext context)
    {
        _context = context;
    }

    // Pozwalamy tylko na pobieranie (Read-Only)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Role>>> GetRoles()
    {
        return await _context.Roles.ToListAsync();
    }
}