using MVEA.Domain.Enums;

namespace MVEA.Domain.Entities;

/// <summary>
/// Chat conversation between voter and MLA team
/// </summary>
public class Chat : BaseEntity
{
    public int VoterId { get; set; }
    public int? MLAId { get; set; }
    public int? MLATeamMemberId { get; set; }
    public ChatType Type { get; set; } = ChatType.General;
    public bool IsActive { get; set; } = true;
    public DateTime? LastMessageAt { get; set; }
}
