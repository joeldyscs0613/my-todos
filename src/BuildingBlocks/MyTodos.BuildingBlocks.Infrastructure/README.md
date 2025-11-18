# BuildingBlocks.Infrastructure

Infrastructure implementations for persistence, messaging, and HTTP client concerns.
It may contain other concerns later on (e.g. Caching).

## Structure

```
Infrastructure/
├── Persistence/
│   ├── Abstractions/
│   │   ├── Configs/            # Base EF config classes
│   │   ├── Repositories/       # EF repository implementations
│   │   ├── BaseDbContext.cs
│   │   └── UnitOfWork.cs
│   ├── Configs/
│   │   └── OutboxMessageConfiguration.cs
│   └── Constants/
│       ├── SchemaNames.cs
│       └── TableNames.cs
│
├── Messaging/
│   ├── Abstractions/
│   │   └── DomainEventToOutboxHandler.cs
│   ├── Configuration/
│   │   ├── OutboxProcessorSettings.cs
│   │   └── RabbitMqSettings.cs
│   ├── DomainEvents/
│   │   └── DomainEventDispatcher.cs
│   ├── Outbox/
│   │   ├── OutboxMessage.cs
│   │   └── OutboxProcessorService.cs
│   └── RabbitMqPublisher.cs
│
└── Http/
    ├── Abstractions/            # Base HTTP client classes
    ├── Configuration/           # Settings for HTTP clients
    ├── Constants/               # HTTP headers, media types
    ├── Contracts/               # HTTP client interfaces
    ├── Exceptions/              # HTTP-specific exceptions
    ├── Handlers/                # Delegating handlers (logging, correlation)
    ├── Resilience/              # Polly resilience extensions
    └── ServiceClients/          # Base service client classes
```

## Why One Project?

Since this is a job interview take-home assignment project, I kept all infrastructure in one project rather than splitting into `Infrastructure.Persistence`, `Infrastructure.Messaging`, etc.

**Reasons:**
- Simpler for code reviewers to navigate
- Folder structure keeps concerns separated
- Reduces complexity and makes it easier to review (fewer .csproj files, simpler build)

The folder organization mirrors what the split projects would look like, so extraction is straightforward if needed.

## When to Split

This is a take-home assignment; therefore I kept all in a single project for simplicity. 
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
- **UnitOfWork**: Transaction boundary, automatic audit trail, domain event orchestration
- **Base Config Classes**: EF Core configuration base classes for aggregates and entities
- **BaseDbContext**: Includes OutboxMessages table for transactional outbox pattern

### Messaging
- **DomainEventDispatcher**: Publishes domain events via MediatR after successful database commit
- **Outbox Pattern**: Domain events saved to database in same transaction, then published by background processor
- **OutboxProcessorService**: Background service that publishes outbox messages to RabbitMQ
- **RabbitMqPublisher**: Publishes integration events to RabbitMQ

**Flow:**
1. Aggregate raises event: `aggregate.AddDomainEvent(evt)`
2. UnitOfWork collects events and saves them to OutboxMessages table
3. Database saves (transaction boundary)
4. OutboxProcessorService picks up pending messages
5. Messages published to RabbitMQ
6. Other services consume events

### HTTP
- **Typed & Named Clients**: Extension methods for registering HTTP clients with standard configuration
- **Resilience Patterns**: Retry, circuit breaker, and timeout policies via Microsoft.Extensions.Http.Resilience
- **Correlation ID Propagation**: Automatic correlation ID header injection for distributed tracing
- **Request/Response Logging**: Delegating handler for HTTP request/response logging
- **Base Clients**: Abstract base classes for building strongly-typed HTTP service clients

## Design Principles

**Single Responsibility**: Each folder has one reason to change
- Persistence changes when database/queries change
- Messaging changes when event handling/bus changes
- HTTP changes when external API integration patterns change

**Dependency Inversion**: Infrastructure depends on Application contracts (`IUnitOfWork`, `IDomainEventDispatcher`), never the reverse

**Interface Segregation**: Consumers depend on specific contracts, not the entire Infrastructure project

---

*This structure balances simplicity for a take-home assignment with clear separation of concerns for future scalability.*
