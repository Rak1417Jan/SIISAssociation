using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMMS.Model.DTOs.Request
{
    public class RegisterRequest
    {
        // -----------------------------
        // PERSONAL DETAILS
        // -----------------------------

        [Required]
        [StringLength(150)]
        public string OwnerName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; }

        [Required]
        [Phone]
        [StringLength(20)]
        public string MobileNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public DateTime? AnniversaryDate { get; set; }

        // -----------------------------
        // COMPANY DETAILS
        // -----------------------------

        // If joining existing company
        public int? CompanyId { get; set; }

        // If creating new company
        [StringLength(200)]
        public string? CompanyName { get; set; }

        [StringLength(50)]
        public string? GstNumber { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(20)]
        public string? PinCode { get; set; }

        // -----------------------------
        // MEMBERSHIP
        // -----------------------------

        [Required]
        public int PlanId { get; set; }

        // -----------------------------
        // SECURITY
        // -----------------------------

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Otp { get; set; }
    }
}
