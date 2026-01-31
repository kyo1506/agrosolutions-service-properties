using AgroSolutions.Properties.Domain.Entities;

namespace AgroSolutions.Properties.Domain.Interfaces;

public interface IFazendaRepository : IRepository<Fazenda>
{
    Task<IEnumerable<Fazenda>> GetByProdutorIdAsync(
        Guid produtorId,
        CancellationToken cancellationToken = default
    );
    Task<Fazenda?> GetWithTalhoesAsync(Guid id, CancellationToken cancellationToken = default);
}
