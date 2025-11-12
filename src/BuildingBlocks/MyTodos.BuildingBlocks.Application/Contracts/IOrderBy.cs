namespace MyTodos.BuildingBlocks.Application.Contracts;

public interface IOrderBy
{
    dynamic Expression { get; }
}