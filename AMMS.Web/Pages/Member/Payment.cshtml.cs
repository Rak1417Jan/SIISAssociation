using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AMMS.Web.Models;

namespace AMMS.Web.Pages.Member
{
    public class PaymentModel : PageModel
    {
        public AMMS.Web.Models.Member? Member { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public decimal PlanAmount { get; set; }
        public decimal PlatformCharges => MembershipPlanHelper.PlatformCharges;
        public decimal SubTotal { get; set; }
        public decimal GstAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal GstRate => MembershipPlanHelper.GstRatePercent;

        public IActionResult OnGet()
        {
            var memberId = HttpContext.Session.GetInt32("MemberId");
            var planType = HttpContext.Session.GetInt32("SelectedPlanType");
            if (memberId == null) return RedirectToPage("/Member/Login");
            if (planType == null) return RedirectToPage("/Member/PlanSelection");
            Member = MemberStore.GetById(memberId.Value);
            if (Member == null) { HttpContext.Session.Remove("MemberId"); return RedirectToPage("/Member/Login"); }
            var plan = (MembershipPlanType)planType.Value;
            PlanName = MembershipPlanHelper.DisplayName(plan);
            PlanAmount = MembershipPlanHelper.BaseAmount(plan);
            SubTotal = PlanAmount + PlatformCharges;
            GstAmount = Math.Round(SubTotal * (GstRate / 100m), 2);
            TotalAmount = SubTotal + GstAmount;
            return Page();
        }

        public IActionResult OnPost()
        {
            var memberId = HttpContext.Session.GetInt32("MemberId");
            var planType = HttpContext.Session.GetInt32("SelectedPlanType");
            if (memberId == null || planType == null) return RedirectToPage("/Member/Login");
            Member = MemberStore.GetById(memberId.Value);
            if (Member == null) return RedirectToPage("/Member/Login");
            var plan = (MembershipPlanType)planType.Value;
            PlanAmount = MembershipPlanHelper.BaseAmount(plan);
            SubTotal = PlanAmount + PlatformCharges;
            GstAmount = Math.Round(SubTotal * (GstRate / 100m), 2);
            TotalAmount = SubTotal + GstAmount;

            var membership = new MemberMembership
            {
                MemberId = Member.Id,
                PlanType = plan,
                BaseAmount = PlanAmount,
                StartDate = DateTime.UtcNow,
                EndDate = plan == MembershipPlanType.Yearly ? DateTime.UtcNow.AddYears(1) : null,
                IsActive = true
            };
            MemberStore.AddMembership(membership);

            var payment = new MemberPayment
            {
                MemberId = Member.Id,
                MembershipId = membership.Id,
                PlanAmount = PlanAmount,
                PlatformCharges = PlatformCharges,
                SubTotal = SubTotal,
                GstAmount = GstAmount,
                TotalAmount = TotalAmount,
                PaymentDate = DateTime.UtcNow,
                TransactionRef = "TXN-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
                Status = "Completed"
            };
            MemberStore.AddPayment(payment);

            var app = MemberStore.GetApplicationByMemberId(Member.Id);
            if (app == null)
            {
                MemberStore.AddOrUpdateApplication(new MemberApplication
                {
                    MemberId = Member.Id,
                    Status = ApplicationStatus.Pending,
                    ReviewedAt = null
                });
            }

            HttpContext.Session.Remove("SelectedPlanType");
            return RedirectToPage("/Member/Dashboard");
        }
    }
}
