using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MojeApiMariaDB.Data;
using MojeApiMariaDB.Models;

namespace MojeApiMariaDB.Controllers;

[Authorize] // Tylko zalogowani mają dostęp do listy użytkowników
[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        // Zwracamy listę bez haseł dla bezpieczeństwa
        return await _context.Users.Select(u => new User {
            Id = u.Id,
            Login = u.Login,
            Email = u.Email,
            LocationId = u.LocationId,
            RoleId = u.RoleId
        }).ToListAsync();
    }
}