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
public class CommentsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IContentTypeProvider _contentTypeProvider;
    private readonly string _uploadPath;

    public CommentsController(AppDbContext context, IContentTypeProvider contentTypeProvider)
    {
        _context = context;
        _contentTypeProvider = contentTypeProvider;
        _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if (!Directory.Exists(_uploadPath)) Directory.CreateDirectory(_uploadPath);
    }

    // --- POBIERANIE KOMENTARZY DLA ZGŁOSZENIA ---
    [HttpGet("report/{reportId}")]
    public async Task<IActionResult> GetCommentsForReport(int reportId)
    {
        // 1. Sprawdzamy czy zgłoszenie istnieje
        var report = await _context.Reports.FindAsync(reportId);
        if (report == null) return NotFound("Zgłoszenie nie istnieje.");

        // 2. Sprawdzamy uprawnienia do zgłoszenia
        if (!HasAccessToReport(report)) return Forbid();

        // 3. Pobieramy komentarze
        var comments = await _context.Comments
            .Where(c => c.ReportId == reportId)
            .Include(c => c.User) // Dołączamy autora
            .OrderBy(c => c.DateAdded)
            .Select(c => new 
            {
                c.Id,
                c.Content,
                c.DateAdded,
                c.FileId,
                c.FileName,
                c.UserId,
                UserLogin = c.User != null ? c.User.Login : "Nieznany"
            })
            .ToListAsync();

        return Ok(comments);
    }

    // --- DODAWANIE KOMENTARZA ---
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> PostComment([FromForm] Comment comment, IFormFile? file)
    {
        // 1. Sprawdzamy zgłoszenie
        var report = await _context.Reports.FindAsync(comment.ReportId);
        if (report == null) return NotFound("Zgłoszenie nie istnieje.");

        // 2. Sprawdzamy uprawnienia
        if (!HasAccessToReport(report)) return Forbid();

        // 3. Uzupełniamy dane
        comment.UserId = GetId();
        comment.DateAdded = DateTime.UtcNow;

        // 4. Obsługa pliku
        if (file != null && file.Length > 0)
        {
            var fileId = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(_uploadPath, fileId);
            
            using (var stream = new FileStream(filePath, FileMode.Create)) await file.CopyToAsync(stream);
            
            comment.FileId = fileId;
            comment.FileName = file.FileName;
        }

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        // Zwracamy obiekt z loginem autora
        var currentUserName = User.Identity?.Name ?? "Ja";
        return Ok(new 
        {
            comment.Id,
            comment.Content,
            comment.DateAdded,
            comment.FileId,
            comment.FileName,
            comment.UserId,
            UserLogin = currentUserName
        });
    }

    // --- POBIERANIE PLIKU Z KOMENTARZA ---
    [HttpGet("{id}/download-file")]
    public async Task<IActionResult> DownloadCommentFile(int id)
    {
        // 1. Pobieramy komentarz
        var comment = await _context.Comments
            .Include(c => c.Report) // Potrzebujemy raportu do sprawdzenia uprawnień
            .FirstOrDefaultAsync(c => c.Id == id);

        if (comment == null || string.IsNullOrEmpty(comment.FileId)) return NotFound();

        // 2. Sprawdzamy uprawnienia do RAPORTU, którego dotyczy komentarz
        if (comment.Report == null || !HasAccessToReport(comment.Report)) return Forbid();

        var filePath = Path.Combine(_uploadPath, comment.FileId);
        if (!System.IO.File.Exists(filePath)) return NotFound("Plik fizycznie nie istnieje.");

        string contentType;
        if (!_contentTypeProvider.TryGetContentType(comment.FileId!, out contentType))
        {
            contentType = "application/octet-stream";
        }

        var fileStream = System.IO.File.OpenRead(filePath);
        var encodedFileName = Uri.EscapeDataString(comment.FileName!);
        Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{encodedFileName}\"; filename*=UTF-8''{encodedFileName}");

        return File(fileStream, contentType);
    }

    // --- METODY POMOCNICZE (Identyczne jak w ReportsController) ---
    private int GetId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    private string GetRole() => User.FindFirst(ClaimTypes.Role)?.Value ?? "user";

    private bool HasAccessToReport(Report r)
    {
        var id = GetId(); 
        var role = GetRole();
        
        // Admin i Manager widzą wszystko
        if (role == "manager" || role == "admin") return true;
        
        // Worker widzi jeśli zgłosił LUB jest przypisany
        if (role == "worker") return r.ReporterId == id || r.AssigneeId == id;
        
        // User widzi tylko swoje zgłoszenia
        return r.ReporterId == id;
    }
}