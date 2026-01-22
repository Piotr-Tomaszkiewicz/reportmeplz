using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MojeApiMariaDB.Data;
using MojeApiMariaDB.Models;

namespace MojeApiMariaDB.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LocationsController : ControllerBase
{
    private readonly AppDbContext _context;

    public LocationsController(AppDbContext context)
    {
        _context = context;
    }

    // Każdy może zobaczyć listę lokalizacji (potrzebne do rejestracji)
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Location>>> GetLocations()
    {
        return await _context.Locations.ToListAsync();
    }

    // Szczegóły konkretnej lokalizacji również publiczne
    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ActionResult<Location>> GetLocation(int id)
    {
        var location = await _context.Locations.FindAsync(id);
        if (location == null) return NotFound();
        return location;
    }

    // Dodawanie lokalizacji zabezpieczamy - tylko zalogowani (opcjonalnie: tylko admin)
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Location>> PostLocation(Location location)
    {
        _context.Locations.Add(location);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetLocation), new { id = location.Id }, location);
    }

    // Usuwanie lokalizacji zabezpieczone
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLocation(int id)
    {
        var location = await _context.Locations.FindAsync(id);
        if (location == null) return NotFound();

        _context.Locations.Remove(location);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}