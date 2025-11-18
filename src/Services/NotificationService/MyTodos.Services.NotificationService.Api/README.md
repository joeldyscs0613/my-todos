# NotificationService.Api

REST API for sending notifications and viewing notification history.

## What It Does

ASP.NET Core Web API that exposes HTTP endpoints for sending emails, SMS, and push notifications. Also provides notification history. Runs on port 5250.

## Structure

```
Api/
├── Controllers/
│   └── NotificationsController.cs # Send and query endpoints
└── Program.cs                     # API setup, JWT, Swagger, RabbitMQ
```

## Endpoints

| Path | Method | Auth | Description |
|------|--------|------|-------------|
| `/api/notifications/email` | POST | Protected | Send email notification |
| `/api/notifications/sms` | POST | Protected | Send SMS notification |
| `/api/notifications` | GET | Protected | Get notification history |
| `/api/notifications/{id}` | GET | Protected | Get notification details |
| `/health` | GET | Public | Health check |

## Getting Started

```bash
cd src/Services/NotificationService/MyTodos.Services.NotificationService.Api
dotnet run
```

API runs on http://localhost:5250 and https://localhost:5251.

Swagger docs: http://localhost:5250/swagger

---

*Part of MyTodos NotificationService - API layer for a microservices take-home assignment.*
