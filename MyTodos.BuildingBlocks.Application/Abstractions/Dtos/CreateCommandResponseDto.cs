namespace MyTodos.BuildingBlocks.Application.Abstractions.Dtos;

public record CreateCommandResponseDto<TId>(TId Id);