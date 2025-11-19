namespace MyTodos.BuildingBlocks.Application.Helpers;

public class PagedList<T> : List<T>
{
    public PagedListMetadata Metadata { get; private set; }

    private PagedList(List<T> items, int totalCount, int pageNumber, int pageSize, string? sortField,
        string? sortDirection)
    {
        Metadata = new PagedListMetadata
        {
            TotalCount = totalCount,
            PageSize = pageSize,
            PageNumber = pageNumber,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            SortField = sortField,
            SortDirection = sortDirection
        };
        AddRange(items);
    }

    public static PagedList<T> Create(List<T> source, int totalCount, int pageNumber, int pageSize, string? sortField,
        string? sortDirection)
    {
        return new PagedList<T>(source, totalCount, pageNumber, pageSize, sortField, sortDirection);
    }
}