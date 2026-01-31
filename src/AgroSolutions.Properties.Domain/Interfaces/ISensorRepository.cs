using AgroSolutions.Properties.Domain.Entities;

namespace AgroSolutions.Properties.Domain.Interfaces;

public interface ISensorRepository : IRepository<Sensor>
{
    Task<IEnumerable<Sensor>> GetByTalhaoIdAsync(
        Guid talhaoId,
        CancellationToken cancellationToken = default
    );
    Task<Sensor?> GetByCodigoIdentificacaoAsync(
        string codigo,
        CancellationToken cancellationToken = default
    );
}
