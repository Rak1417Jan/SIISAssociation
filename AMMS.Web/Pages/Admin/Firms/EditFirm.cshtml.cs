using System.ComponentModel.DataAnnotations;
using AMMS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AMMS.Web.Pages.Admin.Firms;

public class EditFirmModel : PageModel
{
    [BindProperty]
    public int Id { get; set; }

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
    public FirmType Type { get; set; }

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

    public string? ErrorMessage { get; set; }

    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> TypeOptions { get; } = FirmTypeHelper.All
        .Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(FirmTypeHelper.DisplayName(x.Value), ((int)x.Value).ToString()))
        .ToList();

    public IActionResult OnGet(int id)
    {
        var firm = FirmStore.GetById(id);
        if (firm == null)
            return RedirectToPage("./Index");
        Id = firm.Id;
        FirmCode = firm.FirmCode;
        FirmName = firm.FirmName;
        Type = firm.Type;
        DateOfEstablishment = firm.DateOfEstablishment;
        GstNo = firm.GstNo;
        RegNo = firm.RegNo;
        Address = firm.Address;
        OfficeAddress = firm.OfficeAddress;
        TelephoneNo = firm.TelephoneNo;
        MobileNo = firm.MobileNo;
        Email = firm.Email;
        Website = firm.Website;
        Products = firm.Products;
        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            ErrorMessage = "Please correct the errors below.";
            return Page();
        }

        var firm = FirmStore.GetById(Id);
        if (firm == null)
        {
            ErrorMessage = "Firm not found.";
            return Page();
        }

        firm.FirmCode = FirmCode;
        firm.FirmName = FirmName;
        firm.Type = Type;
        firm.DateOfEstablishment = DateOfEstablishment;
        firm.GstNo = GstNo;
        firm.RegNo = RegNo;
        firm.Address = Address;
        firm.OfficeAddress = OfficeAddress;
        firm.TelephoneNo = TelephoneNo;
        firm.MobileNo = MobileNo;
        firm.Email = Email;
        firm.Website = Website;
        firm.Products = Products;

        if (!FirmStore.Update(firm))
        {
            ErrorMessage = "Failed to update firm.";
            return Page();
        }
        return RedirectToPage("./Index", new { success = "3" });
    }
}
