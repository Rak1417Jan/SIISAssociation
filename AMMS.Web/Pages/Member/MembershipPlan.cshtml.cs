using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AMMS.Web.Models;

namespace AMMS.Web.Pages.Member
{
    public class MembershipPlanModel : PageModel
    {
        public IReadOnlyList<MemberMembership>? Memberships { get; set; }

        public IActionResult OnGet()
        {
            var memberId = HttpContext.Session.GetInt32("MemberId");
            if (memberId == null) return RedirectToPage("/Member/Login");
            var member = MemberStore.GetById(memberId.Value);
            if (member == null) { HttpContext.Session.Remove("MemberId"); return RedirectToPage("/Member/Login"); }
            Memberships = MemberStore.GetMembershipsByMemberId(member.Id);
            return Page();
        }
    }
}
