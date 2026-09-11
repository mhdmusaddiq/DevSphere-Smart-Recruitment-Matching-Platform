namespace DevSphere.Infrastructure.Configurations;

public sealed class AuthRateLimitOptions
{
    public const string SectionName = "AuthRateLimits";

    public LoginLimitOptions Login { get; set; } = new();
    public IssueLimitOptions VerificationIssue { get; set; } = new();
    public ConsumeLimitOptions VerificationConsume { get; set; } = new();
    public IssueLimitOptions RecoveryIssue { get; set; } = new();
    public ConsumeLimitOptions ResetConsume { get; set; } = new();

    public void Validate()
    {
        Login.Validate(nameof(Login));
        VerificationIssue.Validate(nameof(VerificationIssue));
        VerificationConsume.Validate(nameof(VerificationConsume));
        RecoveryIssue.Validate(nameof(RecoveryIssue));
        ResetConsume.Validate(nameof(ResetConsume));
    }
}

public sealed class LoginLimitOptions
{
    public int AccountFailedAttempts { get; set; } = 5;
    public int AccountWindowMinutes { get; set; } = 15;
    public int IpPermitLimit { get; set; } = 30;
    public int IpWindowMinutes { get; set; } = 15;

    internal void Validate(string name)
    {
        AuthRateLimitValidation.Positive(AccountFailedAttempts, name);
        AuthRateLimitValidation.Positive(AccountWindowMinutes, name);
        AuthRateLimitValidation.Positive(IpPermitLimit, name);
        AuthRateLimitValidation.Positive(IpWindowMinutes, name);
    }
}

public sealed class IssueLimitOptions
{
    public int EmailIssueLimit { get; set; } = 5;
    public int EmailWindowMinutes { get; set; } = 60;
    public int CooldownSeconds { get; set; } = 60;
    public int IpPermitLimit { get; set; } = 20;
    public int IpWindowMinutes { get; set; } = 60;

    internal void Validate(string name)
    {
        AuthRateLimitValidation.Positive(EmailIssueLimit, name);
        AuthRateLimitValidation.Positive(EmailWindowMinutes, name);
        AuthRateLimitValidation.Positive(CooldownSeconds, name);
        AuthRateLimitValidation.Positive(IpPermitLimit, name);
        AuthRateLimitValidation.Positive(IpWindowMinutes, name);
    }
}

public sealed class ConsumeLimitOptions
{
    public int ChallengeFailedAttempts { get; set; } = 5;
    public int ChallengeLifetimeMinutes { get; set; } = 10;
    public int IpPermitLimit { get; set; } = 30;
    public int IpWindowMinutes { get; set; } = 15;

    internal void Validate(string name)
    {
        AuthRateLimitValidation.Positive(ChallengeFailedAttempts, name);
        AuthRateLimitValidation.Positive(ChallengeLifetimeMinutes, name);
        AuthRateLimitValidation.Positive(IpPermitLimit, name);
        AuthRateLimitValidation.Positive(IpWindowMinutes, name);
    }
}

internal static class AuthRateLimitValidation
{
    public static void Positive(int value, string section)
    {
        if (value <= 0)
        {
            throw new InvalidOperationException(
                $"{AuthRateLimitOptions.SectionName}:{section} values must be positive.");
        }
    }
}

public static class AuthRateLimitPolicyNames
{
    public const string LoginIp = "auth-login-ip";
    public const string VerificationIssueIp = "auth-verification-issue-ip";
    public const string VerificationConsumeIp = "auth-verification-consume-ip";
    public const string RecoveryIssueIp = "auth-recovery-issue-ip";
    public const string ResetConsumeIp = "auth-reset-consume-ip";
}
