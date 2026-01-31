using AgroSolutions.Properties.Domain.Entities;
using AgroSolutions.Properties.Domain.Interfaces;
using AgroSolutions.Properties.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgroSolutions.Properties.Infrastructure.Repositories;

public class TalhaoRepository(PropertiesDbContext context)
    : Repository<Talhao>(context),
        ITalhaoRepository
{
    public async Task<IEnumerable<Talhao>> GetByFazendaIdAsync(
        Guid fazendaId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet
            .Where(t => t.FazendaId == fazendaId && t.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<Talhao?> GetWithSensoresAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet
            .Include(t => t.Sensores)
            .Include(t => t.Fazenda)
            .FirstOrDefaultAsync(t => t.Id == id && t.IsActive, cancellationToken);
    }
}
