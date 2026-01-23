using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MojeApiMariaDB.Models;

public class Comment
{
    public int Id { get; set; }
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    public DateTime DateAdded { get; set; }

    // Relacja do Zgłoszenia
    public int ReportId { get; set; }
    [JsonIgnore] public Report? Report { get; set; }

    // Relacja do Autora komentarza
    public int UserId { get; set; }
    [JsonIgnore] public User? User { get; set; }

    // Opcjonalny plik
    public string? FileId { get; set; }
    public string? FileName { get; set; }
}