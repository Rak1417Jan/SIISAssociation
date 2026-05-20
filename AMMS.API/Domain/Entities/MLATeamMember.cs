namespace MVEA.Domain.Entities;

/// <summary>
/// MLA team member entity
/// </summary>
public class MLATeamMember : BaseEntity
{
    public int UserId { get; set; }
    public int MLAId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public bool CanPostContent { get; set; }
    public bool CanHandleChats { get; set; }
    public bool CanHandleTickets { get; set; }
}
