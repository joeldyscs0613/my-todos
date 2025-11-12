using FluentValidation;
using MediatR;

namespace MyTodos.BuildingBlocks.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseRequest
{
    /// <summary>
    /// Handles the request by validating it using FluentValidation validators.
    /// Throws ValidationException if any validation failures occur.
    /// </summary>
    /// <param name="request">The request to validate.</param>
    /// <param name="next">The next handler in the pipeline.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The response from the handler.</returns>
    /// <exception cref="ValidationException">Thrown when validation fails.</exception>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct = default)
    {
        // Early exit if no validators are registered for this request type
        if (!validators.Any())
        {
            return await next(ct);
        }

        // Create validation context for FluentValidation
        var context = new ValidationContext<TRequest>(request);

        // Run all validators in parallel for better performance
        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, ct)));

        // Collect all validation failures from all validators
        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        // If any validation failures occurred, throw exception to stop pipeline
        if (failures.Any())
        {
            throw new ValidationException(failures);
        }

        // Validation passed - proceed to next handler in the pipeline
        return await next(ct);
    }
}