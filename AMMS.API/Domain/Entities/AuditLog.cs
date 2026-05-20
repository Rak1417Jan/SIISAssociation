namespace MVEA.Domain.Entities;

/// <summary>
/// Audit log entity for tracking all system activities
/// </summary>
public class AuditLog : BaseEntity
{
    public string EntityType { get; set; } = string.Empty; // User, MLA, Ticket, etc.
    public int EntityId { get; set; }
    public string Action { get; set; } = string.Empty; // Create, Update, Delete, Approve, Reject
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public string? OldValues { get; set; } // JSON string of old values
    public string? NewValues { get; set; } // JSON string of new values
    public string? Description { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
