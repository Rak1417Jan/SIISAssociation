namespace MVEA.Model.DTOs.Response;

public sealed class FirmDetailResponse
{
    public int FirmId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string GstNo { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string PinCode { get; init; } = string.Empty;
    public int CompanyTypeId { get; init; }
    public string CompanyTypeName { get; init; } = string.Empty;
    public string CompanyTypeCode { get; init; } = string.Empty;
    public string CompanyCode { get; init; } = string.Empty;
    public DateTime? DateOfEstablishment { get; init; }
    public string RegNo { get; init; } = string.Empty;
    public string TelephoneNo { get; init; } = string.Empty;
    public string Mobile { get; init; } = string.Empty;
    public string Website { get; init; } = string.Empty;
    public string Products { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime CreatedDate { get; init; }
}
