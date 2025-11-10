using MyTodos.SharedKernel.Helpers;

namespace MyTodos.SharedKernel.UnitTests;

public class ErrorTests
{
    #region Static Error Properties

    [Fact]
    public void None_HasCorrectTypeAndDescription()
    {
        // Assert
        Assert.Equal(ErrorType.InternalServerError, Error.None.Type);
        Assert.Equal(string.Empty, Error.None.Description);
    }

    [Fact]
    public void NullValue_HasCorrectTypeAndDescription()
    {
        // Assert
        Assert.Equal(ErrorType.NullValue, Error.NullValue.Type);
        Assert.Equal("Null value was provided", Error.NullValue.Description);
    }

    #endregion

    #region Factory Methods - NotFound

    [Fact]
    public void NotFound_WithDefaultDescription_CreatesNotFoundError()
    {
        // Act
        var error = Error.NotFound();

        // Assert
        Assert.Equal(ErrorType.NotFound, error.Type);
        Assert.Equal("Not found.", error.Description);
    }

    [Fact]
    public void NotFound_WithCustomDescription_CreatesNotFoundError()
    {
        // Arrange
        var customDescription = "User not found.";

        // Act
        var error = Error.NotFound(customDescription);

        // Assert
        Assert.Equal(ErrorType.NotFound, error.Type);
        Assert.Equal(customDescription, error.Description);
    }

    #endregion

    #region Factory Methods - Conflict

    [Fact]
    public void Conflict_WithDefaultDescription_CreatesConflictError()
    {
        // Act
        var error = Error.Conflict();

        // Assert
        Assert.Equal(ErrorType.Conflict, error.Type);
        Assert.Equal("The entity already exists.", error.Description);
    }

    [Fact]
    public void Conflict_WithCustomDescription_CreatesConflictError()
    {
        // Arrange
        var customDescription = "Email already in use.";

        // Act
        var error = Error.Conflict(customDescription);

        // Assert
        Assert.Equal(ErrorType.Conflict, error.Type);
        Assert.Equal(customDescription, error.Description);
    }

    #endregion

    #region Factory Methods - Unauthorized

    [Fact]
    public void Unauthorized_WithDefaultDescription_CreatesUnauthorizedError()
    {
        // Act
        var error = Error.Unauthorized();

        // Assert
        Assert.Equal(ErrorType.Unauthorized, error.Type);
        Assert.Equal("Unauthorized.", error.Description);
    }

    [Fact]
    public void Unauthorized_WithCustomDescription_CreatesUnauthorizedError()
    {
        // Arrange
        var customDescription = "Invalid credentials.";

        // Act
        var error = Error.Unauthorized(customDescription);

        // Assert
        Assert.Equal(ErrorType.Unauthorized, error.Type);
        Assert.Equal(customDescription, error.Description);
    }

    #endregion

    #region Factory Methods - Forbidden

    [Fact]
    public void Forbidden_WithDefaultDescription_CreatesForbiddenError()
    {
        // Act
        var error = Error.Forbidden();

        // Assert
        Assert.Equal(ErrorType.Forbidden, error.Type);
        Assert.Equal("Forbidden.", error.Description);
    }

    [Fact]
    public void Forbidden_WithCustomDescription_CreatesForbiddenError()
    {
        // Arrange
        var customDescription = "Insufficient permissions.";

        // Act
        var error = Error.Forbidden(customDescription);

        // Assert
        Assert.Equal(ErrorType.Forbidden, error.Type);
        Assert.Equal(customDescription, error.Description);
    }

    #endregion

    #region Factory Methods - BadRequest

    [Fact]
    public void BadRequest_WithDescription_CreatesBadRequestError()
    {
        // Arrange
        var description = "Invalid input.";

        // Act
        var error = Error.BadRequest(description);

        // Assert
        Assert.Equal(ErrorType.BadRequest, error.Type);
        Assert.Equal(description, error.Description);
    }

    #endregion

    #region Record Equality

    [Fact]
    public void Errors_WithSameValues_AreEqual()
    {
        // Arrange
        var error1 = Error.NotFound("User not found.");
        var error2 = Error.NotFound("User not found.");

        // Assert
        Assert.Equal(error1, error2);
    }

    [Fact]
    public void Errors_WithDifferentTypes_AreNotEqual()
    {
        // Arrange
        var error1 = Error.NotFound("Not found.");
        var error2 = Error.Conflict("Not found.");

        // Assert
        Assert.NotEqual(error1, error2);
    }

    [Fact]
    public void Errors_WithDifferentDescriptions_AreNotEqual()
    {
        // Arrange
        var error1 = Error.NotFound("User not found.");
        var error2 = Error.NotFound("Task not found.");

        // Assert
        Assert.NotEqual(error1, error2);
    }

    #endregion
}
