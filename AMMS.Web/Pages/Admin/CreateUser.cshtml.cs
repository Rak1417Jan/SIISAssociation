using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AMMS.Web.Pages.Admin
{
    public class CreateUserModel : PageModel
    {
        [BindProperty]
        [Required(ErrorMessage = "Name is required.")]
        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Email / Username is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [Display(Name = "Email / Username")]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Please select a role.")]
        [Display(Name = "Role")]
        public string Role { get; set; } = string.Empty;

        [BindProperty]
        [Display(Name = "Profile Image")]
        public IFormFile? ProfileImage { get; set; }

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public static readonly List<string> RoleOptions = new()
        {
            "Super Admin",
            "Admin",
            "Verification Officer",
            "Finance / Account User",
            "Data Entry Operator"
        };

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Please correct the errors below.";
                return Page();
            }

            // TODO: Save user to database and optionally save profile image to storage.
            // if (ProfileImage != null) { ... save file ... }

            SuccessMessage = "User created successfully.";
            return Page();
        }
    }
}
