namespace MVEA.Model.DTOs.Response;

public sealed class StaffPasswordResetIssueResult
{
    public bool Issued { get; set; }

    public string? Email { get; set; }
}

public sealed class AdminPasswordAckResponse
{
    public string Message { get; init; } = string.Empty;
}
