using AgroSolutions.Properties.Domain.Entities;

namespace AgroSolutions.Properties.Domain.Interfaces;

public interface IProdutorRepository : IRepository<Produtor>
{
    Task<IEnumerable<Produtor>> GetWithFazendasAsync(CancellationToken cancellationToken = default);
}
