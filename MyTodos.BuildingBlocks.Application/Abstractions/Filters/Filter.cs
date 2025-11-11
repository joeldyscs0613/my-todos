using MyTodos.BuildingBlocks.Application.Constants;

namespace MyTodos.BuildingBlocks.Application.Abstractions.Filters;

public abstract class Filter
{
    protected Filter()
    {
    }
    
    private string? _searchBy;
    public string? SearchBy
    {
        get => _searchBy;
        set => _searchBy = value?.Trim();
    }
    
    private string? _sortField;
    public string? SortField
    {
        get => _sortField;
        set => _sortField = value?.Trim();
    }

    private string? _sortDirection;
    public string? SortDirection
    {
        get => _sortDirection;
        set => _sortDirection = value?.Trim();
    }

    private int _pageNumber = PageListConstants.DefaultPageNumber;
    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }
    
    private int _pageSize;
    public int PageSize {
        get => _pageSize;
        set => _pageSize = value < 1 ? PageListConstants.DefaultPageSize : value;
    }
}