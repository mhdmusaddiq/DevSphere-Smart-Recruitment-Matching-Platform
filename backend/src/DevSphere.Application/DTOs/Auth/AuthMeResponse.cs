namespace DevSphere.Application.DTOs.Auth;

public class AuthMeResponse
{
    public string UserId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string AccountState { get; set; } = string.Empty;

    public bool EmailVerified { get; set; }
}
