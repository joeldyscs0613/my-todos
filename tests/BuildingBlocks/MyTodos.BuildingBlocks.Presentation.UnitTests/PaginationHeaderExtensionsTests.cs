using Microsoft.AspNetCore.Http;
using MyTodos.BuildingBlocks.Application.Helpers;
using MyTodos.BuildingBlocks.Presentation.Constants;
using MyTodos.BuildingBlocks.Presentation.Extensions;

namespace MyTodos.BuildingBlocks.Presentation.UnitTests;

public class PaginationHeaderExtensionsTests
{
    private readonly DefaultHttpContext _httpContext;

    public PaginationHeaderExtensionsTests()
    {
        _httpContext = new DefaultHttpContext();
    }

    #region AddPaginationHeaders(PagedListMetadata) Tests

    [Fact]
    public void AddPaginationHeaders_WithMetadata_AddsAllRequiredHeaders()
    {
        // Arrange
        var metadata = new PagedListMetadata
        {
            PageNumber = 2,
            PageSize = 10,
            TotalCount = 43,
            TotalPages = 5,
            SortField = "Name",
            SortDirection = "ASC"
        };

        // Act
        _httpContext.Response.AddPaginationHeaders(metadata);

        // Assert
        Assert.Equal("2", _httpContext.Response.Headers[HeaderConstants.Pagination.CurrentPage]);
        Assert.Equal("10", _httpContext.Response.Headers[HeaderConstants.Pagination.PageSize]);
        Assert.Equal("5", _httpContext.Response.Headers[HeaderConstants.Pagination.TotalPages]);
        Assert.Equal("43", _httpContext.Response.Headers[HeaderConstants.Pagination.TotalCount]);
        Assert.Equal("true", _httpContext.Response.Headers[HeaderConstants.Pagination.HasPrevious]);
        Assert.Equal("true", _httpContext.Response.Headers[HeaderConstants.Pagination.HasNext]);
        Assert.Equal("Name", _httpContext.Response.Headers[HeaderConstants.Pagination.SortField]);
        Assert.Equal("asc", _httpContext.Response.Headers[HeaderConstants.Pagination.SortDirection]);
    }

    [Fact]
    public void AddPaginationHeaders_FirstPage_HasPreviousIsFalse()
    {
        // Arrange
        var metadata = new PagedListMetadata
        {
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 43,
            TotalPages = 5
        };

        // Act
        _httpContext.Response.AddPaginationHeaders(metadata);

        // Assert
        Assert.Equal("false", _httpContext.Response.Headers[HeaderConstants.Pagination.HasPrevious]);
        Assert.Equal("true", _httpContext.Response.Headers[HeaderConstants.Pagination.HasNext]);
    }

    [Fact]
    public void AddPaginationHeaders_LastPage_HasNextIsFalse()
    {
        // Arrange
        var metadata = new PagedListMetadata
        {
            PageNumber = 5,
            PageSize = 10,
            TotalCount = 43,
            TotalPages = 5
        };

        // Act
        _httpContext.Response.AddPaginationHeaders(metadata);

        // Assert
        Assert.Equal("true", _httpContext.Response.Headers[HeaderConstants.Pagination.HasPrevious]);
        Assert.Equal("false", _httpContext.Response.Headers[HeaderConstants.Pagination.HasNext]);
    }

    [Fact]
    public void AddPaginationHeaders_SinglePage_BothNavigationFlagsAreFalse()
    {
        // Arrange
        var metadata = new PagedListMetadata
        {
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 5,
            TotalPages = 1
        };

        // Act
        _httpContext.Response.AddPaginationHeaders(metadata);

        // Assert
        Assert.Equal("false", _httpContext.Response.Headers[HeaderConstants.Pagination.HasPrevious]);
        Assert.Equal("false", _httpContext.Response.Headers[HeaderConstants.Pagination.HasNext]);
    }

    [Fact]
    public void AddPaginationHeaders_WithoutSortFields_DoesNotAddSortHeaders()
    {
        // Arrange
        var metadata = new PagedListMetadata
        {
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 25,
            TotalPages = 3
        };

        // Act
        _httpContext.Response.AddPaginationHeaders(metadata);

        // Assert
        Assert.False(_httpContext.Response.Headers.ContainsKey(HeaderConstants.Pagination.SortField));
        Assert.False(_httpContext.Response.Headers.ContainsKey(HeaderConstants.Pagination.SortDirection));
    }

    [Fact]
    public void AddPaginationHeaders_WithSortFieldOnly_AddsSortFieldHeader()
    {
        // Arrange
        var metadata = new PagedListMetadata
        {
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 25,
            TotalPages = 3,
            SortField = "CreatedAt"
        };

        // Act
        _httpContext.Response.AddPaginationHeaders(metadata);

        // Assert
        Assert.Equal("CreatedAt", _httpContext.Response.Headers[HeaderConstants.Pagination.SortField]);
        Assert.False(_httpContext.Response.Headers.ContainsKey(HeaderConstants.Pagination.SortDirection));
    }

    [Fact]
    public void AddPaginationHeaders_WithSortDirection_NormalizesToLowerCase()
    {
        // Arrange
        var metadata = new PagedListMetadata
        {
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 25,
            TotalPages = 3,
            SortDirection = "DESC"
        };

        // Act
        _httpContext.Response.AddPaginationHeaders(metadata);

        // Assert
        Assert.Equal("desc", _httpContext.Response.Headers[HeaderConstants.Pagination.SortDirection]);
    }

    [Fact]
    public void AddPaginationHeaders_NullResponse_ThrowsArgumentNullException()
    {
        // Arrange
        HttpResponse? nullResponse = null;
        var metadata = new PagedListMetadata
        {
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 25,
            TotalPages = 3
        };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            nullResponse!.AddPaginationHeaders(metadata));
    }

    [Fact]
    public void AddPaginationHeaders_NullMetadata_ThrowsArgumentNullException()
    {
        // Arrange
        PagedListMetadata? nullMetadata = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            _httpContext.Response.AddPaginationHeaders(nullMetadata!));
    }

    [Fact]
    public void AddPaginationHeaders_EmptyResult_HandlesZeroCount()
    {
        // Arrange
        var metadata = new PagedListMetadata
        {
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 0,
            TotalPages = 0
        };

        // Act
        _httpContext.Response.AddPaginationHeaders(metadata);

        // Assert
        Assert.Equal("1", _httpContext.Response.Headers[HeaderConstants.Pagination.CurrentPage]);
        Assert.Equal("0", _httpContext.Response.Headers[HeaderConstants.Pagination.TotalCount]);
        Assert.Equal("0", _httpContext.Response.Headers[HeaderConstants.Pagination.TotalPages]);
        Assert.Equal("false", _httpContext.Response.Headers[HeaderConstants.Pagination.HasPrevious]);
        Assert.Equal("false", _httpContext.Response.Headers[HeaderConstants.Pagination.HasNext]);
    }

    #endregion

    #region AddPaginationHeaders(PagedList<T>) Tests

    [Fact]
    public void AddPaginationHeaders_WithPagedList_AddsHeadersFromMetadata()
    {
        // Arrange
        var items = new List<string> { "Item1", "Item2", "Item3" };
        var pagedList = PagedList<string>.Create(
            items,
            totalCount: 10,
            pageNumber: 2,
            pageSize: 3,
            sortField: null,
            sortDirection: null);

        // Act
        _httpContext.Response.AddPaginationHeaders(pagedList);

        // Assert
        Assert.Equal("2", _httpContext.Response.Headers[HeaderConstants.Pagination.CurrentPage]);
        Assert.Equal("3", _httpContext.Response.Headers[HeaderConstants.Pagination.PageSize]);
        Assert.Equal("4", _httpContext.Response.Headers[HeaderConstants.Pagination.TotalPages]);
        Assert.Equal("10", _httpContext.Response.Headers[HeaderConstants.Pagination.TotalCount]);
        Assert.Equal("true", _httpContext.Response.Headers[HeaderConstants.Pagination.HasPrevious]);
        Assert.Equal("true", _httpContext.Response.Headers[HeaderConstants.Pagination.HasNext]);
    }

    [Fact]
    public void AddPaginationHeaders_NullPagedList_ThrowsArgumentNullException()
    {
        // Arrange
        PagedList<string>? nullPagedList = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            _httpContext.Response.AddPaginationHeaders(nullPagedList!));
    }

    [Fact]
    public void AddPaginationHeaders_EmptyPagedList_HandlesZeroItems()
    {
        // Arrange
        var items = new List<string>();
        var pagedList = PagedList<string>.Create(
            items,
            totalCount: 0,
            pageNumber: 1,
            pageSize: 10,
            sortField: null,
            sortDirection: null);

        // Act
        _httpContext.Response.AddPaginationHeaders(pagedList);

        // Assert
        Assert.Equal("1", _httpContext.Response.Headers[HeaderConstants.Pagination.CurrentPage]);
        Assert.Equal("0", _httpContext.Response.Headers[HeaderConstants.Pagination.TotalCount]);
        Assert.Equal("0", _httpContext.Response.Headers[HeaderConstants.Pagination.TotalPages]);
    }

    #endregion
}
