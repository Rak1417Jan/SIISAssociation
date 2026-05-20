using System.ComponentModel.DataAnnotations;
using AMMS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AMMS.Web.Pages.Admin.Members;

public class IndexModel : PageModel
{
    public sealed class MemberRow
    {
        public int MemberId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string MobileNo { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string FirmName { get; init; } = string.Empty;
        public string StatusDisplay { get; init; } = string.Empty;
        public string StatusCss { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
    }

    [BindProperty(SupportsGet = true)]
    public string? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    [DataType(DataType.Date)]
    public DateTime? FromDate { get; set; }

    [BindProperty(SupportsGet = true)]
    [DataType(DataType.Date)]
    public DateTime? ToDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? FirmName { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Query { get; set; }

    public List<MemberRow> Members { get; private set; } = new();

    public void OnGet()
    {
        var allMembers = MemberStore.GetAll();

        var rows = new List<MemberRow>();
        foreach (var m in allMembers)
        {
            var app = MemberStore.GetApplicationByMemberId(m.Id);
            var status = app?.Status ?? ApplicationStatus.Pending;
            if (!string.IsNullOrEmpty(Status) &&
                !string.Equals(Status, status.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (FromDate.HasValue && m.CreatedAt.Date < FromDate.Value.Date) continue;
            if (ToDate.HasValue && m.CreatedAt.Date > ToDate.Value.Date) continue;

            if (!string.IsNullOrWhiteSpace(FirmName) &&
                (m.FirmName == null || !m.FirmName.Contains(FirmName, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(Query))
            {
                var q = Query.Trim();
                var match =
                    (m.Name?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (m.MobileNo?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (m.Email?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (m.AadharCardNo?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false);
                if (!match) continue;
            }

            var (display, css) = MapStatus(status);

            rows.Add(new MemberRow
            {
                MemberId = m.Id,
                Name = m.Name,
                MobileNo = m.MobileNo,
                Email = m.Email,
                FirmName = m.FirmName ?? string.Empty,
                StatusDisplay = display,
                StatusCss = css,
                CreatedAt = m.CreatedAt
            });
        }

        Members = rows;
    }

    private static (string display, string css) MapStatus(ApplicationStatus status)
    {
        return status switch
        {
            ApplicationStatus.Approved => ("Verified", "status-approved"),
            ApplicationStatus.Pending => ("Unverified", "status-pending"),
            ApplicationStatus.Hold => ("Hold", "status-pending"),
            ApplicationStatus.Rejected => ("Rejected", "status-unapproved"),
            _ => (status.ToString(), "status-pending")
        };
    }
}

