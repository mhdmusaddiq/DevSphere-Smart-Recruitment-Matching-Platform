using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DevSphere.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace DevSphere.Infrastructure.Services.Auth;

public sealed class JwtSessionValidator
{
    public const string SecurityStampClaim = "session_stamp";

    private readonly UserManager<ApplicationUser> _userManager;

    public JwtSessionValidator(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> IsCurrentAsync(
        ClaimsPrincipal? principal)
    {
        var userId = principal?.FindFirstValue(
                ClaimTypes.NameIdentifier) ??
            principal?.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var tokenStamp = principal?.FindFirstValue(
            SecurityStampClaim);

        if (string.IsNullOrWhiteSpace(userId) ||
            string.IsNullOrWhiteSpace(tokenStamp))
        {
            return false;
        }

        var user = await _userManager.FindByIdAsync(userId);

        return user is { IsActive: true } &&
            !string.IsNullOrWhiteSpace(user.SecurityStamp) &&
            string.Equals(
                tokenStamp,
                user.SecurityStamp,
                StringComparison.Ordinal);
    }
}
