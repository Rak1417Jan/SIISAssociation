using MVEA.Domain.Entities;
using MVEA.Domain.Enums;

namespace MVEA.Domain.Interfaces;

public interface ITicketRepository : IRepository<Ticket>
{
    Task<IEnumerable<Ticket>> GetByVoterIdAsync(int voterId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Ticket>> GetByStatusAsync(TicketStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Ticket>> GetByAssemblyIdAsync(int assemblyId, CancellationToken cancellationToken = default);
    Task<Ticket?> GetByTicketNumberAsync(string ticketNumber, CancellationToken cancellationToken = default);
}
