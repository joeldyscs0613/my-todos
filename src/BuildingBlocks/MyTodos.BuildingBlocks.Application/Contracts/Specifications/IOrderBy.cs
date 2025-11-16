namespace MyTodos.BuildingBlocks.Application.Contracts.Specifications;

public interface IOrderBy
{
    dynamic Expression { get; }
}