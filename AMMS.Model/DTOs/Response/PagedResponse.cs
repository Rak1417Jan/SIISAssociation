namespace MVEA.Model.DTOs.Response;

public sealed class PagedResponse<T>
{
    public required int Total { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required IReadOnlyList<T> Records { get; init; }
}

