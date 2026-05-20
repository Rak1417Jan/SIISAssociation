using MVEA.Model.Enums;

namespace MVEA.Model.DTOs.Response;

public class ChatConversationResponse
{
    public int Id { get; set; }
    public int VoterId { get; set; }
    public string VoterName { get; set; } = string.Empty;
    public string? VoterMobile { get; set; }
    public int? MLAId { get; set; }
    public string? MLAName { get; set; }
    public int? MLATeamMemberId { get; set; }
    public string? MLATeamMemberName { get; set; }
    public ChatType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public string? LastMessage { get; set; }
    public bool HasUnreadMessages { get; set; }
    public int UnreadCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
