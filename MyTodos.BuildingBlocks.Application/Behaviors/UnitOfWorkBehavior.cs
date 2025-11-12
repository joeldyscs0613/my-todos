using System.Transactions;
using MediatR;
using MyTodos.BuildingBlocks.Application.Contracts;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.BuildingBlocks.Application.Behaviors;

public sealed class UnitOfWorkBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>
    /// Handles the request by wrapping command execution in a database transaction.
    /// Queries bypass the unit of work. Transactions only commit on successful results.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="next">The next handler in the pipeline.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The response from the handler.</returns>
    public async Task<TResponse> Handle(TRequest request,
        RequestHandlerDelegate<TResponse> next, CancellationToken ct = default)
    {
        // Bypass unit of work for queries - they don't modify data
        if (!IsCommand())
        {
            return await next(ct);
        }

        // Create transaction scope with async flow enabled to support async/await
        // The transaction will automatically rollback on dispose if not explicitly completed
        using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        // Execute the command handler within the transaction
        var response = await next(ct);

        // Check if the response indicates a failure (using Result pattern)
        // Don't commit the transaction if the operation failed (we got an error result)
        if (response is Result { IsFailure: true })
        {
            // Transaction will automatically rollback when disposed
            return response;
        }

        // Success path - commit the database changes and complete the transaction
        await unitOfWork.CommitAsync(ct);
        transactionScope.Complete();

        return response;
    }

    private static bool IsCommand()
    {
        var interfaces = typeof(TRequest).GetInterfaces();
        var isCommand = interfaces.Contains(typeof(ICommand)) 
                        || typeof(TRequest).Name.EndsWith("Command", StringComparison.OrdinalIgnoreCase);
        
        return isCommand;
    }
}