using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using MyTodos.BuildingBlocks.Application.Behaviors;
using MyTodos.BuildingBlocks.Application.Contracts;
using MyTodos.BuildingBlocks.Application.Contracts.Commands;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.BuildingBlocks.Application.UnitTests;

public class UnitOfWorkBehaviorTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<DbContext> _dbContext;
    private readonly UnitOfWorkBehavior<TestCommand, Result> _behavior;
    private readonly UnitOfWorkBehavior<TestQuery, Result> _queryBehavior;
    private readonly UnitOfWorkBehavior<TestByNameCommand, Result> _commandByNameBehavior;

    public UnitOfWorkBehaviorTests()
    {
        _unitOfWork = new Mock<IUnitOfWork>();

        // Create mock transaction
        var mockTransaction = new Mock<IDbContextTransaction>();
        mockTransaction.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mockTransaction.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mockTransaction.Setup(t => t.DisposeAsync())
            .Returns(ValueTask.CompletedTask);

        // Create mock database facade
        var mockDatabase = new Mock<DatabaseFacade>(MockBehavior.Default, null);
        mockDatabase.Setup(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTransaction.Object);

        // Setup DbContext mock
        _dbContext = new Mock<DbContext>();
        _dbContext.Setup(c => c.Database)
            .Returns(mockDatabase.Object);

        _behavior = new UnitOfWorkBehavior<TestCommand, Result>(_unitOfWork.Object, _dbContext.Object);
        _queryBehavior = new UnitOfWorkBehavior<TestQuery, Result>(_unitOfWork.Object, _dbContext.Object);
        _commandByNameBehavior = new UnitOfWorkBehavior<TestByNameCommand, Result>(_unitOfWork.Object, _dbContext.Object);
    }

    #region Test Helper Classes

    private class TestCommand : ICommand, IRequest<Result>
    {
    }

    private class TestQuery : IRequest<Result>
    {
    }

    private class TestByNameCommand : IRequest<Result>
    {
    }

    #endregion
    
    #region Query Bypass Tests

    [Fact]
    public async Task Handle_WithQuery_BypassesUnitOfWork()
    {
        // Arrange
        var query = new TestQuery();
        var expectedResult = Result.Success();
        var nextCalled = false;
        RequestHandlerDelegate<Result> next = (ct) =>
        {
            nextCalled = true;
            return Task.FromResult(expectedResult);
        };

        // Act
        var result = await _queryBehavior.Handle(query, next, CancellationToken.None);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal(expectedResult, result);
        _unitOfWork.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Command Detection Tests

    [Fact]
    public async Task Handle_WithCommandImplementingICommand_UsesUnitOfWork()
    {
        // Arrange
        var command = new TestCommand();
        var expectedResult = Result.Success();
        RequestHandlerDelegate<Result> next = (ct) => Task.FromResult(expectedResult);
        _unitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _behavior.Handle(command, next, CancellationToken.None);

        // Assert
        Assert.Equal(expectedResult, result);
        _unitOfWork.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCommandByNamingConvention_UsesUnitOfWork()
    {
        // Arrange
        var command = new TestByNameCommand();
        var expectedResult = Result.Success();
        RequestHandlerDelegate<Result> next = (ct) => Task.FromResult(expectedResult);
        _unitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _commandByNameBehavior.Handle(command, next, CancellationToken.None);

        // Assert
        Assert.Equal(expectedResult, result);
        _unitOfWork.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Success Result Tests

    [Fact]
    public async Task Handle_WithSuccessResult_CommitsTransaction()
    {
        // Arrange
        var command = new TestCommand();
        var successResult = Result.Success();
        RequestHandlerDelegate<Result> next = (ct) => Task.FromResult(successResult);
        _unitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _behavior.Handle(command, next, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _unitOfWork.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithSuccessResultContainingValue_CommitsTransaction()
    {
        // Arrange
        // Create mock transaction for this test
        var mockTransaction = new Mock<IDbContextTransaction>();
        mockTransaction.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mockTransaction.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mockTransaction.Setup(t => t.DisposeAsync())
            .Returns(ValueTask.CompletedTask);

        // Create mock database facade for this test
        var mockDatabase = new Mock<DatabaseFacade>(MockBehavior.Default, null);
        mockDatabase.Setup(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTransaction.Object);

        // Setup DbContext mock for this test
        var dbContext = new Mock<DbContext>();
        dbContext.Setup(c => c.Database)
            .Returns(mockDatabase.Object);

        var behavior = new UnitOfWorkBehavior<TestCommand, Result<int>>(_unitOfWork.Object, dbContext.Object);
        var command = new TestCommand();
        var successResult = Result.Success(42);
        RequestHandlerDelegate<Result<int>> next = (ct) => Task.FromResult(successResult);
        _unitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
        _unitOfWork.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Failure Result Tests

    [Fact]
    public async Task Handle_WithFailureResult_DoesNotCommitTransaction()
    {
        // Arrange
        var command = new TestCommand();
        var failureResult = Result.BadRequest("Validation failed");
        RequestHandlerDelegate<Result> next = (ct) => Task.FromResult(failureResult);

        // Act
        var result = await _behavior.Handle(command, next, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Validation failed", result.FirstError.Description);
        _unitOfWork.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithFailureResultContainingValue_DoesNotCommitTransaction()
    {
        // Arrange
        // Create mock transaction for this test
        var mockTransaction = new Mock<IDbContextTransaction>();
        mockTransaction.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mockTransaction.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mockTransaction.Setup(t => t.DisposeAsync())
            .Returns(ValueTask.CompletedTask);

        // Create mock database facade for this test
        var mockDatabase = new Mock<DatabaseFacade>(MockBehavior.Default, null);
        mockDatabase.Setup(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTransaction.Object);

        // Setup DbContext mock for this test
        var dbContext = new Mock<DbContext>();
        dbContext.Setup(c => c.Database)
            .Returns(mockDatabase.Object);

        var behavior = new UnitOfWorkBehavior<TestCommand, Result<int>>(_unitOfWork.Object, dbContext.Object);
        var command = new TestCommand();
        var failureResult = Result.NotFound<int>("Entity not found");
        RequestHandlerDelegate<Result<int>> next = (ct) => Task.FromResult(failureResult);

        // Act
        var result = await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Entity not found", result.FirstError.Description);
        _unitOfWork.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithConflictResult_DoesNotCommitTransaction()
    {
        // Arrange
        var command = new TestCommand();
        var conflictResult = Result.Conflict("Resource already exists");
        RequestHandlerDelegate<Result> next = (ct) => Task.FromResult(conflictResult);

        // Act
        var result = await _behavior.Handle(command, next, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
        _unitOfWork.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_PassesCancellationTokenToCommitAsync()
    {
        // Arrange
        var command = new TestCommand();
        var cts = new CancellationTokenSource();
        var token = cts.Token;
        var successResult = Result.Success();
        RequestHandlerDelegate<Result> next = (ct) => Task.FromResult(successResult);
        _unitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _behavior.Handle(command, next, token);

        // Assert
        _unitOfWork.Verify(x => x.CommitAsync(token), Times.Once);
    }

    #endregion
}
