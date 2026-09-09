namespace DevSphere.Application.DTOs.Auth;

public class ResendEmailVerificationRequest
{
    public string Email { get; set; } = string.Empty;
}

public class VerifyEmailRequest
{
    public string Email { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;
}
