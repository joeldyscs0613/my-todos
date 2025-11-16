using System.Linq.Expressions;
using MyTodos.BuildingBlocks.Application.Contracts;
using MyTodos.BuildingBlocks.Application.Contracts.Specifications;

namespace MyTodos.BuildingBlocks.Application.Helpers;

public class OrderBy<TEntity, TBy> : IOrderBy
{
    private readonly Expression<Func<TEntity, TBy>> _expression;
	
    public OrderBy(Expression<Func<TEntity, TBy>> expression)
    {
        _expression = expression;
    }

    public dynamic Expression => _expression;
}