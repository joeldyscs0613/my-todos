using MyTodos.SharedKernel.Helpers;

namespace MyTodos.SharedKernel.UnitTests;

public class ResultTests
{
    #region Basic Success/Failure

    [Fact]
    public void Success_CreatesSuccessfulResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Empty(result.Errors);
        Assert.Equal(Error.None, result.FirstError);
    }

    [Fact]
    public void Success_WithValue_CreatesSuccessfulResult()
    {
        // Arrange
        var value = 42;

        // Act
        var result = Result.Success(value);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(value, result.Value);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Failure_WithSingleError_CreatesFailureResult()
    {
        // Arrange
        var error = Error.NotFound("Item not found.");

        // Act
        var result = Result.Failure(error);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Single(result.Errors);
        Assert.Equal(error, result.FirstError);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public void Failure_WithMultipleErrors_CreatesFailureResult()
    {
        // Arrange
        var errors = new List<Error>
        {
            Error.BadRequest("First error"),
            Error.BadRequest("Second error")
        };

        // Act
        var result = Result.Failure(errors);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(2, result.Errors.Count);
        Assert.Equal(errors[0], result.FirstError);
        Assert.Equal(ErrorType.BadRequest, result.ErrorType);
    }

    [Fact]
    public void Failure_Generic_WithSingleError_CreatesFailureResult()
    {
        // Arrange
        var error = Error.NotFound("Item not found.");

        // Act
        var result = Result.Failure<int>(error);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Single(result.Errors);
        Assert.Equal(error, result.FirstError);
    }

    #endregion

    #region Properties

    [Fact]
    public void ErrorDescriptions_ReturnsAllErrorDescriptions()
    {
        // Arrange
        var errors = new List<Error>
        {
            Error.BadRequest("First error"),
            Error.BadRequest("Second error"),
            Error.BadRequest("Third error")
        };

        // Act
        var result = Result.Failure(errors);

        // Assert
        Assert.Equal(3, result.ErrorDescriptions.Count);
        Assert.Contains("First error", result.ErrorDescriptions);
        Assert.Contains("Second error", result.ErrorDescriptions);
        Assert.Contains("Third error", result.ErrorDescriptions);
    }

    [Fact]
    public void ErrorType_WithSingleError_ReturnsErrorType()
    {
        // Arrange
        var error = Error.NotFound();

        // Act
        var result = Result.Failure(error);

        // Assert
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public void ErrorType_WithMultipleErrors_ReturnsBadRequest()
    {
        // Arrange
        var errors = new List<Error>
        {
            Error.NotFound(),
            Error.Conflict()
        };

        // Act
        var result = Result.Failure(errors);

        // Assert
        Assert.Equal(ErrorType.BadRequest, result.ErrorType);
    }

    [Fact]
    public void FirstError_WithNoErrors_ReturnsErrorNone()
    {
        // Act
        var result = Result.Success();

        // Assert
        Assert.Equal(Error.None, result.FirstError);
    }

    #endregion

    #region Factory Methods - NotFound

    [Fact]
    public void NotFound_WithDefaultDescription_CreatesNotFoundResult()
    {
        // Act
        var result = Result.NotFound();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
        Assert.Equal("Not found.", result.FirstError.Description);
    }

    [Fact]
    public void NotFound_WithCustomDescription_CreatesNotFoundResult()
    {
        // Arrange
        var description = "User not found.";

        // Act
        var result = Result.NotFound(description);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
        Assert.Equal(description, result.FirstError.Description);
    }

    [Fact]
    public void NotFound_Generic_CreatesNotFoundResult()
    {
        // Act
        var result = Result.NotFound<int>();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    #endregion

    #region Factory Methods - Conflict

    [Fact]
    public void Conflict_WithDefaultDescription_CreatesConflictResult()
    {
        // Act
        var result = Result.Conflict();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
        Assert.Equal("The entity already exists.", result.FirstError.Description);
    }

    [Fact]
    public void Conflict_WithCustomDescription_CreatesConflictResult()
    {
        // Arrange
        var description = "Email already exists.";

        // Act
        var result = Result.Conflict(description);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
        Assert.Equal(description, result.FirstError.Description);
    }

    [Fact]
    public void Conflict_Generic_CreatesConflictResult()
    {
        // Act
        var result = Result.Conflict<int>();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
    }

    #endregion

    #region Factory Methods - Unauthorized

    [Fact]
    public void Unauthorized_WithDefaultDescription_CreatesUnauthorizedResult()
    {
        // Act
        var result = Result.Unauthorized();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unauthorized, result.ErrorType);
        Assert.Equal("Unauthorized.", result.FirstError.Description);
    }

    [Fact]
    public void Unauthorized_WithCustomDescription_CreatesUnauthorizedResult()
    {
        // Arrange
        var description = "Invalid token.";

        // Act
        var result = Result.Unauthorized(description);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unauthorized, result.ErrorType);
        Assert.Equal(description, result.FirstError.Description);
    }

    [Fact]
    public void Unauthorized_Generic_CreatesUnauthorizedResult()
    {
        // Act
        var result = Result.Unauthorized<int>();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unauthorized, result.ErrorType);
    }

    #endregion

    #region Factory Methods - Forbidden

    [Fact]
    public void Forbidden_WithDefaultDescription_CreatesForbiddenResult()
    {
        // Act
        var result = Result.Forbidden();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, result.ErrorType);
        Assert.Equal("Forbidden.", result.FirstError.Description);
    }

    [Fact]
    public void Forbidden_WithCustomDescription_CreatesForbiddenResult()
    {
        // Arrange
        var description = "Access denied.";

        // Act
        var result = Result.Forbidden(description);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, result.ErrorType);
        Assert.Equal(description, result.FirstError.Description);
    }

    [Fact]
    public void Forbidden_Generic_CreatesForbiddenResult()
    {
        // Act
        var result = Result.Forbidden<int>();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, result.ErrorType);
    }

    #endregion

    #region Factory Methods - BadRequest

    [Fact]
    public void BadRequest_WithDescription_CreatesBadRequestResult()
    {
        // Arrange
        var description = "Invalid input.";

        // Act
        var result = Result.BadRequest(description);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.BadRequest, result.ErrorType);
        Assert.Equal(description, result.FirstError.Description);
    }

    [Fact]
    public void BadRequest_Generic_WithDescription_CreatesBadRequestResult()
    {
        // Arrange
        var description = "Invalid input.";

        // Act
        var result = Result.BadRequest<int>(description);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.BadRequest, result.ErrorType);
        Assert.Equal(description, result.FirstError.Description);
    }

    [Fact]
    public void BadRequest_WithListOfErrors_CreatesBadRequestResult()
    {
        // Arrange
        var errors = new List<Error>
        {
            Error.NotFound("Item not found"),
            Error.Conflict("Item exists")
        };

        // Act
        var result = Result.BadRequest(errors);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.BadRequest, result.ErrorType);
        Assert.Equal(2, result.Errors.Count);
        Assert.All(result.Errors, e => Assert.Equal(ErrorType.BadRequest, e.Type));
    }

    [Fact]
    public void BadRequest_Generic_WithListOfDescriptions_CreatesBadRequestResult()
    {
        // Arrange
        var descriptions = new List<string> { "Error 1", "Error 2", "Error 3" };

        // Act
        var result = Result.BadRequest<int>(descriptions);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.BadRequest, result.ErrorType);
        Assert.Equal(3, result.Errors.Count);
        Assert.Contains("Error 1", result.ErrorDescriptions);
        Assert.Contains("Error 2", result.ErrorDescriptions);
        Assert.Contains("Error 3", result.ErrorDescriptions);
    }

    [Fact]
    public void BadRequest_Generic_WithListOfErrors_CreatesBadRequestResult()
    {
        // Arrange
        var errors = new List<Error>
        {
            Error.NotFound("Item not found"),
            Error.Conflict("Item exists")
        };

        // Act
        var result = Result.BadRequest<int>(errors);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.BadRequest, result.ErrorType);
        Assert.Equal(2, result.Errors.Count);
        Assert.All(result.Errors, e => Assert.Equal(ErrorType.BadRequest, e.Type));
    }

    #endregion

    #region Factory Methods - NullValue

    [Fact]
    public void NullValue_CreatesNullValueResult()
    {
        // Act
        var result = Result.NullValue();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NullValue, result.ErrorType);
        Assert.Equal("Null value was provided", result.FirstError.Description);
    }

    #endregion

    #region Result<T> Specific

    [Fact]
    public void Value_OnSuccess_ReturnsValue()
    {
        // Arrange
        var expectedValue = 42;
        var result = Result.Success(expectedValue);

        // Act
        var value = result.Value;

        // Assert
        Assert.Equal(expectedValue, value);
    }

    [Fact]
    public void Value_OnFailure_ThrowsInvalidOperationException()
    {
        // Arrange
        var result = Result.Failure<int>(Error.NotFound());

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => result.Value);
        Assert.Equal("The value of a failure result can't be accessed.", exception.Message);
    }

    [Fact]
    public void ImplicitOperator_WithNonNullValue_CreatesSuccessResult()
    {
        // Arrange
        int value = 42;

        // Act
        Result<int> result = value;

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(value, result.Value);
    }

    [Fact]
    public void ImplicitOperator_WithNullValue_CreatesFailureResult()
    {
        // Arrange
        string? value = null;

        // Act
        Result<string> result = value;

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NullValue, result.ErrorType);
    }

    [Fact]
    public void Create_WithNonNullValue_CreatesSuccessResult()
    {
        // Arrange
        var value = "test";

        // Act
        var result = Result.Create(value);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(value, result.Value);
    }

    [Fact]
    public void Create_WithNullValue_CreatesFailureResult()
    {
        // Arrange
        string? value = null;

        // Act
        var result = Result.Create(value);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NullValue, result.ErrorType);
    }

    #endregion

    #region Pattern Matching

    [Fact]
    public void Match_OnSuccess_CallsOnSuccessAction()
    {
        // Arrange
        var result = Result.Success();
        var successCalled = false;
        var failureCalled = false;

        // Act
        result.Match(
            onSuccess: () => successCalled = true,
            onFailure: _ => failureCalled = true
        );

        // Assert
        Assert.True(successCalled);
        Assert.False(failureCalled);
    }

    [Fact]
    public void Match_OnFailure_CallsOnFailureAction()
    {
        // Arrange
        var result = Result.Failure(Error.NotFound());
        var successCalled = false;
        var failureCalled = false;
        List<Error>? capturedErrors = null;

        // Act
        result.Match(
            onSuccess: () => successCalled = true,
            onFailure: errors =>
            {
                failureCalled = true;
                capturedErrors = errors;
            }
        );

        // Assert
        Assert.False(successCalled);
        Assert.True(failureCalled);
        Assert.NotNull(capturedErrors);
        Assert.Single(capturedErrors);
    }

    [Fact]
    public void Match_Generic_OnSuccess_CallsOnSuccessFunction()
    {
        // Arrange
        var value = 42;
        var result = Result.Success(value);

        // Act
        var output = result.Match(
            onSuccess: v => v * 2,
            onFailure: _ => 0
        );

        // Assert
        Assert.Equal(84, output);
    }

    [Fact]
    public void Match_Generic_OnFailure_CallsOnFailureFunction()
    {
        // Arrange
        var result = Result.Failure<int>(Error.NotFound());

        // Act
        var output = result.Match(
            onSuccess: v => v * 2,
            onFailure: errors => errors.Count
        );

        // Assert
        Assert.Equal(1, output);
    }

    [Fact]
    public void Match_Generic_WithAction_OnSuccess_CallsOnSuccessAction()
    {
        // Arrange
        var value = 42;
        var result = Result.Success(value);
        var successCalled = false;
        var capturedValue = 0;

        // Act
        result.Match(
            onSuccess: v =>
            {
                successCalled = true;
                capturedValue = v;
            },
            onFailure: _ => { }
        );

        // Assert
        Assert.True(successCalled);
        Assert.Equal(value, capturedValue);
    }

    [Fact]
    public void Match_Generic_WithAction_OnFailure_CallsOnFailureAction()
    {
        // Arrange
        var result = Result.Failure<int>(Error.NotFound());
        var failureCalled = false;
        List<Error>? capturedErrors = null;

        // Act
        result.Match(
            onSuccess: _ => { },
            onFailure: errors =>
            {
                failureCalled = true;
                capturedErrors = errors;
            }
        );

        // Assert
        Assert.True(failureCalled);
        Assert.NotNull(capturedErrors);
        Assert.Single(capturedErrors);
    }

    #endregion

    #region Immutability

    [Fact]
    public void Errors_IsReadOnly()
    {
        // Arrange
        var errors = new List<Error> { Error.BadRequest("Error 1") };
        var result = Result.Failure(errors);

        // Act & Assert
        Assert.IsAssignableFrom<IReadOnlyList<Error>>(result.Errors);
    }

    [Fact]
    public void ErrorDescriptions_IsReadOnly()
    {
        // Arrange
        var errors = new List<Error> { Error.BadRequest("Error 1") };
        var result = Result.Failure(errors);

        // Act & Assert
        Assert.IsAssignableFrom<IReadOnlyList<string>>(result.ErrorDescriptions);
    }

    #endregion
}
