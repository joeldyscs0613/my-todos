namespace MyTodos.Services.IdentityService.Application.Users.Queries.GetPagedList;

public sealed record UserPagedListResponseDto(
    Guid Id, string FirstName, string LastName, string Username, string Email, bool IsActive);