using System.ComponentModel.DataAnnotations;

namespace MVEA.Model.DTOs.Request;

public sealed class AdminStaffPasswordResetRequest : IValidatableObject
{
    [Range(1, int.MaxValue, ErrorMessage = "Client is required.")]
    public int ClientId { get; init; }

    public string? Email { get; init; }

    public string? Username { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        bool hasEmail = !string.IsNullOrWhiteSpace(Email);
        bool hasUsername = !string.IsNullOrWhiteSpace(Username);
        if (!hasEmail && !hasUsername)
        {
            yield return new ValidationResult("Email or username is required.", new[] { nameof(Email), nameof(Username) });
        }
    }
}

public sealed class AdminStaffPasswordChangeRequest
{
    [Required]
    public string Token { get; init; } = string.Empty;

    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
    public string NewPassword { get; init; } = string.Empty;
}
