using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AMMS.Web.Models;

namespace AMMS.Web.Pages.Member
{
    public class RegisterModel : PageModel
    {
        [BindProperty] [Required(ErrorMessage = "Name is required.")] public string Name { get; set; } = string.Empty;
        [BindProperty] [Required(ErrorMessage = "Father name is required.")] public string FatherName { get; set; } = string.Empty;
        [BindProperty]
        [Required(ErrorMessage = "Mobile number is required.")]
        [StringLength(10, MinimumLength = 10)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Enter valid 10-digit mobile.")]
        public string MobileNo { get; set; } = string.Empty;
        [BindProperty] [Required] [EmailAddress] public string Email { get; set; } = string.Empty;
        [BindProperty] public string? Designation { get; set; }
        [BindProperty] public string? Education { get; set; }
        [BindProperty] [DataType(DataType.Date)] public DateTime? DateOfBirth { get; set; }
        [BindProperty] [DataType(DataType.Date)] public DateTime? DateOfAnniversary { get; set; }
        
        [BindProperty]
        [RegularExpression(@"^(?:\d{12}|\d{4}\s?\d{4}\s?\d{4})$", ErrorMessage = "Enter valid 12-digit Aadhar number.")]
        public string? AadharCardNo { get; set; }
        [BindProperty] public IFormFile? PhotoFile { get; set; }
        [BindProperty] public IFormFile? LeaseDeedFile { get; set; }
        [BindProperty] public IFormFile? RegistrationFile { get; set; }
        [BindProperty] public IFormFile? GstCopyFile { get; set; }
        // New Firm property (static options provided in the view)
        [BindProperty] public string? Firm { get; set; }

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            var mobile = HttpContext.Session.GetString("RegisterMobile");
            if (!string.IsNullOrEmpty(mobile)) MobileNo = mobile;
            Firm = "Yash Infotech";
            return Page();
        }

        private string? SaveFile(IFormFile? file, int memberId, string prefix)
        {
            if (file == null || file.Length == 0) return null;
            var ext = Path.GetExtension(file.FileName) ?? ".bin";
            var safeName = $"{prefix}_{Guid.NewGuid():N}{ext}";
            var dir = Path.Combine("wwwroot", "uploads", "member", memberId.ToString());
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, safeName);
            using (var stream = new FileStream(path, FileMode.Create))
                file.CopyTo(stream);
            return Path.Combine("uploads", "member", memberId.ToString(), safeName);
        }

        private bool SaveMember(out AMMS.Web.Models.Member? member)
        {
            member = null;
            if (!ModelState.IsValid) return false;
            var existing = MemberStore.GetByMobile(MobileNo);
            if (existing != null)
            {
                ErrorMessage = "This mobile number is already registered. Please login.";
                return false;
            }
            member = new AMMS.Web.Models.Member
            {
                Name = Name,
                FatherName = FatherName,
                MobileNo = MobileNo,
                Email = Email,
                Designation = Designation,
                Education = Education,
                DateOfBirth = DateOfBirth,
                DateOfAnniversary = DateOfAnniversary,
                AadharCardNo = AadharCardNo,
                CreatedAt = DateTime.UtcNow
            };
            MemberStore.Add(member);
            try
            {
                member.PhotoFileName = SaveFile(PhotoFile, member.Id, "photo") ?? member.PhotoFileName;
                member.LeaseDeedFileName = SaveFile(LeaseDeedFile, member.Id, "lease") ?? member.LeaseDeedFileName;
                member.RegistrationFileName = SaveFile(RegistrationFile, member.Id, "reg") ?? member.RegistrationFileName;
                member.GstCopyFileName = SaveFile(GstCopyFile, member.Id, "gst") ?? member.GstCopyFileName;
                MemberStore.Update(member);
            }
            catch (Exception)
            {
                // ignore file errors for demo
            }
            return true;
        }

        public IActionResult OnPostSave()
        {
            if (SaveMember(out var member))
            {
                HttpContext.Session.SetInt32("MemberId", member!.Id);
                HttpContext.Session.Remove("RegisterMobile");
                SuccessMessage = "Registration saved successfully.";
                return Page();
            }
            return Page();
        }

        public IActionResult OnPostSaveAndContinue()
        {
            if (SaveMember(out var member))
            {
                HttpContext.Session.SetInt32("MemberId", member!.Id);
                HttpContext.Session.Remove("RegisterMobile");
                return RedirectToPage("/Member/PlanSelection");
            }
            return Page();
        }
    }
}
