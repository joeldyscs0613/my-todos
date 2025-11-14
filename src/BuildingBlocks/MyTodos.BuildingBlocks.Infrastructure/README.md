# BuildingBlocks.Infrastructure

Infrastructure implementations for persistence and messaging concerns.

## Structure

```
Infrastructure/
├── Persistence/
│   └── Abstractions/
│       ├── Repositories/       # EF Core read/write repositories
│       └── UnitOfWork.cs       # Transaction + audit trail + event dispatch
│
└── Messaging/
    └── DomainEvents/
        └── DomainEventDispatcher.cs    # MediatR event publisher
```

## Why One Project?

Since this is a job interview take home assignment project, I kept all infrastructure in one project rather than splitting into `Infrastructure.Persistence`, `Infrastructure.Messaging`, etc.

**Reasons:**
- Simpler for code reviewers to navigate
- Folder structure keeps concerns separated
- Reduces complexity and makes it easier to review (fewer .csproj files, simpler build)

The folder organization mirrors what the split projects would look like, so extraction is straightforward if needed.

## When to Split

In a production scenario, I'd split into separate projects when:

- **Different release cadences** - Messaging changes weekly, persistence is stable
- **Dependency isolation** - Services using only persistence shouldn't pull in MediatR, RabbitMQ, etc.
- **Team ownership** - Data team owns persistence, integration team owns messaging
- **Independent versioning** - Want to publish separate NuGet packages for each concern (e.g. Persistence, Messaging, etc.)
- **Technology swaps** - Replacing MediatR with MassTransit shouldn't touch database code

The split would look like:
```
BuildingBlocks.Infrastructure.Persistence/   # EF Core, repositories
BuildingBlocks.Infrastructure.Messaging/     # MediatR, domain events, outbox
BuildingBlocks.Infrastructure.Caching/       # Redis, memory cache
BuildingBlocks.Infrastructure.Http/          # HTTP clients, Polly policies
```

## Current Implementation

### Persistence
- **Repositories**: Read (no-tracking), Write (tracked), PagedList (with Specification pattern)
- **UnitOfWork**: Transaction boundary, automatic audit trail (CreatedBy, ModifiedBy), domain event orchestration
- **Query Configuration**: Centralized eager loading via `IEntityQueryConfiguration`

### Messaging
- **DomainEventDispatcher**: Publishes domain events via MediatR after successful database commit
- **Eventual Consistency**: Events only dispatched after `SaveChangesAsync()` succeeds
- **Sequential Publishing**: Events published in order to maintain dependencies

**Flow:**
1. Aggregate raises event: `aggregate.AddDomainEvent(evt)`
2. UnitOfWork collects events from tracked aggregates
3. Database saves (transaction boundary)
4. On success, events dispatched via MediatR
5. Event handlers react

## Design Principles

**Single Responsibility**: Each folder has one reason to change
- Persistence changes when database/queries change
- Messaging changes when event handling/bus changes

**Dependency Inversion**: Infrastructure depends on Application contracts (`IUnitOfWork`, `IDomainEventDispatcher`), never the reverse

**Interface Segregation**: Consumers depend on specific contracts, not the entire Infrastructure project

---

*This structure balances simplicity for a take-home assignment with clear separation of concerns for future scalability.*
