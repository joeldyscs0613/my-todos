# TodoService.Api

REST API for the core of the domain main features.

## What It Does

ASP.NET Core Web API that exposes HTTP endpoints for creating, reading, updating, and deleting todo items. 
Runs on port 5011.

## Structure

```
Api/
├── Controllers/
│   └── TodosController.cs         # CRUD endpoints
└── Program.cs                     # API setup, JWT, Swagger
```

## Endpoints

| Path | Method | Auth | Description |
|------|--------|------|-------------|
| `/api/todos` | GET | Protected | Get paginated todo list |
| `/api/todos/{id}` | GET | Protected | Get todo details |
| `/api/todos` | POST | Protected | Create new todo |
| `/api/todos/{id}` | PUT | Protected | Update todo |
| `/api/todos/{id}/complete` | PATCH | Protected | Mark todo as completed |
| `/api/todos/{id}` | DELETE | Protected | Delete todo |
| `/health` | GET | Public | Health check |

## Getting Started

```bash
cd src/Services/TodoService/MyTodos.Services.TodoService.Api
dotnet run
```

API runs on http://localhost:5011 and https://localhost:5012.

Swagger docs: http://localhost:5011/swagger

---

*Part of MyTodos TodoService - API layer for a microservices take-home assignment.*
