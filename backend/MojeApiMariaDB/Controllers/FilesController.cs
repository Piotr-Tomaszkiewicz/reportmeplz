using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MojeApiMariaDB.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class FilesController : ControllerBase
{
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        // 1. Walidacja czy plik w ogóle dotarł
        if (file == null || file.Length == 0)
            return BadRequest("Nie przesłano pliku.");

        // 2. Generowanie unikalnej nazwy, aby uniknąć nadpisywania plików
        var fileExtension = Path.GetExtension(file.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

        // 3. Budowanie ścieżki zapisu
        var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        var filePath = Path.Combine(uploadPath, uniqueFileName);

        // 4. Zapis pliku na dysku
        try
        {
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 5. Zwracamy relatywną ścieżkę do pliku, którą GUI przekaże do tabeli Reports
            var fileUrl = $"/uploads/{uniqueFileName}";
            return Ok(new { url = fileUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Błąd wewnętrzny serwera: {ex.Message}");
        }
    }
}