namespace MyTodos.BuildingBlocks.Application.Abstractions.Dtos;

/// <summary>
/// Generic DTO for dropdown/select list options.
/// Non-sealed to allow inheritance for specialized option DTOs.
/// </summary>
/// <typeparam name="TId">The type of the identifier (e.g., Guid, int)</typeparam>
public record OptionDto<TId>(TId Id, string Name);
