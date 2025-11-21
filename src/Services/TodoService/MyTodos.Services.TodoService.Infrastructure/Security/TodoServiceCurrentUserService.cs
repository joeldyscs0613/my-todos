using Microsoft.AspNetCore.Http;
using MyTodos.BuildingBlocks.Infrastructure.Security;

namespace MyTodos.Services.TodoService.Infrastructure.Security;

/// <summary>
/// Extended CurrentUserService for TodoService.
/// Inherits from base CurrentUserService to reuse core functionality (UserId, Username, TenantId, etc.)
/// Add custom methods here as needed for todo-specific authorization and user context logic.
/// </summary>
public class TodoServiceCurrentUserService : CurrentUserService
{
    public TodoServiceCurrentUserService(IHttpContextAccessor httpContextAccessor)
        : base(httpContextAccessor)
    {
    }

    // Example methods to demonstrate extensibility:
    // Uncomment and implement as needed for your business logic

    // /// <summary>
    // /// Checks if the current user has permission to manage todos.
    // /// </summary>
    // public bool CanManageTodos()
    // {
    //     if (!IsAuthenticated)
    //         return false;
    //
    //     var permissions = GetPermissions();
    //     return permissions.Contains("*") ||
    //            permissions.Contains("Todos.Manage") ||
    //            permissions.Contains("Todos.Write");
    // }
}
