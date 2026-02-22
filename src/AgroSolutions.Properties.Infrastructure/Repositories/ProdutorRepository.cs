using AgroSolutions.Properties.Domain.Entities;
using AgroSolutions.Properties.Domain.Interfaces;
using AgroSolutions.Properties.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgroSolutions.Properties.Infrastructure.Repositories;

public class ProdutorRepository(PropertiesDbContext context)
    : Repository<Produtor>(context),
        IProdutorRepository
{
    public async Task<IEnumerable<Produtor>> GetWithFazendasAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet
            .Include(p => p.Fazendas)
            .Where(p => p.IsActive)
            .ToListAsync(cancellationToken);
    }
}
