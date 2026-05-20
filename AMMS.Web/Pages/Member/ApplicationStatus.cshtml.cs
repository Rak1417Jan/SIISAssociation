using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AMMS.Web.Models;

namespace AMMS.Web.Pages.Member
{
    public class ApplicationStatusModel : PageModel
    {
        public MemberApplication? Application { get; set; }

        public IActionResult OnGet()
        {
            var memberId = HttpContext.Session.GetInt32("MemberId");
            if (memberId == null) return RedirectToPage("/Member/Login");
            var member = MemberStore.GetById(memberId.Value);
            if (member == null) { HttpContext.Session.Remove("MemberId"); return RedirectToPage("/Member/Login"); }
            Application = MemberStore.GetApplicationByMemberId(member.Id);
            return Page();
        }
    }
}
