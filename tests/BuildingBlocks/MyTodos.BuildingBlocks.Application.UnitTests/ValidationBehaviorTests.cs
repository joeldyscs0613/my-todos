using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using MyTodos.BuildingBlocks.Application.Behaviors;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.BuildingBlocks.Application.UnitTests;

public class ValidationBehaviorTests
{
    #region Test Helper Classes

    public class TestRequest : IBaseRequest
    {
        public string Value { get; set; } = string.Empty;
    }

    #endregion
    
    #region No Validators Tests

    [Fact]
    public async Task Handle_WithNoValidators_BypassesValidation()
    {
        // Arrange
        var validators = Enumerable.Empty<IValidator<TestRequest>>();
        var behavior = new ValidationBehavior<TestRequest, Result>(validators);
        var request = new TestRequest { Value = "test" };
        var expectedResult = Result.Success();
        RequestHandlerDelegate<Result> next = (ct) => Task.FromResult(expectedResult);

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    #endregion

    #region Validation Success Tests

    [Fact]
    public async Task Handle_WithValidRequest_ProceedsToNextHandler()
    {
        // Arrange
        var validator = new Mock<IValidator<TestRequest>>();
        var validators = new[] { validator.Object };
        var behavior = new ValidationBehavior<TestRequest, Result>(validators);
        var request = new TestRequest { Value = "test" };
        var expectedResult = Result.Success();
        RequestHandlerDelegate<Result> next = (ct) => Task.FromResult(expectedResult);

        validator.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        Assert.Equal(expectedResult, result);
        validator.Verify(x => x.ValidateAsync(
            It.IsAny<ValidationContext<TestRequest>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithMultipleValidatorsAllPassing_ProceedsToNextHandler()
    {
        // Arrange
        var validator1 = new Mock<IValidator<TestRequest>>();
        var validator2 = new Mock<IValidator<TestRequest>>();
        var validator3 = new Mock<IValidator<TestRequest>>();
        var validators = new[] { validator1.Object, validator2.Object, validator3.Object };
        var behavior = new ValidationBehavior<TestRequest, Result>(validators);
        var request = new TestRequest { Value = "test" };
        var expectedResult = Result.Success();
        RequestHandlerDelegate<Result> next = (ct) => Task.FromResult(expectedResult);

        validator1.Setup(x 
                => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        validator2.Setup(x 
                => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        validator3.Setup(x 
                => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        Assert.Equal(expectedResult, result);
        validator1.Verify(x 
            => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()), Times.Once);
        validator2.Verify(x 
            => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()), Times.Once);
        validator3.Verify(x 
            => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Validation Failure Tests

    [Fact]
    public async Task Handle_WithValidationFailure_ThrowsValidationException()
    {
        // Arrange
        var validator = new Mock<IValidator<TestRequest>>();
        var validators = new[] { validator.Object };
        var behavior = new ValidationBehavior<TestRequest, Result>(validators);
        var request = new TestRequest { Value = "" };
        RequestHandlerDelegate<Result> next = (ct) => Task.FromResult(Result.Success());

        var validationFailure = new ValidationFailure("Value", "Value is required");
        var validationResult = new ValidationResult(new[] { validationFailure });
        validator.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(request, next, CancellationToken.None));

        Assert.Single(exception.Errors);
        Assert.Contains(exception.Errors, e => e.PropertyName == "Value" && e.ErrorMessage == "Value is required");
    }

    [Fact]
    public async Task Handle_WithMultipleValidationFailures_ThrowsValidationExceptionWithAllErrors()
    {
        // Arrange
        var validator = new Mock<IValidator<TestRequest>>();
        var validators = new[] { validator.Object };
        var behavior = new ValidationBehavior<TestRequest, Result>(validators);
        var request = new TestRequest { Value = "" };
        RequestHandlerDelegate<Result> next = (ct) => Task.FromResult(Result.Success());

        var validationFailures = new[]
        {
            new ValidationFailure("Value", "Value is required"),
            new ValidationFailure("Value", "Value must be at least 3 characters"),
            new ValidationFailure("OtherProperty", "OtherProperty is invalid")
        };
        var validationResult = new ValidationResult(validationFailures);
        validator.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(request, next, CancellationToken.None));

        Assert.Equal(3, exception.Errors.Count());
        Assert.Contains(exception.Errors, 
            e => e.PropertyName == "Value" && e.ErrorMessage == "Value is required");
        Assert.Contains(exception.Errors, 
            e => e.PropertyName == "Value" && e.ErrorMessage == "Value must be at least 3 characters");
        Assert.Contains(exception.Errors, 
            e => e.PropertyName == "OtherProperty" && e.ErrorMessage == "OtherProperty is invalid");
    }

    [Fact]
    public async Task Handle_WithMultipleValidators_CollectsAllFailures()
    {
        // Arrange
        var validator1 = new Mock<IValidator<TestRequest>>();
        var validator2 = new Mock<IValidator<TestRequest>>();
        var validators = new[] { validator1.Object, validator2.Object };
        var behavior = new ValidationBehavior<TestRequest, Result>(validators);
        var request = new TestRequest { Value = "" };
        RequestHandlerDelegate<Result> next = (ct) => Task.FromResult(Result.Success());

        var failure1 = new ValidationFailure("Value", "Error from validator 1");
        var failure2 = new ValidationFailure("Value", "Error from validator 2");
        validator1.Setup(x 
                => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { failure1 }));
        validator2.Setup(x 
                => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { failure2 }));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(request, next, CancellationToken.None));

        Assert.Equal(2, exception.Errors.Count());
        Assert.Contains(exception.Errors, e => e.ErrorMessage == "Error from validator 1");
        Assert.Contains(exception.Errors, e => e.ErrorMessage == "Error from validator 2");
    }

    [Fact]
    public async Task Handle_WithMixedValidators_CollectsOnlyFailures()
    {
        // Arrange
        var validator1 = new Mock<IValidator<TestRequest>>();
        var validator2 = new Mock<IValidator<TestRequest>>();
        var validator3 = new Mock<IValidator<TestRequest>>();
        var validators = new[] { validator1.Object, validator2.Object, validator3.Object };
        var behavior = new ValidationBehavior<TestRequest, Result>(validators);
        var request = new TestRequest { Value = "test" };
        RequestHandlerDelegate<Result> next = (ct) => Task.FromResult(Result.Success());

        var failure = new ValidationFailure("Value", "Error from validator 2");
        validator1.Setup(x 
                => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult()); // Success
        validator2.Setup(x 
                => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { failure })); // Failure
        validator3.Setup(x 
                => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult()); // Success

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(request, next, CancellationToken.None));

        Assert.Single(exception.Errors);
        Assert.Contains(exception.Errors, e => e.ErrorMessage == "Error from validator 2");
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_PassesCancellationTokenToValidators()
    {
        // Arrange
        var validator = new Mock<IValidator<TestRequest>>();
        var validators = new[] { validator.Object };
        var behavior = new ValidationBehavior<TestRequest, Result>(validators);
        var request = new TestRequest { Value = "test" };
        var cts = new CancellationTokenSource();
        var token = cts.Token;
        RequestHandlerDelegate<Result> next = (ct) => Task.FromResult(Result.Success());

        validator.Setup(x 
                => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        await behavior.Handle(request, next, token);

        // Assert
        validator.Verify(x => x.ValidateAsync(
            It.IsAny<ValidationContext<TestRequest>>(),
            token), Times.Once);
    }

    [Fact]
    public async Task Handle_WithMultipleValidators_PassesCancellationTokenToAll()
    {
        // Arrange
        var validator1 = new Mock<IValidator<TestRequest>>();
        var validator2 = new Mock<IValidator<TestRequest>>();
        var validators = new[] { validator1.Object, validator2.Object };
        var behavior = new ValidationBehavior<TestRequest, Result>(validators);
        var request = new TestRequest { Value = "test" };
        var cts = new CancellationTokenSource();
        var token = cts.Token;
        RequestHandlerDelegate<Result> next = (ct) => Task.FromResult(Result.Success());

        validator1.Setup(x 
                => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        validator2.Setup(x 
                => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        await behavior.Handle(request, next, token);

        // Assert
        validator1.Verify(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), token), Times.Once);
        validator2.Verify(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), token), Times.Once);
    }

    #endregion
}
