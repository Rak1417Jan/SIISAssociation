using MVEA.Domain.Entities;

namespace MVEA.Domain.Interfaces;

public interface ITicketAttachmentRepository : IRepository<TicketAttachment>
{
    Task<IEnumerable<TicketAttachment>> GetByTicketIdAsync(int ticketId, CancellationToken cancellationToken = default);
}
