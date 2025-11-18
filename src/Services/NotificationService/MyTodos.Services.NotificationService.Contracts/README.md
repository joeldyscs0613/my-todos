# NotificationService.Contracts

Public contracts for inter-service communication and client integration.

## What It Does

Defines DTOs and request contracts that clients use to request notifications. No logic, just data structures.

## Structure

```
Contracts/
└── Notifications/
    ├── NotificationDto.cs             # Notification data transfer object
    ├── SendEmailRequest.cs            # Email send request
    └── SendSmsRequest.cs              # SMS send request
```

## Key Concepts

- **DTOs**: Data shapes for API responses (NotificationDto with id, recipient, message, status)
- **Request Contracts**: Strongly-typed request models for sending notifications
- **No dependencies**: Contracts are standalone

## When to Use

- **Other services** need to send notifications → reference NotificationService.Contracts
- **React Client** calls NotificationService API → uses DTOs from Swagger/OpenAPI

---

*Part of MyTodos NotificationService - contracts layer for a microservices take-home assignment.*
