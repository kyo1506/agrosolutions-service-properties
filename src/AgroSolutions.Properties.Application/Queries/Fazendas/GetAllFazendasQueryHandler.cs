using AgroSolutions.Properties.Domain.Interfaces;
using AgroSolutions.Properties.Shared.DTOs;
using AutoMapper;
using MediatR;

namespace AgroSolutions.Properties.Application.Queries.Fazendas;

public class GetAllFazendasQueryHandler(IFazendaRepository repository, IMapper mapper)
    : IRequestHandler<GetAllFazendasQuery, IEnumerable<FazendaDto>>
{
    public async Task<IEnumerable<FazendaDto>> Handle(
        GetAllFazendasQuery request,
        CancellationToken cancellationToken
    )
    {
        var fazendas = await repository.GetAllAsync(cancellationToken);
        return mapper.Map<IEnumerable<FazendaDto>>(fazendas);
    }
}
