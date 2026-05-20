namespace MVEA.Model.DTOs.Response;

public class FirmListItemResponse
{
    public int FirmId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string GstNo { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public int CompanyTypeId { get; init; }
    public string CompanyTypeName { get; init; } = string.Empty;
    public string CompanyCode { get; init; } = string.Empty;
    public string RegNo { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime CreatedDate { get; init; }
}
