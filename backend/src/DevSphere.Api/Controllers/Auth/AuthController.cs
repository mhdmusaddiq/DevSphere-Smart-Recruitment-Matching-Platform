using System.Security.Claims;
using DevSphere.Application.DTOs.Auth;
using DevSphere.Infrastructure.Identity;
using DevSphere.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using DevSphere.Infrastructure.Services.Auth;

namespace DevSphere.Api.Controllers.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TokenService _tokenService;
    private readonly EmailVerificationChallengeService _emailVerification;
    private readonly PasswordRecoveryChallengeService _passwordRecovery;
    private readonly IAuthChallengeDelivery _challengeDelivery;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        TokenService tokenService,
        EmailVerificationChallengeService emailVerification,
        PasswordRecoveryChallengeService passwordRecovery,
        IAuthChallengeDelivery challengeDelivery)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _emailVerification = emailVerification;
        _passwordRecovery = passwordRecovery;
        _challengeDelivery = challengeDelivery;
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

        var issue = await _emailVerification.IssueAsync(
            user,
            HttpContext.RequestAborted);

        if (issue.Accepted &&
            !string.IsNullOrWhiteSpace(issue.DevelopmentCode))
        {
            await _challengeDelivery.DeliverAsync(
                "EmailVerification",
                user.Email ?? string.Empty,
                issue.DevelopmentCode,
                HttpContext.RequestAborted);
        }

        var environment = HttpContext.RequestServices
            .GetRequiredService<IHostEnvironment>();

        if (environment.IsDevelopment())
        {
            return Ok(new
            {
                user.Email,
                user.DisplayName,
                accountState = "PendingEmailVerification",
                emailVerificationRequired = true,
                developmentCode = issue.DevelopmentCode
            });
        }

        return Ok(new
        {
            user.Email,
            user.DisplayName,
            accountState = "PendingEmailVerification",
            emailVerificationRequired = true,
            message =
                _challengeDelivery.IsAvailable
                    ? "Account created. Verification is required. Check your email."
                    : "Account created. Email verification is required. Email delivery is not configured."
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginRequest request)
    {
        var user = await _userManager
            .FindByEmailAsync(request.Email);

        if (user == null ||
            !user.IsActive ||
            !user.EmailConfirmed)
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

    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendVerification(
        ResendEmailVerificationRequest request)
    {
        var genericResponse = new
        {
            message =
                "If the account exists, a verification challenge has been processed."
        };

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return Ok(genericResponse);
        }

        var user =
            await _userManager.FindByEmailAsync(
                request.Email.Trim());

        if (user == null)
        {
            return Ok(genericResponse);
        }

        var issue =
            await _emailVerification.IssueAsync(
                user,
                HttpContext.RequestAborted);

        if (!issue.Accepted)
        {
            return Ok(genericResponse);
        }

        if (!string.IsNullOrWhiteSpace(
                issue.DevelopmentCode))
        {
            await _challengeDelivery.DeliverAsync(
                "EmailVerification",
                user.Email ?? string.Empty,
                issue.DevelopmentCode,
                HttpContext.RequestAborted);
        }
        if (HttpContext.RequestServices
            .GetRequiredService<IHostEnvironment>()
            .IsDevelopment())
        {
            return Ok(new
            {
                genericResponse.message,
                developmentCode =
                    issue.DevelopmentCode
            });
        }

        return Ok(genericResponse);
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail(
        VerifyEmailRequest request)
    {
        const string invalidMessage =
            "The verification challenge is invalid or expired.";

        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Code))
        {
            return BadRequest(new
            {
                message = invalidMessage
            });
        }

        var user =
            await _userManager.FindByEmailAsync(
                request.Email.Trim());

        if (user == null)
        {
            return BadRequest(new
            {
                message = invalidMessage
            });
        }

        var result =
            await _emailVerification.VerifyAsync(
                user,
                request.Code,
                HttpContext.RequestAborted);

        return result switch
        {
            EmailVerificationConsumeResult.Verified =>
                Ok(new
                {
                    message =
                        "Email verified successfully."
                }),

            EmailVerificationConsumeResult.AlreadyVerified =>
                Ok(new
                {
                    message =
                        "Email is already verified."
                }),

            _ =>
                BadRequest(new
                {
                    message = invalidMessage
                })
        };
    }
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordRequest request)
    {
        var genericResponse = new
        {
            message =
                "If the account exists, a password recovery challenge has been processed."
        };

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return Ok(genericResponse);
        }

        var user =
            await _userManager.FindByEmailAsync(
                request.Email.Trim());

        if (user == null)
        {
            return Ok(genericResponse);
        }

        var issue =
            await _passwordRecovery.IssueAsync(
                user,
                HttpContext.RequestAborted);

        if (!issue.Accepted)
        {
            return Ok(genericResponse);
        }

        if (!string.IsNullOrWhiteSpace(
                issue.DevelopmentCode))
        {
            await _challengeDelivery.DeliverAsync(
                "PasswordRecovery",
                user.Email ?? string.Empty,
                issue.DevelopmentCode,
                HttpContext.RequestAborted);
        }
        if (!string.IsNullOrWhiteSpace(
                issue.DevelopmentCode) &&
            HttpContext.RequestServices
                .GetRequiredService<IHostEnvironment>()
                .IsDevelopment())
        {
            return Ok(new
            {
                genericResponse.message,
                developmentCode =
                    issue.DevelopmentCode
            });
        }

        return Ok(genericResponse);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordRequest request)
    {
        const string invalidMessage =
            "The password recovery challenge is invalid or expired.";

        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Code))
        {
            return BadRequest(new
            {
                message = invalidMessage
            });
        }

        var user =
            await _userManager.FindByEmailAsync(
                request.Email.Trim());

        if (user == null)
        {
            return BadRequest(new
            {
                message = invalidMessage
            });
        }

        var result =
            await _passwordRecovery.ResetAsync(
                user,
                request.Code,
                request.NewPassword,
                HttpContext.RequestAborted);

        return result.Status switch
        {
            PasswordRecoveryConsumeResult.Reset =>
                Ok(new
                {
                    message =
                        "Password reset successfully."
                }),

            PasswordRecoveryConsumeResult.PasswordRejected =>
                BadRequest(new
                {
                    message =
                        "The new password does not meet password requirements.",
                    errors = result.Errors
                }),

            _ =>
                BadRequest(new
                {
                    message = invalidMessage
                })
        };
    }
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
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

        var result = await _userManager
            .UpdateSecurityStampAsync(user);

        if (!result.Succeeded)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message = "Logout could not be completed."
                });
        }

        return Ok(new
        {
            message = "Logged out. Discard the bearer token."
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
            AccountState = !user.IsActive
                ? "Disabled"
                : user.EmailConfirmed
                    ? "Active"
                    : "PendingEmailVerification",
            EmailVerified = user.EmailConfirmed
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
