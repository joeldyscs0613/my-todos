# IdentityService.Domain

Domain layer for user identity, authentication, and multi-tenant authorization.

## What It Does

Contains the core business entities and rules for managing users, tenants, roles, and permissions. Pure domain logic with no external dependencies.

## Structure

```
Domain/
├── UserAggregate/
│   ├── User.cs                    # User aggregate root
│   ├── UserRole.cs                # User-role assignments
│   ├── UserInvitation.cs          # Invitation entity
│   └── DomainEvents/              # UserRegistered, etc.
├── TenantAggregate/
│   └── Tenant.cs                  # Multi-tenant isolation
├── RoleAggregate/
│   ├── Role.cs                    # Roles (Admin, Member, etc.)
│   └── RolePermission.cs          # Role-permission assignments
└── PermissionAggregate/
    └── Permission.cs              # Fine-grained permissions
```

## Key Rules

- Users belong to one tenant (multi-tenant isolation)
- Static factory methods: `User.Create()`, `Tenant.Create()`
- Domain events raised for important state changes
- No repositories here (they live in Application layer)

---

*Part of MyTodos IdentityService - domain layer for a microservices take-home assignment.*
