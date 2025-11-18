# NotificationService.Infrastructure

Infrastructure layer for notification delivery and external integrations.

## What It Does

Implements EF Core repositories, notification provider integrations (SendGrid, Twilio), RabbitMQ consumers, and database persistence.

## Structure

```
Infrastructure/
├── NotificationAggregate/
│   ├── Persistence/
│   │   └── NotificationConfig.cs
│   └── Repositories/
│       ├── NotificationReadRepository.cs
│       └── NotificationWriteRepository.cs
├── Providers/
│   ├── EmailProvider.cs           # SendGrid integration
│   ├── SmsProvider.cs             # Twilio integration
│   └── PushProvider.cs            # Firebase integration
├── Messaging/
│   └── RabbitMqConsumer.cs        # Listens to integration events
└── Persistence/
    ├── NotificationServiceDbContext.cs
    ├── NotificationServiceUnitOfWork.cs
    └── Migrations/
```

## Key Features

- **EF Core**: SQLite database for notification history
- **SendGrid**: Email delivery via HTTP API
- **Twilio**: SMS delivery (if configured)
- **RabbitMQ**: Consumes events from IdentityService and TodoService

---

*Part of MyTodos NotificationService - infrastructure layer for a microservices take-home assignment.*
