using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using MyTodos.BuildingBlocks.Presentation.Middleware;
using MyTodos.BuildingBlocks.Presentation.ProblemDetails;

namespace MyTodos.BuildingBlocks.Presentation.UnitTests;

public class GlobalExceptionHandlerTests
{
    private readonly Mock<ILogger<GlobalExceptionHandler>> _mockLogger;
    private readonly Mock<Microsoft.Extensions.Hosting.IHostEnvironment> _mockEnvironment;
    private readonly ProblemDetailsFactory _factory;
    private readonly GlobalExceptionHandler _handler;
    private readonly DefaultHttpContext _httpContext;

    public GlobalExceptionHandlerTests()
    {
        _mockLogger = new Mock<ILogger<GlobalExceptionHandler>>();
        _mockEnvironment = new Mock<Microsoft.Extensions.Hosting.IHostEnvironment>();
        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Production");
        _factory = new ProblemDetailsFactory(_mockEnvironment.Object);
        _handler = new GlobalExceptionHandler(_mockLogger.Object, _factory);
        _httpContext = new DefaultHttpContext();
        _httpContext.Response.Body = new MemoryStream();
    }

    #region Exception Handling Tests

    [Fact]
    public async Task TryHandleAsync_WithAnyException_ReturnsTrue()
    {
        // Arrange
        var exception = new Exception("Test exception");

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task TryHandleAsync_WithKeyNotFoundException_SetsNotFoundStatusCode()
    {
        // Arrange
        var exception = new KeyNotFoundException("Test not found exception");

        // Act
        await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        Assert.Equal(404, _httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_WithUnhandledException_SetsInternalServerErrorStatusCode()
    {
        // Arrange
        var exception = new Exception("Unhandled exception");

        // Act
        await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        Assert.Equal(500, _httpContext.Response.StatusCode);
    }

    #endregion

    #region Logging Tests

    [Fact]
    public async Task TryHandleAsync_WithNonMediatRException_LogsError()
    {
        // Arrange
        var exception = new Exception("Non-MediatR exception");

        // Act
        await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(
                    "Unhandled exception occurred outside MediatR pipeline")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task TryHandleAsync_WithMediatRException_LogsDebug()
    {
        // Arrange
        var exception = new Exception("MediatR exception");
        exception.Data["MediatRPipeline"] = true;

        // Act
        await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(
                    "Converting MediatR exception to Problem Details response")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task TryHandleAsync_WithExceptionContainingMediatRInStackTrace_LogsDebug()
    {
        // Arrange
        var exception = CreateExceptionWithStackTrace("at MediatR.Pipeline.RequestHandler.Handle()");

        // Act
        await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(
                    "Converting MediatR exception to Problem Details response")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Helper Methods

    private static Exception CreateExceptionWithStackTrace(string stackTrace)
    {
        try
        {
            throw new Exception("Test exception with stack trace");
        }
        catch (Exception ex)
        {
            // Use reflection to set the StackTrace property
            var stackTraceField = typeof(Exception).GetField("_stackTraceString",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            stackTraceField?.SetValue(ex, stackTrace);
            return ex;
        }
    }

    #endregion
}
