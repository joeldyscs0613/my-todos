# IdentityService.Contracts

Public contracts for inter-service communication and client integration.

## What It Does

Defines DTOs, events, and data contracts that other services and clients use to interact with IdentityService. No logic, just data structures.

## Structure

```
Contracts/
├── Users/
│   ├── UserDto.cs                     # User data transfer object
│   └── UserCreatedEvent.cs            # Integration event
├── Tenants/
│   └── TenantDto.cs
├── Roles/
│   └── RoleDto.cs
└── Permissions/
    └── PermissionDto.cs
```

## Key Concepts

- **DTOs**: Data shapes for API responses and requests
- **Integration Events**: Published to RabbitMQ for other services (e.g., UserCreatedEvent)
- **No dependencies**: Contracts don't reference domain or application layers

## When to Use

- **TodoService** needs user info → references IdentityService.Contracts
- **NotificationService** listens to UserCreatedEvent → references IdentityService.Contracts
- **React Client** calls API → uses DTOs from Swagger/OpenAPI

---

*Part of MyTodos IdentityService - contracts layer for a microservices take-home assignment.*
