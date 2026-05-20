using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AMMS.Web.Models;

namespace AMMS.Web.Pages.Member
{
    public class MyInfoModel : PageModel
    {
        public AMMS.Web.Models.Member? Member { get; set; }

        public IActionResult OnGet()
        {
            var memberId = HttpContext.Session.GetInt32("MemberId");
            if (memberId == null) return RedirectToPage("/Member/Login");
            Member = MemberStore.GetById(memberId.Value);
            if (Member == null) { HttpContext.Session.Remove("MemberId"); return RedirectToPage("/Member/Login"); }
            return Page();
        }
    }
}
