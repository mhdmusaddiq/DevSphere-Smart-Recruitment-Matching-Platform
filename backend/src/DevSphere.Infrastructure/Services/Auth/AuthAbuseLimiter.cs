using System.Collections.Concurrent;

namespace DevSphere.Infrastructure.Services.Auth;

public interface IAuthAbuseLimiter
{
    bool RecordLoginFailure(
        string normalizedAccount,
        int permitLimit,
        TimeSpan window,
        out TimeSpan retryAfter);

    void ResetLoginFailures(string normalizedAccount);
}

public sealed class AuthAbuseLimiter : IAuthAbuseLimiter
{
    private readonly ConcurrentDictionary<string, Counter> _loginFailures =
        new(StringComparer.Ordinal);
    private int _operations;

    public bool RecordLoginFailure(
        string normalizedAccount,
        int permitLimit,
        TimeSpan window,
        out TimeSpan retryAfter)
    {
        var now = DateTimeOffset.UtcNow;
        var counter = _loginFailures.GetOrAdd(
            normalizedAccount,
            _ => new Counter(now.Add(window)));
        bool accepted;

        lock (counter.Sync)
        {
            if (counter.ExpiresAt <= now)
            {
                counter.Count = 0;
                counter.ExpiresAt = now.Add(window);
            }

            counter.Count++;
            accepted = counter.Count <= permitLimit;
            retryAfter = counter.ExpiresAt - now;
        }

        if ((Interlocked.Increment(ref _operations) & 255) == 0)
        {
            RemoveExpired(now);
        }

        return accepted;
    }

    public void ResetLoginFailures(string normalizedAccount)
    {
        _loginFailures.TryRemove(normalizedAccount, out _);
    }

    private void RemoveExpired(DateTimeOffset now)
    {
        foreach (var item in _loginFailures)
        {
            if (item.Value.ExpiresAt <= now)
            {
                _loginFailures.TryRemove(item.Key, out _);
            }
        }
    }

    private sealed class Counter
    {
        public Counter(DateTimeOffset expiresAt)
        {
            ExpiresAt = expiresAt;
        }

        public object Sync { get; } = new();
        public int Count { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
    }
}
