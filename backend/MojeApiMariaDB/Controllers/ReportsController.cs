using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.StaticFiles;
using MojeApiMariaDB.Data;
using MojeApiMariaDB.Models;

namespace MojeApiMariaDB.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IContentTypeProvider _contentTypeProvider;
    private readonly string _uploadPath;

    public ReportsController(AppDbContext context, IContentTypeProvider contentTypeProvider)
    {
        _context = context;
        _contentTypeProvider = contentTypeProvider;
        _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if (!Directory.Exists(_uploadPath)) Directory.CreateDirectory(_uploadPath);
    }

    // --- POBIERANIE MOICH ZGŁOSZEŃ ---
    [HttpGet("my")]
    public async Task<IActionResult> GetMyReports()
    {
        var userId = GetId();
        var role = GetRole();
        
        IQueryable<Report> q = _context.Reports
            .Include(r => r.Reporter)
            .Include(r => r.Assignee)
            .Where(r => r.ReporterId == userId || r.AssigneeId == userId);

        var result = await q.Select(r => new 
        {
            r.Id,
            r.Title,
            r.Description,
            r.Priority,
            r.Status,
            r.FileId,
            r.FileName,
            r.ReporterId,
            ReporterNick = r.Reporter != null ? r.Reporter.Login : "Nieznany",
            r.AssigneeId,
            AssigneeNick = r.Assignee != null ? r.Assignee.Login : "Nieprzypisany"
        }).ToListAsync();

        return Ok(result);
    }

    // --- POBIERANIE WSZYSTKICH (TYLKO DLA MANAGERA/ADMINA) ---
    [HttpGet]
    public async Task<IActionResult> GetAllReports()
    {
        var role = GetRole();
        if (role != "admin" && role != "manager") return Forbid();

        var result = await _context.Reports
            .Include(r => r.Reporter)
            .Include(r => r.Assignee)
            .Select(r => new 
            {
                r.Id, r.Title, r.Description, r.Priority, r.Status, 
                r.FileId,
                r.FileName,
                r.ReporterId,
                ReporterNick = r.Reporter != null ? r.Reporter.Login : "Nieznany",
                r.AssigneeId,
                AssigneeNick = r.Assignee != null ? r.Assignee.Login : "Nieprzypisany"
            }).ToListAsync();

        return Ok(result);
    }
    
    // --- POBIERANIE POJEDYNCZEGO ZGŁOSZENIA ---
    [HttpGet("{id}")]
    public async Task<IActionResult> GetReport(int id)
    {
        var report = await _context.Reports
            .Include(r => r.Reporter)
            .Include(r => r.Assignee)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (report == null) return NotFound();
        if (!CanAccess(report)) return Forbid();

        return Ok(new 
        {
            report.Id, report.Title, report.Description, report.Priority, report.Status, 
            report.FileId,
            report.FileName,
            report.ReporterId,
            ReporterNick = report.Reporter?.Login ?? "Nieznany",
            report.AssigneeId,
            AssigneeNick = report.Assignee?.Login ?? "Nieprzypisany"
        });
    }

    // --- DODAWANIE (z obsługą pliku) ---
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> PostReport([FromForm] Report report, IFormFile? file)
    {
        report.ReporterId = GetId();
        if (string.IsNullOrEmpty(report.Status)) report.Status = "Zarejestrowany";

        if (file != null && file.Length > 0)
        {
            var fileId = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(_uploadPath, fileId);
            
            using (var stream = new FileStream(filePath, FileMode.Create)) await file.CopyToAsync(stream);
            
            report.FileId = fileId; 
            report.FileName = file.FileName; 
        }

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        var currentUserName = User.Identity?.Name ?? "Ja";
        
        return Ok(new 
        {
            report.Id, report.Title, report.Description, report.Priority, report.Status, 
            report.FileId,
            report.FileName,
            report.ReporterId,
            ReporterNick = currentUserName,
            report.AssigneeId,
            AssigneeNick = "Nieprzypisany"
        });
    }

    // --- POBIERANIE PLIKU (Z POPRAWNĄ NAZWĄ W NAGŁÓWKU) ---
    [HttpGet("{id}/download-file")]
    public async Task<IActionResult> Download(int id)
    {
        var report = await _context.Reports.FindAsync(id);
        
        if (report == null || !CanAccess(report) || string.IsNullOrEmpty(report.FileId)) 
            return NotFound("Plik nie istnieje lub brak uprawnień.");

        var filePath = Path.Combine(_uploadPath, report.FileId);
        
        if (!System.IO.File.Exists(filePath))
            return NotFound("Plik fizycznie nie istnieje na serwerze.");

        // 1. USTALENIE TYPU MIME
        string contentType;
        if (!_contentTypeProvider.TryGetContentType(report.FileId!, out contentType))
        {
            contentType = "application/octet-stream";
        }
        
        // 2. PRZYGOTOWANIE STRUMIENIA PLIKU
        var fileStream = System.IO.File.OpenRead(filePath);
        
        // 3. KODOWANIE NAZWY PLIKU dla nagłówka (obsługa spacji i znaków specjalnych)
        var encodedFileName = Uri.EscapeDataString(report.FileName!);

        // 4. USTAWIANIE NAGŁÓWKA CONTENT-DISPOSITION
        // Używamy File, aby mieć pełną kontrolę nad nagłówkami, w tym UTF-8
        Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{encodedFileName}\"; filename*=UTF-8''{encodedFileName}");

        return File(
            fileStream, 
            contentType
        );
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var report = await _context.Reports.FindAsync(id);
        if (report == null) return NotFound();
        var uid = GetId(); var role = GetRole();
        if (report.ReporterId != uid && role != "manager" && role != "admin") return Forbid();
        
        if (!string.IsNullOrEmpty(report.FileId))
        {
            var p = Path.Combine(_uploadPath, report.FileId);
            if(System.IO.File.Exists(p)) System.IO.File.Delete(p);
        }
        _context.Reports.Remove(report);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private int GetId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    private string GetRole() => User.FindFirst(ClaimTypes.Role)?.Value ?? "user";
    private bool CanAccess(Report r)
    {
        var id = GetId(); var role = GetRole();
        if (role == "manager" || role == "admin") return true;
        return r.ReporterId == id || r.AssigneeId == id;
    }
}