namespace MyTodos.BuildingBlocks.Infrastructure.Http.Contracts;

/// <summary>
/// Marker interface for typed HTTP clients.
/// Enables dependency injection and configuration of specific HTTP clients.
/// </summary>
public interface ITypedHttpClient
{
    /// <summary>
    /// Gets the base address for this HTTP client.
    /// </summary>
    string BaseAddress { get; }
}
