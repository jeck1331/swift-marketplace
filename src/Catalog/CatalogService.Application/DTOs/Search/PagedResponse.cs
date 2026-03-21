namespace CatalogService.Application.DTOs.Search;

public record PagedResponse<T>(
    IReadOnlyList<T> Items,
    long TotalCount,
    int Page,
    int PageSize)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNext => Page < TotalPages;
    public bool HasPrevious => Page > 1;
}