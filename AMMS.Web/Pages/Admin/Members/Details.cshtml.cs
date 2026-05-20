using AMMS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AMMS.Web.Pages.Admin.Members;

public class DetailsModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public AMMS.Web.Models.Member? Member { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;
    public string StatusCss { get; set; } = string.Empty;
    public string? ExistingComment { get; set; }

    [BindProperty]
    public string? AdminComment { get; set; }

    public IActionResult OnGet()
    {
        LoadMember();
        return Page();
    }

    public IActionResult OnPostVerify()
    {
        UpdateStatus(ApplicationStatus.Approved, AdminComment);
        return RedirectToPage(new { id = Id });
    }

    public IActionResult OnPostHold()
    {
        UpdateStatus(ApplicationStatus.Hold, AdminComment);
        return RedirectToPage(new { id = Id });
    }

    public IActionResult OnPostReject()
    {
        UpdateStatus(ApplicationStatus.Rejected, AdminComment);
        return RedirectToPage(new { id = Id });
    }

    private void LoadMember()
    {
        Member = MemberStore.GetById(Id);
        if (Member == null) return;

        var app = MemberStore.GetApplicationByMemberId(Member.Id);
        var status = app?.Status ?? ApplicationStatus.Pending;
        ExistingComment = app?.DiscrepancyRemarks;

        (StatusDisplay, StatusCss) = IndexModel_MapStatus(status);
    }

    private void UpdateStatus(ApplicationStatus newStatus, string? comment)
    {
        var member = MemberStore.GetById(Id);
        if (member == null) return;

        MemberStore.AddOrUpdateApplication(new MemberApplication
        {
            MemberId = member.Id,
            Status = newStatus,
            DiscrepancyRemarks = newStatus == ApplicationStatus.Rejected ? comment : null,
            ReviewedAt = DateTime.UtcNow
        });
    }

    private static (string display, string css) IndexModel_MapStatus(ApplicationStatus status)
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

