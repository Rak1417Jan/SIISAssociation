namespace MVEA.Model.DTOs.Response;

public sealed class CompanyTypeResponse
{
    public int CompanyTypeId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public DateTime? CreatedDate { get; init; }
    public DateTime? ModifiedDate { get; init; }
}
