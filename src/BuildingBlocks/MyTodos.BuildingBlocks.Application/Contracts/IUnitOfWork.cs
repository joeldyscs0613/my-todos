namespace MyTodos.BuildingBlocks.Application.Contracts;

public interface IUnitOfWork
{
    public int Commit();
    public Task<int> CommitAsync(CancellationToken ct = default);
    public void Dispose();
}