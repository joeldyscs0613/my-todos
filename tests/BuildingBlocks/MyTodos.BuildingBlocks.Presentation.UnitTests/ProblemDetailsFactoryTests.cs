using System.Net;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Moq;
using MyTodos.BuildingBlocks.Presentation.Constants;
using MyTodos.BuildingBlocks.Presentation.ProblemDetails;
using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.BuildingBlocks.Presentation.UnitTests;

public class ProblemDetailsFactoryTests
{
    private readonly Mock<IHostEnvironment> _mockEnvironment;
    private readonly Mock<HttpContext> _mockHttpContext;
    private readonly ProblemDetailsFactory _factory;

    public ProblemDetailsFactoryTests()
    {
        _mockEnvironment = new Mock<IHostEnvironment>();
        _mockHttpContext = new Mock<HttpContext>();
        _mockHttpContext.Setup(x => x.TraceIdentifier).Returns("test-trace-id-123");

        _factory = new ProblemDetailsFactory(_mockEnvironment.Object);
    }

    private void SetupDevelopmentEnvironment()
    {
        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Development");
    }

    private void SetupProductionEnvironment()
    {
        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Production");
    }

    #region Environment Awareness Tests

    [Fact]
    public void CreateFromException_InDevelopment_ShowsDetailedExceptionInfo()
    {
        // Arrange
        SetupDevelopmentEnvironment();
        var exception = new InvalidOperationException("Test exception message");

        // Act
        var result = _factory.CreateFromException(_mockHttpContext.Object, exception);

        // Assert
        Assert.NotNull(result.Detail);
        Assert.Contains("Test exception message", result.Detail);
        Assert.Contains("InvalidOperationException", result.Detail);
    }

    [Fact]
    public void CreateFromException_InProduction_ShowsGenericMessage()
    {
        // Arrange
        SetupProductionEnvironment();
        var exception = new InvalidOperationException("Sensitive internal error details");

        // Act
        var result = _factory.CreateFromException(_mockHttpContext.Object, exception);

        // Assert
        Assert.NotNull(result.Detail);
        Assert.DoesNotContain("Sensitive internal error details", result.Detail);
        Assert.Equal("An unexpected error occurred. Please contact support if the problem persists.", result.Detail);
    }

    [Fact]
    public void CreateFromException_DomainException_InProduction_ShowsExceptionMessage()
    {
        // Arrange
        SetupProductionEnvironment();
        var exception = new DomainException("User with email 'john@example.com' already exists");

        // Act
        var result = _factory.CreateFromException(_mockHttpContext.Object, exception);

        // Assert
        Assert.NotNull(result.Detail);
        Assert.Equal("User with email 'john@example.com' already exists", result.Detail);
        Assert.DoesNotContain("DomainException", result.Detail); // Should not show exception type
    }

    [Fact]
    public void CreateFromException_DomainException_InDevelopment_ShowsFullException()
    {
        // Arrange
        SetupDevelopmentEnvironment();
        var exception = new DomainException("Task title cannot be empty");

        // Act
        var result = _factory.CreateFromException(_mockHttpContext.Object, exception);

        // Assert
        Assert.NotNull(result.Detail);
        Assert.Contains("Task title cannot be empty", result.Detail);
        Assert.Contains("DomainException", result.Detail); // Should show full exception in dev
    }

    #endregion

    #region TraceId Tests

    [Fact]
    public void CreateFromException_AlwaysIncludesTraceId()
    {
        // Arrange
        SetupProductionEnvironment();
        var exception = new Exception("Test exception");

        // Act
        var result = _factory.CreateFromException(_mockHttpContext.Object, exception);

        // Assert
        Assert.True(result.Extensions.ContainsKey("traceId"));
        Assert.Equal("test-trace-id-123", result.Extensions["traceId"]);
    }

    [Fact]
    public void CreateFromException_ValidationException_IncludesTraceId()
    {
        // Arrange
        SetupProductionEnvironment();
        var validationFailures = new List<ValidationFailure>
        {
            new("Name", "Name is required")
        };
        var exception = new ValidationException(validationFailures);

        // Act
        var result = _factory.CreateFromException(_mockHttpContext.Object, exception);

        // Assert
        Assert.True(result.Extensions.ContainsKey("traceId"));
        Assert.Equal("test-trace-id-123", result.Extensions["traceId"]);
    }

    #endregion

    #region Exception Data Tests

    [Fact]
    public void CreateFromException_InDevelopment_WithExceptionData_IncludesData()
    {
        // Arrange
        SetupDevelopmentEnvironment();
        var exception = new InvalidOperationException("Test exception");
        exception.Data["CustomKey"] = "CustomValue";
        exception.Data["UserId"] = 12345;

        // Act
        var result = _factory.CreateFromException(_mockHttpContext.Object, exception);

        // Assert
        Assert.True(result.Extensions.ContainsKey("data"));
        Assert.Equal(exception.Data, result.Extensions["data"]);
    }

    [Fact]
    public void CreateFromException_InProduction_WithExceptionData_DoesNotIncludeData()
    {
        // Arrange
        SetupProductionEnvironment();
        var exception = new InvalidOperationException("Test exception");
        exception.Data["SensitiveKey"] = "SensitiveValue";

        // Act
        var result = _factory.CreateFromException(_mockHttpContext.Object, exception);

        // Assert
        Assert.False(result.Extensions.ContainsKey("data"));
    }

    [Fact]
    public void CreateFromException_InDevelopment_WithoutExceptionData_DoesNotIncludeData()
    {
        // Arrange
        SetupDevelopmentEnvironment();
        var exception = new InvalidOperationException("Test exception");

        // Act
        var result = _factory.CreateFromException(_mockHttpContext.Object, exception);

        // Assert
        Assert.False(result.Extensions.ContainsKey("data"));
    }

    #endregion

    #region Exception Type Mapping Tests

    [Fact]
    public void CreateFromException_DomainException_ReturnsBadRequestWithMessage()
    {
        // Arrange
        SetupProductionEnvironment();
        var exception = new DomainException("User with email 'john@example.com' already exists");

        // Act
        var result = _factory.CreateFromException(_mockHttpContext.Object, exception);

        // Assert
        Assert.Equal((int)HttpStatusCode.BadRequest, result.Status);
        Assert.Equal(ProblemDetailsConstants.Titles.BadRequest, result.Title);
        Assert.Equal("User with email 'john@example.com' already exists", result.Detail);
        Assert.Equal(ProblemDetailsConstants.Types.BadRequest, result.Type);
    }

    [Fact]
    public void CreateFromException_ValidationException_ReturnsCorrectProblemDetails()
    {
        // Arrange
        SetupProductionEnvironment();
        var validationFailures = new List<ValidationFailure>
        {
            new("Name", "Name is required"),
            new("Email", "Email is invalid")
        };
        var exception = new ValidationException(validationFailures);

        // Act
        var result = _factory.CreateFromException(_mockHttpContext.Object, exception);

        // Assert
        Assert.Equal((int)HttpStatusCode.BadRequest, result.Status);
        Assert.Equal("Validation Error", result.Title);
        Assert.Equal("One or more validation errors occurred.", result.Detail);
        Assert.Equal(ProblemDetailsConstants.Types.BadRequest, result.Type);
        Assert.True(result.Extensions.ContainsKey("errors"));
    }

    [Fact]
    public void CreateFromException_UnauthorizedAccessException_ReturnsCorrectProblemDetails()
    {
        // Arrange
        SetupProductionEnvironment();
        var exception = new UnauthorizedAccessException();

        // Act
        var result = _factory.CreateFromException(_mockHttpContext.Object, exception);

        // Assert
        Assert.Equal((int)HttpStatusCode.Unauthorized, result.Status);
        Assert.Equal(ProblemDetailsConstants.Titles.Unauthorized, result.Title);
        Assert.Equal("Authentication is required to access this resource.", result.Detail);
        Assert.Equal(ProblemDetailsConstants.Types.Unauthorized, result.Type);
    }

    [Fact]
    public void CreateFromException_KeyNotFoundException_ReturnsCorrectProblemDetails()
    {
        // Arrange
        SetupProductionEnvironment();
        var exception = new KeyNotFoundException();

        // Act
        var result = _factory.CreateFromException(_mockHttpContext.Object, exception);

        // Assert
        Assert.Equal((int)HttpStatusCode.NotFound, result.Status);
        Assert.Equal(ProblemDetailsConstants.Titles.NotFound, result.Title);
        Assert.Equal("The requested resource was not found.", result.Detail);
        Assert.Equal(ProblemDetailsConstants.Types.NotFound, result.Type);
    }

    [Fact]
    public void CreateFromException_InvalidOperationException_ReturnsCorrectProblemDetails()
    {
        // Arrange
        SetupProductionEnvironment();
        var exception = new InvalidOperationException();

        // Act
        var result = _factory.CreateFromException(_mockHttpContext.Object, exception);

        // Assert
        Assert.Equal((int)HttpStatusCode.InternalServerError, result.Status);
        Assert.Equal(ProblemDetailsConstants.Titles.InternalServerError, result.Title);
        Assert.Equal("An unexpected error occurred. Please contact support if the problem persists.", result.Detail);
        Assert.Equal(ProblemDetailsConstants.Types.InternalServerError, result.Type);
    }

    [Fact]
    public void CreateFromException_ArgumentException_ReturnsCorrectProblemDetails()
    {
        // Arrange
        SetupProductionEnvironment();
        var exception = new ArgumentException();

        // Act
        var result = _factory.CreateFromException(_mockHttpContext.Object, exception);

        // Assert
        Assert.Equal((int)HttpStatusCode.InternalServerError, result.Status);
        Assert.Equal(ProblemDetailsConstants.Titles.InternalServerError, result.Title);
        Assert.Equal("An unexpected error occurred. Please contact support if the problem persists.", result.Detail);
        Assert.Equal(ProblemDetailsConstants.Types.InternalServerError, result.Type);
    }

    [Fact]
    public void CreateFromException_UnknownException_ReturnsInternalServerError()
    {
        // Arrange
        SetupProductionEnvironment();
        var exception = new Exception("Unknown error");

        // Act
        var result = _factory.CreateFromException(_mockHttpContext.Object, exception);

        // Assert
        Assert.Equal((int)HttpStatusCode.InternalServerError, result.Status);
        Assert.Equal(ProblemDetailsConstants.Titles.InternalServerError, result.Title);
        Assert.Equal("An unexpected error occurred. Please contact support if the problem persists.", result.Detail);
        Assert.Equal(ProblemDetailsConstants.Types.InternalServerError, result.Type);
    }

    #endregion

    #region Validation Error Formatting Tests

    [Fact]
    public void CreateFromException_ValidationException_GroupsErrorsByProperty()
    {
        // Arrange
        SetupProductionEnvironment();
        var validationFailures = new List<ValidationFailure>
        {
            new("Name", "Name is required"),
            new("Name", "Name must be at least 3 characters"),
            new("Email", "Email is required")
        };
        var exception = new ValidationException(validationFailures);

        // Act
        var result = _factory.CreateFromException(_mockHttpContext.Object, exception);

        // Assert
        Assert.True(result.Extensions.ContainsKey("errors"));
        var errors = result.Extensions["errors"] as Dictionary<string, string[]>;
        Assert.NotNull(errors);
        Assert.Equal(2, errors.Count);
        Assert.Equal(2, errors["Name"].Length);
        Assert.Single(errors["Email"]);
    }

    [Fact]
    public void CreateFromException_ValidationException_EmptyErrors_IncludesEmptyErrorsCollection()
    {
        // Arrange
        SetupProductionEnvironment();
        var exception = new ValidationException(new List<ValidationFailure>());

        // Act
        var result = _factory.CreateFromException(_mockHttpContext.Object, exception);

        // Assert
        Assert.True(result.Extensions.ContainsKey("errors"));
        var errors = result.Extensions["errors"] as Dictionary<string, string[]>;
        Assert.NotNull(errors);
        Assert.Empty(errors);
    }

    #endregion
}
