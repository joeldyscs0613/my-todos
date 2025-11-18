using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using MyTodos.BuildingBlocks.Infrastructure.Http.Configuration;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace MyTodos.BuildingBlocks.Infrastructure.Http.Resilience.Extensions;

/// <summary>
/// Extension methods for configuring resilience policies for HTTP clients.
/// </summary>
public static class ResilienceExtensions
{
    /// <summary>
    /// Adds a standard resilience handler with retry, circuit breaker, and timeout policies.
    /// Uses Microsoft.Extensions.Http.Resilience (Polly V8).
    /// </summary>
    public static IHttpClientBuilder AddStandardResilienceHandler(
        this IHttpClientBuilder builder,
        string? pipelineName = null)
    {
        var name = pipelineName ?? "standard-resilience";

        builder.AddResilienceHandler(name, (resiliencePipelineBuilder, context) =>
        {
            var settings = context.ServiceProvider.GetService<IOptions<ResilienceSettings>>()?.Value
                ?? new ResilienceSettings();

            // Configure the standard resilience pipeline
            ConfigureStandardPipeline(resiliencePipelineBuilder, settings);
        });

        return builder;
    }

    /// <summary>
    /// Adds a standard resilience handler with custom settings.
    /// </summary>
    public static IHttpClientBuilder AddStandardResilienceHandler(
        this IHttpClientBuilder builder,
        Action<ResilienceSettings> configureSettings,
        string? pipelineName = null)
    {
        var name = pipelineName ?? "standard-resilience";

        builder.AddResilienceHandler(name, (resiliencePipelineBuilder, context) =>
        {
            var settings = new ResilienceSettings();
            configureSettings(settings);

            ConfigureStandardPipeline(resiliencePipelineBuilder, settings);
        });

        return builder;
    }

    private static void ConfigureStandardPipeline(
        ResiliencePipelineBuilder<HttpResponseMessage> builder,
        ResilienceSettings settings)
    {
        // 1. Total request timeout (outer timeout)
        builder.AddTimeout(new HttpTimeoutStrategyOptions
        {
            Timeout = TimeSpan.FromSeconds(settings.Timeout.TotalTimeoutSeconds),
            Name = "TotalRequestTimeout"
        });

        // 2. Retry policy with exponential backoff
        builder.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = settings.Retry.MaxRetries,
            Delay = TimeSpan.FromSeconds(settings.Retry.BackoffBaseSeconds),
            UseJitter = settings.Retry.UseJitter,
            BackoffType = settings.Retry.UseExponentialBackoff
                ? DelayBackoffType.Exponential
                : DelayBackoffType.Constant,
            Name = "RetryPolicy"
        });

        // 3. Circuit breaker policy
        builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
        {
            FailureRatio = settings.CircuitBreaker.FailureThreshold / 100.0,
            SamplingDuration = TimeSpan.FromSeconds(settings.CircuitBreaker.SamplingDurationSeconds),
            MinimumThroughput = settings.CircuitBreaker.MinimumThroughput,
            BreakDuration = TimeSpan.FromSeconds(settings.CircuitBreaker.BreakDurationSeconds),
            Name = "CircuitBreaker"
        });

        // 4. Attempt timeout (inner timeout for each retry attempt)
        builder.AddTimeout(new HttpTimeoutStrategyOptions
        {
            Timeout = TimeSpan.FromSeconds(settings.Timeout.AttemptTimeoutSeconds),
            Name = "AttemptTimeout"
        });
    }

    /// <summary>
    /// Adds a custom retry policy.
    /// </summary>
    public static IHttpClientBuilder AddRetryPolicy(
        this IHttpClientBuilder builder,
        int maxRetries = 3,
        int backoffBaseSeconds = 2,
        bool useExponentialBackoff = true)
    {
        builder.AddResilienceHandler("retry", (resiliencePipelineBuilder, _) =>
        {
            resiliencePipelineBuilder.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = maxRetries,
                Delay = TimeSpan.FromSeconds(backoffBaseSeconds),
                BackoffType = useExponentialBackoff
                    ? DelayBackoffType.Exponential
                    : DelayBackoffType.Constant,
                UseJitter = true
            });
        });

        return builder;
    }

    /// <summary>
    /// Adds a custom circuit breaker policy.
    /// </summary>
    public static IHttpClientBuilder AddCircuitBreakerPolicy(
        this IHttpClientBuilder builder,
        double failureRatio = 0.5,
        int minimumThroughput = 10,
        int breakDurationSeconds = 30)
    {
        builder.AddResilienceHandler("circuit-breaker", (resiliencePipelineBuilder, _) =>
        {
            resiliencePipelineBuilder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
            {
                FailureRatio = failureRatio,
                MinimumThroughput = minimumThroughput,
                BreakDuration = TimeSpan.FromSeconds(breakDurationSeconds)
            });
        });

        return builder;
    }

    /// <summary>
    /// Adds a custom timeout policy.
    /// </summary>
    public static IHttpClientBuilder AddTimeoutPolicy(
        this IHttpClientBuilder builder,
        int timeoutSeconds = 30)
    {
        builder.AddResilienceHandler("timeout", (resiliencePipelineBuilder, _) =>
        {
            resiliencePipelineBuilder.AddTimeout(TimeSpan.FromSeconds(timeoutSeconds));
        });

        return builder;
    }
}
