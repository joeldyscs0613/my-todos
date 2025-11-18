# BuildingBlocks.Presentation

ASP.NET Core web infrastructure for all microservice APIs.

## What It Does

Provides base controllers, middleware, authorization attributes, and common API configuration so each service doesn't reinvent the wheel.

## Structure

```
Presentation/
├── Controllers/
│   └── ApiControllerBase.cs        # Base class with ISender injected
├── Authorization/
│   └── HasPermissionAttribute.cs   # [HasPermission(...)] for endpoints
├── Extensions/
│   ├── ResultExtensions.cs         # result.ToActionResult()
│   └── ConfigurationExtensions.cs  # AddControllers, Swagger, etc.
└── Constants/
    └── ApiConstants.cs             # Common route conventions
```

## Key Features

- **ApiControllerBase**: All service controllers inherit from this (includes ISender). 
   I plan to add virtual concrete implementation methods/routes for Create, PagedList, AsOptions, Details, Updates, Delete, and other common and useful queries and commands as well as some additional helpers.
- **Result → ActionResult**: Converts `Result<T>` to proper HTTP responses
- **Permission Authorization**: Attribute-based endpoint protection
- **Problem Details**: Standardized error responses (RFC 7807)

## Usage

```csharp
[Route("api/users")]
public sealed class UsersController : ApiControllerBase
{
    [HttpGet("{id}")]
    [HasPermission(Permissions.Users.ViewDetails)]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken ct)
    {
        var query = new GetUserDetailsQuery(id);
        var result = await Sender.Send(query, ct);
        return result.ToActionResult();
    }
}
```

---

*Part of MyTodos BuildingBlocks - reusable infrastructure for microservices.*
