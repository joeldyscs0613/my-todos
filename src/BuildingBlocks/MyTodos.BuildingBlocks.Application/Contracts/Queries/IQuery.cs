namespace MyTodos.BuildingBlocks.Application.Contracts.Queries;

/// <summary>
/// Marker interface for queries in the CQRS pattern.
/// Queries are read-only operations that return data without modifying state.
/// </summary>
/// <remarks>
/// This interface serves as a marker to distinguish queries from commands in the MediatR pipeline.
/// It enables query-specific behaviors and middleware to be applied selectively.
/// </remarks>
public interface IQuery
{
}
