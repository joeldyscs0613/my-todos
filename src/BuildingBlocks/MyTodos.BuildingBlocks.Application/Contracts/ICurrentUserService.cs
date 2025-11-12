namespace MyTodos.BuildingBlocks.Application.Contracts;

public interface ICurrentUserService
{
    public string? Username { get; }
}