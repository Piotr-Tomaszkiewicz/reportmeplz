using System.Text.Json.Serialization;

namespace MojeApiMariaDB.Models;

public class User
{
    public int Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    // Relacja do Location
    public int LocationId { get; set; }
    [JsonIgnore] // Ukrywamy obiekt w JSON, aby uniknąć cykli, przesyłamy samo ID
    public Location? Location { get; set; }

    // Relacja do Role
    public int RoleId { get; set; }
    [JsonIgnore]
    public Role? Role { get; set; }
}