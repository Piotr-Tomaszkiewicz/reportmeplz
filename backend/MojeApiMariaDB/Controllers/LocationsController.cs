using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MojeApiMariaDB.Data;
using MojeApiMariaDB.Models;
using System.Security.Claims;

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

    // --- READ (Dostęp Publiczny dla Rejestracji) ---
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Location>>> GetLocations()
    {
        return await _context.Locations.ToListAsync();
    }
    
    // --- CREATE (Tylko Admin) ---
    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<ActionResult<Location>> PostLocation(Location location)
    {
        if (string.IsNullOrWhiteSpace(location.ShortName))
        {
            return BadRequest("Krótka nazwa jest wymagana.");
        }

        _context.Locations.Add(location);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetLocations), new { id = location.Id }, location);
    }
    
    // --- UPDATE (Tylko Admin) ---
    [Authorize(Roles = "admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> PutLocation(int id, Location location)
    {
        if (id != location.Id)
        {
            return BadRequest("ID w URL i obiekcie muszą być zgodne.");
        }
        if (string.IsNullOrWhiteSpace(location.ShortName))
        {
            return BadRequest("Krótka nazwa jest wymagana.");
        }

        _context.Entry(location).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Locations.Any(e => e.Id == id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent(); // 204 Success
    }

    // --- DELETE (Tylko Admin) ---
    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLocation(int id)
    {
        var location = await _context.Locations.FindAsync(id);
        if (location == null) 
        {
            return NotFound();
        }

        // DODATKOWA WERYFIKACJA: Czy istnieją użytkownicy powiązani z tą lokalizacją?
        var usersCount = await _context.Users.CountAsync(u => u.LocationId == id);
        if (usersCount > 0)
        {
            return BadRequest($"Nie można usunąć lokalizacji ID {id}, ponieważ jest powiązana z {usersCount} użytkownikami.");
        }

        _context.Locations.Remove(location);
        await _context.SaveChangesAsync();
        
        return NoContent(); // 204 Success
    }
}