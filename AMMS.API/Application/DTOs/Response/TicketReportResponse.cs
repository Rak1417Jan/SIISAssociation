using MVEA.Domain.Enums;

namespace MVEA.Application.DTOs.Response;

public class TicketReportResponse
{
    public int TotalTickets { get; set; }
    public int NewTickets { get; set; }
    public int InProgressTickets { get; set; }
    public int ResolvedTickets { get; set; }
    public int ClosedTickets { get; set; }
    public double AverageResolutionTimeHours { get; set; }
    public double ResolutionRate { get; set; } // Percentage
    public List<TicketCategoryStats> CategoryStats { get; set; } = new();
    public List<TicketBoothStats> BoothStats { get; set; } = new();
    public DateTime? ReportStartDate { get; set; }
    public DateTime? ReportEndDate { get; set; }
}

public class TicketCategoryStats
{
    public TicketCategory Category { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int Count { get; set; }
    public double AverageResolutionTimeHours { get; set; }
}

public class TicketBoothStats
{
    public int BoothId { get; set; }
    public string BoothNumber { get; set; } = string.Empty;
    public int TicketCount { get; set; }
    public int ResolvedCount { get; set; }
    public double ResolutionRate { get; set; }
}
