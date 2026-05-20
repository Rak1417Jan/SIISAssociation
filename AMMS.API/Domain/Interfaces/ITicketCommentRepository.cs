using MVEA.Domain.Entities;

namespace MVEA.Domain.Interfaces;

public interface ITicketCommentRepository : IRepository<TicketComment>
{
    Task<IEnumerable<TicketComment>> GetByTicketIdAsync(int ticketId, bool includeInternal = false, CancellationToken cancellationToken = default);
}
