# Extending CurrentUserService

This guide explains how to extend the base `CurrentUserService` with custom functionality specific to a service.

## Overview

The base `CurrentUserService` is located in `BuildingBlocks.Infrastructure` and provides core functionality:
- `UserId`, `Username`, `TenantId`, `IsAuthenticated` properties
- `GetRoles()` - virtual method
- `GetPermissions()` - virtual method

All services automatically get this base implementation. To add custom methods or override behavior, create an extended version.

## Example: TodoService Extension

### Step 1: Create Extended Class

Create a new class that inherits from `CurrentUserService`:

**Location:** `src/Services/TodoService/MyTodos.Services.TodoService.Infrastructure/Security/TodoServiceCurrentUserService.cs`

```csharp
using Microsoft.AspNetCore.Http;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.BuildingBlocks.Infrastructure.Security;

namespace MyTodos.Services.TodoService.Infrastructure.Security;

public class TodoServiceCurrentUserService : CurrentUserService
{
    public TodoServiceCurrentUserService(IHttpContextAccessor httpContextAccessor)
        : base(httpContextAccessor)
    {
    }

    // Add custom methods
    public bool CanManageTodos()
    {
        if (!IsAuthenticated)
            return false;

        var permissions = GetPermissions();
        return permissions.Contains("*") ||
               permissions.Contains("Todos.Manage");
    }

    // Override virtual methods if needed
    public override List<string> GetRoles()
    {
        var roles = base.GetRoles();
        // Add custom logic here
        return roles;
    }
}
```

### Step 2: Register in DependencyInjection

**Option A: Replace the base implementation (recommended)**

Update `TodoService.Infrastructure/DependencyInjection.cs`:

```csharp
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.TodoService.Infrastructure.Security;

public static IServiceCollection AddTodoServiceInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // ... existing code ...

    // BEFORE calling AddBuildingBlocksInfrastructure:
    // Register the extended CurrentUserService
    services.AddScoped<ICurrentUserService, TodoServiceCurrentUserService>();

    // BuildingBlocks uses TryAddScoped, so it will NOT override your registration
    services.AddBuildingBlocksInfrastructure(configuration);

    return services;
}
```

**How it works:** BuildingBlocks now uses `TryAddScoped` instead of `AddScoped`, which means it only registers if no implementation is already registered. This allows services to provide their own implementation before calling `AddBuildingBlocksInfrastructure()`.

**Option B: Register as a separate service**

If you want to keep both versions available:

```csharp
public static IServiceCollection AddTodoServiceInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // ... existing code ...

    // Register extended version with its own interface or as concrete type
    services.AddScoped<TodoServiceCurrentUserService>();

    services.AddBuildingBlocksInfrastructure(configuration);

    return services;
}
```

Then inject `TodoServiceCurrentUserService` directly where you need the extended methods.

### Step 3: Use in Your Code

**Example 1: In a Repository**

```csharp
public class TaskReadRepository : ReadEfRepository<TodoTask, Guid, TodoServiceDbContext>
{
    private readonly TodoServiceCurrentUserService _currentUserService;

    public TaskReadRepository(
        TodoServiceDbContext context,
        TodoServiceCurrentUserService currentUserService)
        : base(context, currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public async Task<List<TodoTask>> GetUserTasksAsync()
    {
        // Use custom methods
        if (!_currentUserService.CanManageTodos())
            throw new UnauthorizedAccessException();

        var tenantId = _currentUserService.GetEffectiveTenantIdForTodos();
        // ... query logic
    }
}
```

**Example 2: In a Command Handler**

```csharp
public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, Result<Guid>>
{
    private readonly TodoServiceCurrentUserService _currentUserService;

    public CreateTaskCommandHandler(TodoServiceCurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateTaskCommand request, CancellationToken ct)
    {
        // Use custom methods
        if (!_currentUserService.CanManageTodos())
            return Result<Guid>.Failure("Insufficient permissions");

        // ... rest of handler
    }
}
```

## Key Points

1. **Inheritance**: Your extended class inherits all base functionality (UserId, TenantId, etc.)

2. **Virtual Methods**: You can override `GetRoles()` and `GetPermissions()` to customize behavior

3. **Protected Members**: The base class exposes `_user` (ClaimsPrincipal) as protected, so you can access it in your extended class

4. **Registration Order**:
   - If you want to replace the base implementation, register your extended version BEFORE calling `AddBuildingBlocksInfrastructure()`
   - If you want both available, register your extended version as a separate service

5. **Dependency Injection**:
   - When replacing: Inject `ICurrentUserService` and it will resolve to your extended version
   - When separate: Inject your concrete type `TodoServiceCurrentUserService`

## When to Extend

Extend CurrentUserService when you need:
- Service-specific authorization checks
- Custom claim parsing logic
- Business logic related to the current user
- Overriding base behavior for your service's needs

## When NOT to Extend

Don't extend if you only need:
- The basic properties (UserId, TenantId, etc.) - use the base implementation
- Standard roles and permissions - use the base `GetRoles()` and `GetPermissions()`
