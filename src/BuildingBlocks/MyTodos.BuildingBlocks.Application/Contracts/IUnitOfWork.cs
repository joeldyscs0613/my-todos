namespace MyTodos.BuildingBlocks.Application.Contracts;

public interface IUnitOfWork : IAsyncDisposable
{
    public Task<int> CommitAsync(CancellationToken ct = default);
}