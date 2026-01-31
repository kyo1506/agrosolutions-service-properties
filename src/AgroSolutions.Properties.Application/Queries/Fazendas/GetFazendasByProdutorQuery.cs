using AgroSolutions.Properties.Shared.DTOs;
using MediatR;

namespace AgroSolutions.Properties.Application.Queries.Fazendas;

public class GetFazendasByProdutorQuery : IRequest<IEnumerable<FazendaDto>>
{
    public Guid ProdutorId { get; set; }
}
