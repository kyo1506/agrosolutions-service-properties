using AgroSolutions.Properties.Domain.Interfaces;
using AgroSolutions.Properties.Shared.DTOs;
using AutoMapper;
using MediatR;

namespace AgroSolutions.Properties.Application.Queries.Talhoes;

public class GetTalhoesByFazendaQueryHandler(ITalhaoRepository repository, IMapper mapper)
    : IRequestHandler<GetTalhoesByFazendaQuery, IEnumerable<TalhaoDto>>
{
    public async Task<IEnumerable<TalhaoDto>> Handle(
        GetTalhoesByFazendaQuery request,
        CancellationToken cancellationToken
    )
    {
        var talhoes = await repository.GetByFazendaIdAsync(request.FazendaId, cancellationToken);
        return mapper.Map<IEnumerable<TalhaoDto>>(talhoes);
    }
}
