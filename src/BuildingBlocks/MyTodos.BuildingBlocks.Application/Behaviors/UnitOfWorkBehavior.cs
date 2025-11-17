using MediatR;
using Microsoft.EntityFrameworkCore;
using MyTodos.BuildingBlocks.Application.Contracts;
using MyTodos.BuildingBlocks.Application.Contracts.Commands;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.BuildingBlocks.Application.Behaviors;

/// <summary>
/// - Identified that SQLite doesn't support ambient transactions (System.Transactions.TransactionScope)
/// - Chose explicit EF Core transactions over just suppressing the warning
/// - Followed Microsoft's recommended approach: DbContext.Database.BeginTransactionAsync()
///
/// TO SWITCH BACK TO TRANSACTIONSCOPE (when migrating to SQL Server/PostgreSQL):
/// 1. Replace "using Microsoft.EntityFrameworkCore;" with "using System.Transactions;"
/// 2. Remove DbContext parameter from constructor
/// 3. Replace the try-catch block (lines 40-66) with:
///    using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
///    var response = await next(ct);
///    if (response is Result { IsFailure: true }) return response;
///    await unitOfWork.CommitAsync(ct);
///    transactionScope.Complete();
///    return response;
/// </summary>
public sealed class UnitOfWorkBehavior<TRequest, TResponse>(
    IUnitOfWork unitOfWork,
    DbContext dbContext) // Injected to use explicit transactions for SQLite compatibility
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
        // Bypass unit of work for queries - no data modification
        if (!IsCommand())
        {
            return await next(ct);
        }

        // Use explicit EF Core transaction instead of TransactionScope for SQLite compatibility
        // SQLite doesn't support ambient transactions, so we use Database.BeginTransactionAsync()
        // This is also Microsoft's recommended approach for EF Core (better async support)
        await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);

        try
        {
            // Execute the command handler within the transaction
            var response = await next(ct);

            // Check if the response indicates a failure (using Result pattern)
            // Don't commit the transaction if the operation failed
            if (response is Result { IsFailure: true })
            {
                // Explicit rollback for clarity (transaction auto-rolls back on dispose anyway)
                await transaction.RollbackAsync(ct);
                return response;
            }

            // Success path - commit the database changes and the transaction
            await unitOfWork.CommitAsync(ct);
            await transaction.CommitAsync(ct);

            return response;
        }
        catch
        {
            // Ensure transaction rollback on any exceptions
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    private static bool IsCommand()
    {
        var interfaces = typeof(TRequest).GetInterfaces();
        var isCommand = interfaces.Contains(typeof(ICommand)) 
                        || typeof(TRequest).Name.EndsWith("Command", StringComparison.OrdinalIgnoreCase);
        
        return isCommand;
    }
}