using System.ComponentModel.DataAnnotations;
using AMMS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AMMS.Web.Pages.Admin.Firms;

public class AddFirmModel : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Firm Code is required.")]
    [Display(Name = "Firm Code")]
    public string FirmCode { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Firm Name is required.")]
    [Display(Name = "Firm Name")]
    public string FirmName { get; set; } = string.Empty;

    [BindProperty]
    [Display(Name = "Type")]
    public FirmType Type { get; set; } = FirmType.Individual;

    [BindProperty]
    [Display(Name = "Date of Establishment")]
    [DataType(DataType.Date)]
    public DateTime? DateOfEstablishment { get; set; }

    [BindProperty]
    [Display(Name = "GST No.")]
    public string? GstNo { get; set; }

    [BindProperty]
    [Display(Name = "Reg No.")]
    public string? RegNo { get; set; }

    [BindProperty]
    [Display(Name = "Address")]
    public string? Address { get; set; }

    [BindProperty]
    [Display(Name = "Office Address")]
    public string? OfficeAddress { get; set; }

    [BindProperty]
    [Display(Name = "Telephone No.")]
    public string? TelephoneNo { get; set; }

    [BindProperty]
    [Display(Name = "Mobile No.")]
    public string? MobileNo { get; set; }

    [BindProperty]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [BindProperty]
    [Url(ErrorMessage = "Invalid URL.")]
    [Display(Name = "Website")]
    public string? Website { get; set; }

    [BindProperty]
    [Display(Name = "Products (comma separated)")]
    public string? Products { get; set; }

    // New file upload bindings
    [BindProperty]
    public IFormFile? LeaseDeedFile { get; set; }

    [BindProperty]
    public IFormFile? RegistrationFile { get; set; }

    [BindProperty]
    public IFormFile? GstCopyFile { get; set; }

    public string? ErrorMessage { get; set; }

    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> TypeOptions { get; } = FirmTypeHelper.All
        .Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(FirmTypeHelper.DisplayName(x.Value), ((int)x.Value).ToString()))
        .ToList();

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            ErrorMessage = "Please correct the errors below.";
            return Page();
        }

        var firm = new Firm
        {
            FirmCode = FirmCode,
            FirmName = FirmName,
            Type = Type,
            DateOfEstablishment = DateOfEstablishment,
            GstNo = GstNo,
            RegNo = RegNo,
            Address = Address,
            OfficeAddress = OfficeAddress,
            TelephoneNo = TelephoneNo,
            MobileNo = MobileNo,
            Email = Email,
            Website = Website,
            Products = Products
        };
        FirmStore.Add(firm);
        return RedirectToPage("./Index", new { success = "2" });
    }
}
