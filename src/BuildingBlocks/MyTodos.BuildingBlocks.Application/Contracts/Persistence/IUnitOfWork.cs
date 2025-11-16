namespace MyTodos.BuildingBlocks.Application.Contracts.Persistence;

public interface IUnitOfWork : IAsyncDisposable
{
    public Task<int> CommitAsync(CancellationToken ct = default);
}