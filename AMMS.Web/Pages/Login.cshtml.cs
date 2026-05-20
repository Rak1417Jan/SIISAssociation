using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AMMS.Web.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        [Display(Prompt = "Email / Username")]
        [Required(ErrorMessage = "Email or username is required.")]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public bool RememberMe { get; set; }

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Please correct the errors and try again.";
                return Page();
            }

            // TODO: Replace with real authentication (e.g. SignInManager, custom service).
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Invalid email/username or password.";
                return Page();
            }

            // Placeholder: accept any non-empty credentials for now.
            // In production: validate against database, then sign in and set cookie.
            return RedirectToPage("/Admin/Dashboard");
        }
    }
}
