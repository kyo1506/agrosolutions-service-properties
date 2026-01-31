using AgroSolutions.Properties.Domain.Entities;
using AgroSolutions.Properties.Domain.Interfaces;
using AgroSolutions.Properties.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgroSolutions.Properties.Infrastructure.Repositories;

public class FazendaRepository(PropertiesDbContext context)
    : Repository<Fazenda>(context),
        IFazendaRepository
{
    public async Task<IEnumerable<Fazenda>> GetByProdutorIdAsync(
        Guid produtorId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet
            .Where(f => f.ProdutorId == produtorId && f.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<Fazenda?> GetWithTalhoesAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet
            .Include(f => f.Talhoes)
            .FirstOrDefaultAsync(f => f.Id == id && f.IsActive, cancellationToken);
    }
}
