# IdentityService.Application

Application layer with CQRS commands and queries for identity operations.

## What It Does

Implements use cases for user management, authentication, tenant setup, and role/permission assignment using MediatR. This is where business workflows live.

## Structure

```
Application/
├── Users/
│   ├── Commands/                  # InviteUser, RegisterUser, DeactivateUser
│   ├── Queries/                   # GetUserDetails, GetUsersPagedList
│   └── Contracts/                 # IUserReadRepository, IUserWriteRepository
├── Tenants/
│   ├── Commands/                  # CreateTenant, UpdateTenant
│   └── Queries/                   # GetTenantDetails
├── Authentication/
│   └── Commands/                  # SignIn, SignOut, RefreshToken
└── Invitations/
    ├── Commands/                  # AcceptInvitation
    └── Queries/                   # GetInvitationDetails
```

## Key Features

- **CQRS**: Commands (write) and queries (read) separated
- **MediatR**: All requests go through handlers
- **FluentValidation**: Input validation for commands/queries
- **Repository Contracts**: Interfaces defined here (implementations in Infrastructure)

---

*Part of MyTodos IdentityService - application layer for a microservices take-home assignment.*
