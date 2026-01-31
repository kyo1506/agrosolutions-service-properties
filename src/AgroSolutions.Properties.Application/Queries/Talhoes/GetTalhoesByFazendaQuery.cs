using AgroSolutions.Properties.Shared.DTOs;
using MediatR;

namespace AgroSolutions.Properties.Application.Queries.Talhoes;

public class GetTalhoesByFazendaQuery : IRequest<IEnumerable<TalhaoDto>>
{
    public Guid FazendaId { get; set; }
}
