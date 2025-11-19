namespace MyTodos.BuildingBlocks.Application.Helpers;

public sealed class PagedListMetadata
{
    public int PageNumber { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public string? SortField { get; set; }
    public string? SortDirection { get; set; }
}