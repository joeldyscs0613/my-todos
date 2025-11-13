using System.Linq.Expressions;
using MyTodos.BuildingBlocks.Application.Abstractions.Filters;
using MyTodos.BuildingBlocks.Application.Abstractions.Queries;
using MyTodos.BuildingBlocks.Application.Abstractions.Specifications;
using MyTodos.BuildingBlocks.Application.Constants;
using MyTodos.BuildingBlocks.Application.Exceptions;
using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.BuildingBlocks.Application.UnitTests;

public class SpecificationTests
{
    #region Test Helper Classes

    // Mock query configuration for tests - doesn't add any includes
    private class TestEntityQueryConfiguration : IEntityQueryConfiguration<TestEntity>
    {
        public IQueryable<TestEntity> ConfigureAggregate(IQueryable<TestEntity> query)
        {
            return query; // No includes needed for test entities
        }
    }

    public class TestEntity : Entity<int>
    {
        public TestEntity(int id) : base(id)
        {
        }

        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TestFilter : Filter
    {
        public string? NameFilter { get; set; }
    }

    public class TestSpecification : Specification<TestEntity, int, TestFilter>
    {
        public TestSpecification(TestFilter filter) : base(filter)
        {
        }

        // No longer need to override ApplyIncludes - base class provides default implementation

        protected override IQueryable<TestEntity> ApplyFilter(IQueryable<TestEntity> query)
        {
            if (!string.IsNullOrWhiteSpace(Filter.NameFilter))
            {
                query = query.Where(e => e.Name.Contains(Filter.NameFilter));
            }
            return query;
        }

        protected override IQueryable<TestEntity> ApplySearchBy(IQueryable<TestEntity> query)
        {
            if (!string.IsNullOrWhiteSpace(Filter.SearchBy))
            {
                query = query.Where(e => e.Name.Contains(Filter.SearchBy));
            }
            return query;
        }

        protected override Dictionary<string, Expression<Func<TestEntity, object>>> GetSortFunctions()
        {
            return new Dictionary<string, Expression<Func<TestEntity, object>>>
            {
                { "Name", e => e.Name },
                { "Age", e => e.Age },
                { "CreatedAt", e => e.CreatedAt }
            };
        }

        // Public wrappers for testing protected methods
        public new IQueryable<TestEntity> ApplySort(IQueryable<TestEntity> query) => base.ApplySort(query);
        public new IQueryable<TestEntity> ApplyPaging(IQueryable<TestEntity> query) => base.ApplyPaging(query);
    }

    #endregion

    #region ApplySort Tests

    [Fact]
    public void ApplySort_WithValidSortField_SortsAscending()
    {
        // Arrange
        var filter = new TestFilter { SortField = "Name", SortDirection = "asc" };
        var specification = new TestSpecification(filter);
        var data = new List<TestEntity>
        {
            new(1) { Name = "Charlie" },
            new(1) { Name = "Alice" },
            new(1) { Name = "Bob" }
        }.AsQueryable();

        // Act
        var result = specification.ApplySort(data);

        // Assert
        var sortedList = result.ToList();
        Assert.Equal("Alice", sortedList[0].Name);
        Assert.Equal("Bob", sortedList[1].Name);
        Assert.Equal("Charlie", sortedList[2].Name);
    }

    [Fact]
    public void ApplySort_WithValidSortField_SortsDescending()
    {
        // Arrange
        var filter = new TestFilter { SortField = "Name", SortDirection = "desc" };
        var specification = new TestSpecification(filter);
        var data = new List<TestEntity>
        {
            new(1) { Name = "Charlie" },
            new(1) { Name = "Alice" },
            new(1) { Name = "Bob" }
        }.AsQueryable();

        // Act
        var result = specification.ApplySort(data);

        // Assert
        var sortedList = result.ToList();
        Assert.Equal("Charlie", sortedList[0].Name);
        Assert.Equal("Bob", sortedList[1].Name);
        Assert.Equal("Alice", sortedList[2].Name);
    }

    [Fact]
    public void ApplySort_WithLowercaseSortField_CapitalizesAndSorts()
    {
        // Arrange
        var filter = new TestFilter { SortField = "name", SortDirection = "asc" };
        var specification = new TestSpecification(filter);
        var data = new List<TestEntity>
        {
            new(1) { Name = "Charlie" },
            new(1) { Name = "Alice" }
        }.AsQueryable();

        // Act
        var result = specification.ApplySort(data);

        // Assert
        var sortedList = result.ToList();
        Assert.Equal("Alice", sortedList[0].Name);
        Assert.Equal("Charlie", sortedList[1].Name);
    }

    [Fact]
    public void ApplySort_WithInvalidSortField_ThrowsInvalidSortFieldException()
    {
        // Arrange
        var filter = new TestFilter { SortField = "InvalidField" };
        var specification = new TestSpecification(filter);
        var data = new List<TestEntity>
        {
            new(1) { Name = "Alice" }
        }.AsQueryable();

        // Act & Assert
        var exception = Assert.Throws<InvalidSortFieldException>(() => specification.ApplySort(data).ToList());
        Assert.Equal("InvalidField", exception.ProvidedField);
        Assert.Contains("Name", exception.ValidFields);
        Assert.Contains("Age", exception.ValidFields);
        Assert.Contains("CreatedAt", exception.ValidFields);
        Assert.Contains("InvalidField", exception.Message);
    }

    [Fact]
    public void ApplySort_WithNullSortField_SkipsSorting()
    {
        // Arrange
        var filter = new TestFilter { SortField = null };
        var specification = new TestSpecification(filter);
        var data = new List<TestEntity>
        {
            new(1) { Name = "Charlie" },
            new(1) { Name = "Alice" }
        }.AsQueryable();

        // Act
        var result = specification.ApplySort(data);

        // Assert
        var list = result.ToList();
        Assert.Equal("Charlie", list[0].Name);
        Assert.Equal("Alice", list[1].Name);
    }

    [Fact]
    public void ApplySort_WithEmptySortField_SkipsSorting()
    {
        // Arrange
        var filter = new TestFilter { SortField = "  " };
        var specification = new TestSpecification(filter);
        var data = new List<TestEntity>
        {
            new(1) { Name = "Charlie" },
            new(1) { Name = "Alice" }
        }.AsQueryable();

        // Act
        var result = specification.ApplySort(data);

        // Assert
        var list = result.ToList();
        Assert.Equal("Charlie", list[0].Name);
        Assert.Equal("Alice", list[1].Name);
    }

    [Fact]
    public void ApplySort_SortDirectionCaseInsensitive_SortsDescending()
    {
        // Arrange
        var filter = new TestFilter { SortField = "Age", SortDirection = "DESC" };
        var specification = new TestSpecification(filter);
        var data = new List<TestEntity>
        {
            new(1) { Age = 25 },
            new(1) { Age = 30 },
            new(1) { Age = 20 }
        }.AsQueryable();

        // Act
        var result = specification.ApplySort(data);

        // Assert
        var sortedList = result.ToList();
        Assert.Equal(30, sortedList[0].Age);
        Assert.Equal(25, sortedList[1].Age);
        Assert.Equal(20, sortedList[2].Age);
    }

    [Fact]
    public void ApplySort_WithNullSortDirection_SortsAscending()
    {
        // Arrange
        var filter = new TestFilter { SortField = "Age", SortDirection = null };
        var specification = new TestSpecification(filter);
        var data = new List<TestEntity>
        {
            new(1) { Age = 25 },
            new(1) { Age = 20 },
            new(1) { Age = 30 }
        }.AsQueryable();

        // Act
        var result = specification.ApplySort(data);

        // Assert
        var sortedList = result.ToList();
        Assert.Equal(20, sortedList[0].Age);
        Assert.Equal(25, sortedList[1].Age);
        Assert.Equal(30, sortedList[2].Age);
    }

    #endregion

    #region ApplyPaging Tests

    [Fact]
    public void ApplyPaging_WithNormalPageSize_ReturnsCorrectPage()
    {
        // Arrange
        var filter = new TestFilter { PageNumber = 2, PageSize = 2 };
        var specification = new TestSpecification(filter);
        var data = new List<TestEntity>
        {
            new(1) { Name = "Item1" },
            new(1) { Name = "Item2" },
            new(1) { Name = "Item3" },
            new(1) { Name = "Item4" },
            new(1) { Name = "Item5" }
        }.AsQueryable();

        // Act
        var result = specification.ApplyPaging(data);

        // Assert
        var pagedList = result.ToList();
        Assert.Equal(2, pagedList.Count);
        Assert.Equal("Item3", pagedList[0].Name);
        Assert.Equal("Item4", pagedList[1].Name);
        Assert.Equal(5, specification.TotalCount);
    }

    [Fact]
    public void ApplyPaging_PageSizeExceedsMax_CapsAtMaxPageSize()
    {
        // Arrange
        var filter = new TestFilter { PageNumber = 1, PageSize = 100 };
        var specification = new TestSpecification(filter);
        var data = Enumerable.Range(1, 60).Select(i => new TestEntity(i) { Name = $"Item{i}" }).AsQueryable();

        // Act
        var result = specification.ApplyPaging(data);

        // Assert
        var pagedList = result.ToList();
        Assert.Equal(PageListConstants.MaxPageSize, pagedList.Count);
        Assert.Equal(PageListConstants.MaxPageSize, filter.PageSize);
    }

    [Fact]
    public void ApplyPaging_PageSizeBelowMin_SetsToDefaultPageSize()
    {
        // Arrange
        var filter = new TestFilter { PageNumber = 1, PageSize = 0 };
        var specification = new TestSpecification(filter);
        var data = Enumerable.Range(1, 20).Select(i => new TestEntity(i) { Name = $"Item{i}" }).AsQueryable();

        // Act
        var result = specification.ApplyPaging(data);

        // Assert
        Assert.Equal(PageListConstants.DefaultPageSize, filter.PageSize);
    }

    [Fact]
    public void ApplyPaging_WithNegativePageSize_SetsToDefaultPageSize()
    {
        // Arrange
        var filter = new TestFilter { PageNumber = 1, PageSize = -5 };
        var specification = new TestSpecification(filter);
        var data = Enumerable.Range(1, 20).Select(i => new TestEntity(i) { Name = $"Item{i}" }).AsQueryable();

        // Act
        var result = specification.ApplyPaging(data);

        // Assert
        Assert.Equal(PageListConstants.DefaultPageSize, filter.PageSize);
    }

    [Fact]
    public void ApplyPaging_CalculatesTotalCount()
    {
        // Arrange
        var filter = new TestFilter { PageNumber = 1, PageSize = 10 };
        var specification = new TestSpecification(filter);
        var data = Enumerable.Range(1, 25).Select(i => new TestEntity(i) { Name = $"Item{i}" }).AsQueryable();

        // Act
        specification.ApplyPaging(data);

        // Assert
        Assert.Equal(25, specification.TotalCount);
    }

    [Fact]
    public void ApplyPaging_FirstPage_ReturnsCorrectItems()
    {
        // Arrange
        var filter = new TestFilter { PageNumber = 1, PageSize = 3 };
        var specification = new TestSpecification(filter);
        var data = new List<TestEntity>
        {
            new(1) { Name = "Item1" },
            new(1) { Name = "Item2" },
            new(1) { Name = "Item3" },
            new(1) { Name = "Item4" },
            new(1) { Name = "Item5" }
        }.AsQueryable();

        // Act
        var result = specification.ApplyPaging(data);

        // Assert
        var pagedList = result.ToList();
        Assert.Equal(3, pagedList.Count);
        Assert.Equal("Item1", pagedList[0].Name);
        Assert.Equal("Item2", pagedList[1].Name);
        Assert.Equal("Item3", pagedList[2].Name);
    }

    [Fact]
    public void ApplyPaging_LastPage_ReturnsRemainingItems()
    {
        // Arrange
        var filter = new TestFilter { PageNumber = 3, PageSize = 2 };
        var specification = new TestSpecification(filter);
        var data = new List<TestEntity>
        {
            new(1) { Name = "Item1" },
            new(1) { Name = "Item2" },
            new(1) { Name = "Item3" },
            new(1) { Name = "Item4" },
            new(1) { Name = "Item5" }
        }.AsQueryable();

        // Act
        var result = specification.ApplyPaging(data);

        // Assert
        var pagedList = result.ToList();
        Assert.Single(pagedList);
        Assert.Equal("Item5", pagedList[0].Name);
    }

    #endregion

    #region Apply Tests

    [Fact]
    public void Apply_WithCompleteWorkflow_ReturnsFilteredSortedPagedResults()
    {
        // Arrange
        var filter = new TestFilter
        {
            NameFilter = "test",
            SortField = "Age",
            SortDirection = "desc",
            PageNumber = 1,
            PageSize = 2
        };
        var specification = new TestSpecification(filter);
        var data = new List<TestEntity>
        {
            new(1) { Name = "test1", Age = 25 },
            new(1) { Name = "test2", Age = 30 },
            new(1) { Name = "other", Age = 35 },
            new(1) { Name = "test3", Age = 20 },
            new(1) { Name = "test4", Age = 28 }
        }.AsQueryable();

        // Act
        var result = specification.Apply(data, new TestEntityQueryConfiguration());

        // Assert
        var finalList = result.ToList();
        Assert.Equal(2, finalList.Count);
        Assert.Equal("test2", finalList[0].Name); // Age 30
        Assert.Equal("test4", finalList[1].Name); // Age 28
        Assert.Equal(4, specification.TotalCount);
    }

    [Fact]
    public void Apply_WithSearchBy_AppliesSearch()
    {
        // Arrange
        var filter = new TestFilter
        {
            SearchBy = "Alice",
            PageNumber = 1,
            PageSize = 10
        };
        var specification = new TestSpecification(filter);
        var data = new List<TestEntity>
        {
            new(1) { Name = "Alice Smith" },
            new(1) { Name = "Bob Jones" },
            new(1) { Name = "Alice Johnson" }
        }.AsQueryable();

        // Act
        var result = specification.Apply(data, new TestEntityQueryConfiguration());

        // Assert
        var finalList = result.ToList();
        Assert.Equal(2, finalList.Count);
        Assert.All(finalList, item => Assert.Contains("Alice", item.Name));
    }

    [Fact]
    public void Apply_WithInvalidSortField_ThrowsInvalidSortFieldException()
    {
        // Arrange
        var filter = new TestFilter
        {
            SortField = "InvalidField",
            PageNumber = 1,
            PageSize = 10
        };
        var specification = new TestSpecification(filter);
        var data = new List<TestEntity>
        {
            new(1) { Name = "Alice" },
            new(1) { Name = "Bob" }
        }.AsQueryable();

        // Act & Assert
        var exception = Assert.Throws<InvalidSortFieldException>(() => specification.Apply(data, new TestEntityQueryConfiguration()).ToList());
        Assert.Equal("InvalidField", exception.ProvidedField);
        Assert.Contains("InvalidField", exception.Message);
    }

    [Fact]
    public void Apply_WithNoSearchBy_SkipsSearchStep()
    {
        // Arrange
        var filter = new TestFilter
        {
            SearchBy = null,
            PageNumber = 1,
            PageSize = 10
        };
        var specification = new TestSpecification(filter);
        var data = new List<TestEntity>
        {
            new(1) { Name = "Alice" },
            new(1) { Name = "Bob" },
            new(1) { Name = "Charlie" }
        }.AsQueryable();

        // Act
        var result = specification.Apply(data, new TestEntityQueryConfiguration());

        // Assert
        var finalList = result.ToList();
        Assert.Equal(3, finalList.Count);
    }

    [Fact]
    public void Apply_WithEmptySearchBy_SkipsSearchStep()
    {
        // Arrange
        var filter = new TestFilter
        {
            SearchBy = "   ",
            PageNumber = 1,
            PageSize = 10
        };
        var specification = new TestSpecification(filter);
        var data = new List<TestEntity>
        {
            new(1) { Name = "Alice" },
            new(1) { Name = "Bob" }
        }.AsQueryable();

        // Act
        var result = specification.Apply(data, new TestEntityQueryConfiguration());

        // Assert
        var finalList = result.ToList();
        Assert.Equal(2, finalList.Count);
    }

    #endregion

    #region Filter Property Tests

    [Fact]
    public void Specification_ExposesFilterProperties()
    {
        // Arrange
        var filter = new TestFilter
        {
            PageNumber = 5,
            PageSize = 20,
            SortField = "Name",
            SortDirection = "desc"
        };
        var specification = new TestSpecification(filter);

        // Assert
        Assert.Equal(5, specification.PageNumber);
        Assert.Equal(20, specification.PageSize);
        Assert.Equal("Name", specification.SortField);
        Assert.Equal("desc", specification.SortDirection);
        Assert.Same(filter, specification.Filter);
    }

    #endregion
}
