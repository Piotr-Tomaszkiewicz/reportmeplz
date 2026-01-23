using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MojeApiMariaDB.Data;
using MojeApiMariaDB.DTOs; // Pamiętaj o dodaniu tego usinga
using MojeApiMariaDB.Models;
using System.Security.Claims;

namespace MojeApiMariaDB.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }
    
    // --- EDYCJA UŻYTKOWNIKA (TYLKO ADMIN) ---
    [Authorize(Roles = "admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateRequest request)
    {
        // 1. Znajdź użytkownika
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound("Użytkownik nie istnieje.");

        // 2. Walidacja: Czy nowy login nie jest już zajęty przez kogoś innego?
        if (await _context.Users.AnyAsync(u => u.Login == request.Login && u.Id != id))
        {
            return BadRequest($"Login '{request.Login}' jest już zajęty.");
        }

        // 3. Walidacja: Czy lokalizacja i rola istnieją?
        if (!await _context.Locations.AnyAsync(l => l.Id == request.LocationId))
            return BadRequest("Podana lokalizacja nie istnieje.");
            
        if (!await _context.Roles.AnyAsync(r => r.Id == request.RoleId))
            return BadRequest("Podana rola nie istnieje.");

        // 4. Aktualizacja pól
        user.Login = request.Login;
        user.Email = request.Email;
        user.LocationId = request.LocationId;
        user.RoleId = request.RoleId;

        // 5. Zmiana hasła (tylko jeśli zostało podane)
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return StatusCode(500, "Błąd podczas zapisu danych.");
        }

        return NoContent(); // 204 Success
    }

    // --- USUWANIE UŻYTKOWNIKA (TYLKO ADMIN) ---
    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        // Opcjonalne zabezpieczenie: nie pozwól adminowi usunąć samego siebie
        if (id == GetCurrentUserId())
        {
            return BadRequest("Nie możesz usunąć własnego konta.");
        }

        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // --- POBIERANIE WŁASNYCH DANYCH ---
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUserProfile()
    {
        var userId = GetCurrentUserId();
        var user = await _context.Users
            .Include(u => u.Location)
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return NotFound();
        return Ok(MapUserToProfile(user));
    }
    
    // --- POBIERANIE PO ID (DLA WORKER/MANAGER/ADMIN) ---
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var role = GetCurrentUserRole();
        if (role == "user") return Forbid();

        var user = await _context.Users
            .Include(u => u.Location)
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return NotFound();
        return Ok(MapUserToProfile(user));
    }

    // --- LISTA UŻYTKOWNIKÓW (ADMIN/MANAGER) ---
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetUsers()
    {
        if (GetCurrentUserRole() != "admin" && GetCurrentUserRole() != "manager")
            return Forbid();

        var users = await _context.Users
            .Include(u => u.Location)
            .Include(u => u.Role)
            .Select(u => MapUserToProfile(u))
            .ToListAsync();
            
        return Ok(users);
    }

    // --- POMOCNICZE ---
    private static object MapUserToProfile(User user)
    {
        return new
        {
            user.Id,
            user.Login,
            user.Email,
            LocationId = user.LocationId,
            LocationShortName = user.Location?.ShortName,
            LocationFullAddress = user.Location?.FullAddress,
            Role = user.Role?.Name,
            RoleId = user.RoleId // Zwracamy też ID roli, przydatne przy edycji w GUI
        };
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null ? int.Parse(claim.Value) : 0;
    }

    private string GetCurrentUserRole()
    {
        var claim = User.FindFirst(ClaimTypes.Role);
        return claim?.Value ?? "user";
    }
}