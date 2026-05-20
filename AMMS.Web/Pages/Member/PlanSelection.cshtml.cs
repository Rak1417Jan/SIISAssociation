using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AMMS.Web.Models;

namespace AMMS.Web.Pages.Member
{
    public class PlanSelectionModel : PageModel
    {
        [BindProperty]
        public int SelectedPlan { get; set; }

        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetInt32("MemberId") == null)
                return RedirectToPage("/Member/Login");
            return Page();
        }

        public IActionResult OnPost()
        {
            if (HttpContext.Session.GetInt32("MemberId") == null)
                return RedirectToPage("/Member/Login");
            if (SelectedPlan != (int)MembershipPlanType.Yearly && SelectedPlan != (int)MembershipPlanType.Lifetime)
            {
                ErrorMessage = "Please select a plan.";
                return Page();
            }
            HttpContext.Session.SetInt32("SelectedPlanType", SelectedPlan);
            return RedirectToPage("/Member/Payment");
        }
    }
}
