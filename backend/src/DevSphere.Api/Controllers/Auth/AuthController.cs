using System.Security.Claims;
using DevSphere.Application.DTOs.Auth;
using DevSphere.Infrastructure.Identity;
using DevSphere.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DevSphere.Api.Controllers.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TokenService _tokenService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        TokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        RegisterRequest request)
    {
        var role = NormalizePublicRole(request.Role);

        if (role == null)
        {
            return BadRequest(new
            {
                message = "Invalid role. Public registration supports JobSeeker or Employer only."
            });
        }

        var existingUser = await _userManager
            .FindByEmailAsync(request.Email);

        if (existingUser != null)
        {
            return BadRequest(new
            {
                message = "Email already exists."
            });
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
            IsActive = true
        };

        var result = await _userManager
            .CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        var roleResult = await _userManager
            .AddToRoleAsync(user, role);

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            return BadRequest(roleResult.Errors);
        }

        var token = await _tokenService.CreateToken(user);

        return Ok(new AuthResponse
        {
            Token = token,
            Email = user.Email!,
            DisplayName = user.DisplayName
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginRequest request)
    {
        var user = await _userManager
            .FindByEmailAsync(request.Email);

        if (user == null || !user.IsActive)
        {
            return Unauthorized();
        }

        var validPassword = await _userManager
            .CheckPasswordAsync(
                user,
                request.Password);

        if (!validPassword)
        {
            return Unauthorized();
        }

        var token = await _tokenService.CreateToken(user);

        return Ok(new AuthResponse
        {
            Token = token,
            Email = user.Email!,
            DisplayName = user.DisplayName
        });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = User.FindFirst(
            ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return Unauthorized();
        }

        var roles = await _userManager.GetRolesAsync(user);

        var internalRole = roles.FirstOrDefault()
            ?? string.Empty;

        return Ok(new AuthMeResponse
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            DisplayName = user.DisplayName,
            Role = ToPublicRole(internalRole),
            AccountState = user.IsActive
                ? "Active"
                : "Disabled"
        });
    }

    private static string? NormalizePublicRole(
        string? requestedRole)
    {
        var role = requestedRole?.Trim();

        if (string.Equals(
                role,
                "JobSeeker",
                StringComparison.OrdinalIgnoreCase))
        {
            return AppRoles.Candidate;
        }

        if (string.Equals(
                role,
                AppRoles.Employer,
                StringComparison.OrdinalIgnoreCase))
        {
            return AppRoles.Employer;
        }

        return null;
    }

    private static string ToPublicRole(
        string internalRole)
    {
        if (string.Equals(
                internalRole,
                AppRoles.Candidate,
                StringComparison.OrdinalIgnoreCase))
        {
            return "JobSeeker";
        }

        return internalRole;
    }
}
