using AMMS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AMMS.Web.Pages.Admin.Firms;

public class IndexModel : PageModel
{
    public IReadOnlyList<Firm> Firms { get; set; } = Array.Empty<Firm>();
    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public void OnGet(string? success = null)
    {
        Firms = FirmStore.GetAll();
        if (success == "1") SuccessMessage = "Firm deleted successfully.";
        if (success == "2") SuccessMessage = "Firm added successfully.";
        if (success == "3") SuccessMessage = "Firm updated successfully.";
    }

    public IActionResult OnPostDelete(int id)
    {
        if (FirmStore.Delete(id))
            return RedirectToPage("./Index", new { success = "1" });
        ErrorMessage = "Firm not found or could not be deleted.";
        Firms = FirmStore.GetAll();
        return Page();
    }
}
