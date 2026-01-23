namespace MojeApiMariaDB.DTOs;

public class UserUpdateRequest
{
    public string Login { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int LocationId { get; set; }
    public int RoleId { get; set; }
    
    // Opcjonalne: Jeśli null lub puste, hasło nie zostanie zmienione
    public string? Password { get; set; }
}