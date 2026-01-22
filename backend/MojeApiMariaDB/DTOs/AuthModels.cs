
namespace MojeApiMariaDB.DTOs;

public record RegisterRequest(string Login, string Email, string Password, int LocationId, int RoleId);
public record LoginRequest(string Login, string Password);
public record AuthResponse(string Token, string Login, string Role);