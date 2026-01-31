using AgroSolutions.Properties.Domain.Entities;

namespace AgroSolutions.Properties.Domain.Interfaces;

public interface ITalhaoRepository : IRepository<Talhao>
{
    Task<IEnumerable<Talhao>> GetByFazendaIdAsync(
        Guid fazendaId,
        CancellationToken cancellationToken = default
    );
    Task<Talhao?> GetWithSensoresAsync(Guid id, CancellationToken cancellationToken = default);
}
