namespace MyTodos.BuildingBlocks.Infrastructure.Http.Configuration;

/// <summary>
/// Configuration settings for HTTP resilience policies.
/// </summary>
public class ResilienceSettings
{
    /// <summary>
    /// Gets or sets the retry policy settings.
    /// </summary>
    public RetrySettings Retry { get; set; } = new();

    /// <summary>
    /// Gets or sets the circuit breaker policy settings.
    /// </summary>
    public CircuitBreakerSettings CircuitBreaker { get; set; } = new();

    /// <summary>
    /// Gets or sets the timeout policy settings.
    /// </summary>
    public TimeoutSettings Timeout { get; set; } = new();

    /// <summary>
    /// Gets or sets the rate limiter policy settings.
    /// </summary>
    public RateLimiterSettings RateLimiter { get; set; } = new();
}

/// <summary>
/// Settings for retry policy.
/// </summary>
public class RetrySettings
{
    /// <summary>
    /// Gets or sets the maximum number of retry attempts. Default is 3.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Gets or sets the base delay in seconds for exponential backoff. Default is 2.
    /// </summary>
    public int BackoffBaseSeconds { get; set; } = 2;

    /// <summary>
    /// Gets or sets whether to use exponential backoff. Default is true.
    /// </summary>
    public bool UseExponentialBackoff { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to use jitter. Default is true.
    /// </summary>
    public bool UseJitter { get; set; } = true;
}

/// <summary>
/// Settings for circuit breaker policy.
/// </summary>
public class CircuitBreakerSettings
{
    /// <summary>
    /// Gets or sets the number of consecutive failures before opening the circuit. Default is 5.
    /// </summary>
    public int FailureThreshold { get; set; } = 5;

    /// <summary>
    /// Gets or sets the sampling duration in seconds. Default is 30.
    /// </summary>
    public int SamplingDurationSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the minimum throughput (requests per sampling duration). Default is 10.
    /// </summary>
    public int MinimumThroughput { get; set; } = 10;

    /// <summary>
    /// Gets or sets the duration of the break in seconds. Default is 30.
    /// </summary>
    public int BreakDurationSeconds { get; set; } = 30;
}

/// <summary>
/// Settings for timeout policy.
/// </summary>
public class TimeoutSettings
{
    /// <summary>
    /// Gets or sets the total request timeout in seconds. Default is 30.
    /// </summary>
    public int TotalTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the per-attempt timeout in seconds. Default is 10.
    /// </summary>
    public int AttemptTimeoutSeconds { get; set; } = 10;
}

/// <summary>
/// Settings for rate limiter policy.
/// </summary>
public class RateLimiterSettings
{
    /// <summary>
    /// Gets or sets the maximum number of permits per time window. Default is 100.
    /// </summary>
    public int PermitLimit { get; set; } = 100;

    /// <summary>
    /// Gets or sets the time window in seconds. Default is 60 (1 minute).
    /// </summary>
    public int WindowSeconds { get; set; } = 60;

    /// <summary>
    /// Gets or sets whether rate limiting is enabled. Default is false.
    /// </summary>
    public bool Enabled { get; set; } = false;
}

/// <summary>
/// Settings for correlation ID propagation.
/// </summary>
public class CorrelationSettings
{
    /// <summary>
    /// Gets or sets the header name for correlation ID. Default is "X-Correlation-ID".
    /// </summary>
    public string HeaderName { get; set; } = "X-Correlation-ID";

    /// <summary>
    /// Gets or sets whether to include correlation ID in logs. Default is true.
    /// </summary>
    public bool IncludeInLogs { get; set; } = true;
}
