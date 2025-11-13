using FluentValidation;
using FluentValidation.TestHelper;
using MyTodos.BuildingBlocks.Application.Abstractions.Filters;
using MyTodos.BuildingBlocks.Application.Abstractions.Queries;
using MyTodos.BuildingBlocks.Application.Abstractions.Specifications;
using MyTodos.BuildingBlocks.Application.Constants;
using MyTodos.BuildingBlocks.Application.Helpers;
using MyTodos.BuildingBlocks.Application.Validators;
using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.BuildingBlocks.Application.UnitTests;

public class PagedListQueryValidatorTests
{
    #region Test Classes

    private class TestFilter : Filter
    {
    }

    private class TestEntity : Entity<int>
    {
        public string Name { get; set; } = string.Empty;

        public TestEntity(int id) : base(id) { }
    }

    private class TestSpecification : Specification<TestEntity, int, TestFilter>
    {
        public TestSpecification(TestFilter filter) : base(filter) { }

        // No longer need to override ApplyIncludes - base class provides default implementation

        protected override IQueryable<TestEntity> ApplyFilter(IQueryable<TestEntity> query)
        {
            return query;
        }

        protected override IQueryable<TestEntity> ApplySearchBy(IQueryable<TestEntity> query)
        {
            return query;
        }

        protected override Dictionary<string, System.Linq.Expressions.Expression<Func<TestEntity, object>>> GetSortFunctions()
        {
            return new Dictionary<string, System.Linq.Expressions.Expression<Func<TestEntity, object>>>
            {
                { "Name", e => e.Name },
                { "Id", e => e.Id }
            };
        }
    }

    private class TestQuery : PagedListQuery<TestSpecification, TestFilter, TestDto>
    {
        public TestQuery(TestFilter filter) : base(filter) { }
    }

    private class TestDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class TestQueryValidator : PagedListQueryValidator<TestQuery, TestSpecification, TestFilter, TestDto>
    {
        public TestQueryValidator()
        {
            // Add sort field validation
            RuleFor(x => x.Filter.SortField)
                .Must((query, sortField) => BeValidSortField(sortField, query.Specification.ValidSortFields))
                .When(x => x.Filter != null && !string.IsNullOrWhiteSpace(x.Filter.SortField))
                .WithMessage((query, sortField) =>
                    GetInvalidSortFieldMessage(sortField!, query.Specification.ValidSortFields));
        }
    }

    #endregion

    private readonly TestQueryValidator _validator;

    public PagedListQueryValidatorTests()
    {
        _validator = new TestQueryValidator();
    }

    #region PageSize Validation Tests

    [Fact]
    public void Validate_WithPageSizeTooLarge_ShouldHaveValidationError()
    {
        // Arrange
        var filter = new TestFilter { PageSize = 51 };
        var query = new TestQuery(filter);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Filter.PageSize)
            .WithErrorMessage($"Page size must not exceed {PageListConstants.MaxPageSize}.");
    }

    [Fact]
    public void Validate_WithMaxPageSize_ShouldNotHaveValidationError()
    {
        // Arrange
        var filter = new TestFilter { PageSize = PageListConstants.MaxPageSize };
        var query = new TestQuery(filter);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Filter.PageSize);
    }

    #endregion

    #region SortDirection Tests

    [Fact]
    public void Validate_WithValidSortDirection_Asc_ShouldNotHaveValidationError()
    {
        // Arrange
        var filter = new TestFilter { SortDirection = "asc" };
        var query = new TestQuery(filter);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Filter.SortDirection);
    }

    [Fact]
    public void Validate_WithValidSortDirection_Desc_ShouldNotHaveValidationError()
    {
        // Arrange
        var filter = new TestFilter { SortDirection = "desc" };
        var query = new TestQuery(filter);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Filter.SortDirection);
    }

    [Fact]
    public void Validate_WithValidSortDirection_CaseInsensitive_ShouldNotHaveValidationError()
    {
        // Arrange
        var filter1 = new TestFilter { SortDirection = "ASC" };
        var filter2 = new TestFilter { SortDirection = "DESC" };
        var query1 = new TestQuery(filter1);
        var query2 = new TestQuery(filter2);

        // Act
        var result1 = _validator.TestValidate(query1);
        var result2 = _validator.TestValidate(query2);

        // Assert
        result1.ShouldNotHaveValidationErrorFor(x => x.Filter.SortDirection);
        result2.ShouldNotHaveValidationErrorFor(x => x.Filter.SortDirection);
    }

    [Fact]
    public void Validate_WithInvalidSortDirection_ShouldHaveValidationError()
    {
        // Arrange
        var filter = new TestFilter { SortDirection = "invalid" };
        var query = new TestQuery(filter);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Filter.SortDirection)
            .WithErrorMessage("Sort direction must be either 'asc' or 'desc'.");
    }

    [Fact]
    public void Validate_WithNullSortDirection_ShouldNotHaveValidationError()
    {
        // Arrange
        var filter = new TestFilter { SortDirection = null };
        var query = new TestQuery(filter);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Filter.SortDirection);
    }

    [Fact]
    public void Validate_WithEmptySortDirection_ShouldNotHaveValidationError()
    {
        // Arrange
        var filter = new TestFilter { SortDirection = string.Empty };
        var query = new TestQuery(filter);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Filter.SortDirection);
    }

    #endregion

    #region SortField Tests

    [Fact]
    public void Validate_WithValidSortField_ShouldNotHaveValidationError()
    {
        // Arrange
        var filter = new TestFilter { SortField = "Name" };
        var query = new TestQuery(filter);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Filter.SortField);
    }

    [Fact]
    public void Validate_WithValidSortField_CaseInsensitive_ShouldNotHaveValidationError()
    {
        // Arrange
        var filter = new TestFilter { SortField = "name" };
        var query = new TestQuery(filter);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Filter.SortField);
    }

    [Fact]
    public void Validate_WithInvalidSortField_ShouldHaveValidationError()
    {
        // Arrange
        var filter = new TestFilter { SortField = "InvalidField" };
        var query = new TestQuery(filter);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Filter.SortField)
            .WithErrorMessage("'InvalidField' is not a valid sort field. Valid fields are: Name, Id.");
    }

    [Fact]
    public void Validate_WithNullSortField_ShouldNotHaveValidationError()
    {
        // Arrange
        var filter = new TestFilter { SortField = null };
        var query = new TestQuery(filter);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Filter.SortField);
    }

    [Fact]
    public void Validate_WithEmptySortField_ShouldNotHaveValidationError()
    {
        // Arrange
        var filter = new TestFilter { SortField = string.Empty };
        var query = new TestQuery(filter);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Filter.SortField);
    }

    #endregion

    #region SearchBy Tests

    [Fact]
    public void Validate_WithValidSearchTerm_ShouldNotHaveValidationError()
    {
        // Arrange
        var filter = new TestFilter { SearchBy = "test search" };
        var query = new TestQuery(filter);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Filter.SearchBy);
    }

    [Fact]
    public void Validate_WithSearchTermTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var filter = new TestFilter { SearchBy = new string('a', 201) };
        var query = new TestQuery(filter);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Filter.SearchBy)
            .WithErrorMessage("Search term must not exceed 200 characters.");
    }

    [Fact]
    public void Validate_WithMaxLengthSearchTerm_ShouldNotHaveValidationError()
    {
        // Arrange
        var filter = new TestFilter { SearchBy = new string('a', 200) };
        var query = new TestQuery(filter);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Filter.SearchBy);
    }

    [Fact]
    public void Validate_WithNullSearchTerm_ShouldNotHaveValidationError()
    {
        // Arrange
        var filter = new TestFilter { SearchBy = null };
        var query = new TestQuery(filter);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Filter.SearchBy);
    }

    #endregion
}
