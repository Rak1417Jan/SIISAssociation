namespace MVEA.Model.DTOs.Request;

public sealed class UpdateFirmRequest
{
    public string Name { get; init; } = string.Empty;
    public int? CompanyTypeId { get; init; }
    public string? CompanyCode { get; init; }
    public string? GstNo { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? PinCode { get; init; }
    public DateTime? DateOfEstablishment { get; init; }
    public string? RegNo { get; init; }
    public string? TelephoneNo { get; init; }
    public string? Mobile { get; init; }
    public string? Website { get; init; }
    /// <summary>Comma-separated product list.</summary>
    public string? Products { get; init; }
    public bool? IsActive { get; init; }
}
