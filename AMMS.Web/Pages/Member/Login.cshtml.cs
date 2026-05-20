using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AMMS.Web.Models;

namespace AMMS.Web.Pages.Member
{
    public class MemberLoginModel : PageModel
    {
        [BindProperty]
        [Required(ErrorMessage = "Mobile number is required.")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Mobile number must be 10 digits.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Enter a valid 10-digit mobile number.")]
        [Display(Name = "Mobile Number")]
        public string MobileNo { get; set; } = string.Empty;

        [BindProperty]
        [Display(Name = "OTP")]
        [StringLength(6, MinimumLength = 1, ErrorMessage = "Enter the OTP received.")]
        public string Otp { get; set; } = string.Empty;

        public bool OtpSent { get; set; }
        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPostSendOtp()
        {
            if (string.IsNullOrWhiteSpace(MobileNo) || MobileNo.Length != 10)
            {
                ErrorMessage = "Please enter a valid 10-digit mobile number.";
                return Page();
            }
            // Demo: any mobile gets "OTP sent"; no real SMS.
            OtpSent = true;
            return Page();
        }

        public IActionResult OnPostVerifyAndLogin()
        {
            OtpSent = true;
            if (string.IsNullOrWhiteSpace(Otp))
            {
                ErrorMessage = "Please enter OTP.";
                return Page();
            }
            // Demo: any OTP is valid. Hardcoded 9829010083 is registered; any other mobile is new.
            var member = MemberStore.GetByMobile(MobileNo);
            if (member != null)
            {
                HttpContext.Session.SetInt32("MemberId", member.Id);
                return RedirectToPage("/Member/Dashboard");
            }
            // Not registered: go to registration with mobile pre-filled
            HttpContext.Session.SetString("RegisterMobile", MobileNo);
            return RedirectToPage("/Member/Register");
        }
    }
}
