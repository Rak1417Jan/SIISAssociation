namespace MVEA.Model.DTOs.Response;

public sealed class AdminDashboardResponse
{
    public int TotalMembers { get; init; }
    public int ActiveMembers { get; init; }
    public int InactiveMembers { get; init; }
    public int PendingApplications { get; init; }
    public int OnHoldApplications { get; init; }
    public int RejectedApplications { get; init; }
    public decimal CurrentYearRevenue { get; init; }
    public IReadOnlyList<DailyRegistrationPoint> Last7DaysRegistrations { get; init; } = Array.Empty<DailyRegistrationPoint>();
}

public sealed class DailyRegistrationPoint
{
    public DateTime Date { get; init; }
    public int Count { get; init; }
}

