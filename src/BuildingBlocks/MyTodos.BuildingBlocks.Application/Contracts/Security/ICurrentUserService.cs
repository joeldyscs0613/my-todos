namespace MyTodos.BuildingBlocks.Application.Contracts.Security;

public interface ICurrentUserService
{
    public string? Username { get; }
}