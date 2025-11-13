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
        if (!validators.Any())
        {
            return await next(ct);
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, ct)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
        {
            throw new ValidationException(failures);
        }

        return await next(ct);
    }
}