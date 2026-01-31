using AgroSolutions.Properties.Domain.Entities;
using AgroSolutions.Properties.Domain.Interfaces;
using AgroSolutions.Properties.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgroSolutions.Properties.Infrastructure.Repositories;

public class SensorRepository(PropertiesDbContext context)
    : Repository<Sensor>(context),
        ISensorRepository
{
    public async Task<IEnumerable<Sensor>> GetByTalhaoIdAsync(
        Guid talhaoId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet
            .Where(s => s.TalhaoId == talhaoId && s.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<Sensor?> GetByCodigoIdentificacaoAsync(
        string codigo,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet
            .Include(s => s.Talhao)
                .ThenInclude(t => t.Fazenda)
            .FirstOrDefaultAsync(
                s => s.CodigoIdentificacao == codigo && s.IsActive,
                cancellationToken
            );
    }
}
