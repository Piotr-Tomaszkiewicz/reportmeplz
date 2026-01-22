using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MojeApiMariaDB.Models;

public class Report
{
    public int Id { get; set; }
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [Range(1, 3)]
    public int Priority { get; set; }
    
    public string Status { get; set; } = "Zarejestrowany";

    public int ReporterId { get; set; }
    public int? AssigneeId { get; set; }
    
    public string? FileId { get; set; }     
    public string? FileName { get; set; }   

    [JsonIgnore] public User? Reporter { get; set; }
    [JsonIgnore] public User? Assignee { get; set; }
}