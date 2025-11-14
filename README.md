# My Todos

A clean architecture take-home assignment showcasing CQRS, DDD patterns, and domain event orchestration.

## Architecture

Built using **Vertical Slice Architecture** principles with reusable **BuildingBlocks** for cross-cutting concerns.

```
src/
├── BuildingBlocks/                    # Reusable infrastructure
│   ├── Application/                   # CQRS, validation, repository contracts
│   ├── Infrastructure/                # EF Core, domain events, persistence
│   └── Presentation/                  # Controllers, middleware, ProblemDetails
├── MyTodos.SharedKernel/              # Domain primitives, Result pattern, base classes
└── Services/                          # Feature modules (bounded contexts)
    └── [Coming soon]
```

## BuildingBlocks

Shared libraries that provide application and infrastructure patterns:

- **Application:** CQRS abstractions, MediatR behaviors (validation, UoW), repository contracts
- **Infrastructure:** EF Core repositories, domain event dispatcher, UnitOfWork
- **Presentation:** Base controllers, exception middleware, ProblemDetails formatting
- **SharedKernel:** DomainException, AggregateRoot, entity base classes, Result pattern

Each BuildingBlock has its own README with detailed explanations.

## Key Patterns

- **CQRS:** Commands modify state, queries read state (MediatR)
- **Repository Pattern:** Read/Write segregation with specifications
- **Domain Events:** Post-commit event publishing for eventual consistency
- **Specification Pattern:** Reusable, composable query logic
- **Result Pattern:** Explicit success/failure handling without exceptions for business logic

## Design Philosophy

This is a **take-home assignment**, so the focus is on demonstrating architectural knowledge while keeping complexity appropriate for review. In production, you'd split BuildingBlocks further based on bounded context needs and team structure.

## Getting Started

```bash
dotnet build
dotnet test
```

*See individual BuildingBlocks READMEs for architecture deep-dives.*
