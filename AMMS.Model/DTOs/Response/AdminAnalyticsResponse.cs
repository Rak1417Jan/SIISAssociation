namespace MVEA.Model.DTOs.Response;

public sealed class AdminAnalyticsResponse
{
    public IReadOnlyList<MembershipGrowthPoint> MembershipGrowth { get; init; } = Array.Empty<MembershipGrowthPoint>();
    public IReadOnlyList<MonthlyRevenuePoint> MonthlyRevenue { get; init; } = Array.Empty<MonthlyRevenuePoint>();
    public IReadOnlyList<PlanBreakdownPoint> PlanBreakdown { get; init; } = Array.Empty<PlanBreakdownPoint>();
    public YearComparison YearComparison { get; init; } = new YearComparison();
}

public sealed class MembershipGrowthPoint
{
    public int Month { get; init; }
    public int NewMembers { get; init; }
}

public sealed class MonthlyRevenuePoint
{
    public int Month { get; init; }
    public decimal Total { get; init; }
}

public sealed class PlanBreakdownPoint
{
    public int PlanId { get; init; }
    public string PlanName { get; init; } = string.Empty;
    public int MemberCount { get; init; }
}

public sealed class YearComparison
{
    public decimal Current { get; init; }
    public decimal Previous { get; init; }
}

