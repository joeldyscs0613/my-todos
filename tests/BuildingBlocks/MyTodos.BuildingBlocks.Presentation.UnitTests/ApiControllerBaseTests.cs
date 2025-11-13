using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MyTodos.BuildingBlocks.Application.Helpers;
using MyTodos.BuildingBlocks.Presentation.Constants;
using MyTodos.BuildingBlocks.Presentation.Controllers;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.BuildingBlocks.Presentation.UnitTests;

public class ApiControllerBaseTests
{
    private readonly TestController _controller;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IUrlHelper> _mockUrlHelper;
    private readonly ServiceCollection _services;

    public ApiControllerBaseTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockUrlHelper = new Mock<IUrlHelper>();

        _services = new ServiceCollection();
        _services.AddSingleton(_mockMediator.Object);

        var serviceProvider = _services.BuildServiceProvider();
        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider
        };

        _controller = new TestController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext,
                ActionDescriptor = new ControllerActionDescriptor()
            },
            Url = _mockUrlHelper.Object
        };
    }

    #region Mediator Tests

    [Fact]
    public void Mediator_LazyLoadsFromRequestServices()
    {
        // Act
        var mediator = _controller.ExposedMediator;

        // Assert
        Assert.NotNull(mediator);
        Assert.Same(_mockMediator.Object, mediator);
    }

    [Fact]
    public void Mediator_CachesInstanceOnSubsequentCalls()
    {
        // Act
        var mediator1 = _controller.ExposedMediator;
        var mediator2 = _controller.ExposedMediator;

        // Assert
        Assert.Same(mediator1, mediator2);
    }

    #endregion

    #region HandleResult(Result) Tests

    [Fact]
    public void HandleResult_WithSuccessResult_ReturnsNoContent()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var actionResult = _controller.ExposedHandleResult(result);

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(actionResult);
        Assert.Equal(204, statusCodeResult.StatusCode);
    }

    [Fact]
    public void HandleResult_WithFailureResult_ReturnsProblemDetails()
    {
        // Arrange
        var error = Error.NotFound("Entity with ID 123 was not found");
        var result = Result.Failure(error);

        // Act
        var actionResult = _controller.ExposedHandleResult(result);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
        var problemDetails = Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(badRequestResult.Value);
        Assert.Equal(404, problemDetails.Status);
        Assert.Equal(404, badRequestResult.StatusCode);
    }

    #endregion

    #region HandleResult<TValue>(Result<TValue>) Tests

    [Fact]
    public void HandleResult_WithSuccessResultOfValue_ReturnsOkWithValue()
    {
        // Arrange
        var value = new TestDto { Id = Guid.NewGuid(), Name = "Test" };
        var result = Result.Success(value);

        // Act
        var actionResult = _controller.ExposedHandleResultOfValue(result);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(200, okResult.StatusCode);
        var returnedValue = Assert.IsType<TestDto>(okResult.Value);
        Assert.Equal(value.Id, returnedValue.Id);
        Assert.Equal(value.Name, returnedValue.Name);
    }

    [Fact]
    public void HandleResult_WithFailureResultOfValue_ReturnsProblemDetails()
    {
        // Arrange
        var error = Error.BadRequest("Name is required");
        var result = Result.Failure<TestDto>(error);

        // Act
        var actionResult = _controller.ExposedHandleResultOfValue(result);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var problemDetails = Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(badRequestResult.Value);
        Assert.Equal(400, problemDetails.Status);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    #endregion

    #region HandleCreatedResult Tests

    [Fact]
    public void HandleCreatedResult_WithSuccessResult_ReturnsCreatedAtRoute()
    {
        // Arrange
        var id = Guid.NewGuid();
        var value = new TestDto { Id = id, Name = "Test" };
        var result = Result.Success(value);
        var routeName = "TestRoute";
        var expectedUrl = $"https://api.example.com/api/test/{id}";

        _mockUrlHelper
            .Setup(x => x.Link(routeName, It.IsAny<object>()))
            .Returns(expectedUrl);

        // Act
        var actionResult = _controller.ExposedHandleCreatedResult(result, routeName, id);

        // Assert
        var createdResult = Assert.IsType<CreatedAtRouteResult>(actionResult.Result);
        Assert.Equal(routeName, createdResult.RouteName);
        Assert.Equal(201, createdResult.StatusCode);
        var returnedValue = Assert.IsType<TestDto>(createdResult.Value);
        Assert.Equal(value.Id, returnedValue.Id);
        Assert.Equal(value.Name, returnedValue.Name);
    }

    [Fact]
    public void HandleCreatedResult_WithFailureResult_ReturnsProblemDetails()
    {
        // Arrange
        var error = Error.Conflict("Email already exists");
        var result = Result.Failure<TestDto>(error);
        var routeName = "TestRoute";
        var id = Guid.NewGuid();

        // Act
        var actionResult = _controller.ExposedHandleCreatedResult(result, routeName, id);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var problemDetails = Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(badRequestResult.Value);
        Assert.Equal(409, problemDetails.Status);
        Assert.Equal(409, badRequestResult.StatusCode);
    }

    [Fact]
    public void HandleCreatedResult_WithSuccessResult_PassesIdToRouteValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var value = new TestDto { Id = id, Name = "Test" };
        var result = Result.Success(value);
        var routeName = "TestRoute";

        // Act
        var actionResult = _controller.ExposedHandleCreatedResult(result, routeName, id);

        // Assert
        var createdResult = Assert.IsType<CreatedAtRouteResult>(actionResult.Result);
        Assert.NotNull(createdResult.RouteValues);
        Assert.True(createdResult.RouteValues.ContainsKey("id"));
        Assert.Equal(id, createdResult.RouteValues["id"]);
    }

    #endregion

    #region HandlePagedResult Tests

    [Fact]
    public void HandlePagedResult_WithSuccessResult_ReturnsOkWithItemsAndAddsHeaders()
    {
        // Arrange
        var items = new List<TestDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Test1" },
            new() { Id = Guid.NewGuid(), Name = "Test2" }
        };
        var pagedList = PagedList<TestDto>.Create(
            items,
            totalCount: 25,
            pageNumber: 1,
            pageSize: 10,
            sortField: "Name",
            sortDirection: "ASC");
        var result = Result.Success(pagedList);

        // Act
        var actionResult = _controller.ExposedHandlePagedResult(result);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(200, okResult.StatusCode);

        var returnedItems = Assert.IsAssignableFrom<IEnumerable<TestDto>>(okResult.Value);
        Assert.Equal(2, returnedItems.Count());

        // Verify pagination headers were added
        var response = _controller.Response;
        Assert.Equal("1", response.Headers[HeaderConstants.Pagination.CurrentPage]);
        Assert.Equal("10", response.Headers[HeaderConstants.Pagination.PageSize]);
        Assert.Equal("3", response.Headers[HeaderConstants.Pagination.TotalPages]);
        Assert.Equal("25", response.Headers[HeaderConstants.Pagination.TotalCount]);
        Assert.Equal("false", response.Headers[HeaderConstants.Pagination.HasPrevious]);
        Assert.Equal("true", response.Headers[HeaderConstants.Pagination.HasNext]);
        Assert.Equal("Name", response.Headers[HeaderConstants.Pagination.SortField]);
        Assert.Equal("asc", response.Headers[HeaderConstants.Pagination.SortDirection]);
    }

    [Fact]
    public void HandlePagedResult_WithFailureResult_ReturnsProblemDetails()
    {
        // Arrange
        var error = Error.BadRequest("Page number must be greater than 0");
        var result = Result.Failure<PagedList<TestDto>>(error);

        // Act
        var actionResult = _controller.ExposedHandlePagedResult(result);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var problemDetails = Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(badRequestResult.Value);
        Assert.Equal(400, problemDetails.Status);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public void HandlePagedResult_WithEmptyPagedList_ReturnsOkWithEmptyCollection()
    {
        // Arrange
        var items = new List<TestDto>();
        var pagedList = PagedList<TestDto>.Create(
            items,
            totalCount: 0,
            pageNumber: 1,
            pageSize: 10,
            sortField: null,
            sortDirection: null);
        var result = Result.Success(pagedList);

        // Act
        var actionResult = _controller.ExposedHandlePagedResult(result);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedItems = Assert.IsAssignableFrom<IEnumerable<TestDto>>(okResult.Value);
        Assert.Empty(returnedItems);

        // Verify pagination headers for empty result
        Assert.Equal("0", _controller.Response.Headers[HeaderConstants.Pagination.TotalCount]);
        Assert.Equal("0", _controller.Response.Headers[HeaderConstants.Pagination.TotalPages]);
    }

    #endregion

    #region Test Infrastructure

    // Concrete implementation for testing
    private class TestController : ApiControllerBase
    {
        // Expose protected members for testing
        public IMediator ExposedMediator => Mediator;

        public ActionResult ExposedHandleResult(Result result)
            => HandleResult(result);

        public ActionResult<TestDto> ExposedHandleResultOfValue(Result<TestDto> result)
            => HandleResult(result);

        public ActionResult<TestDto> ExposedHandleCreatedResult(
            Result<TestDto> result,
            string routeName,
            Guid? id)
            => HandleCreatedResult(result, routeName, id);

        public ActionResult<IEnumerable<TestDto>> ExposedHandlePagedResult(
            Result<PagedList<TestDto>> result)
            => HandlePagedResult(result);
    }

    private class TestDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
    }

    #endregion
}
