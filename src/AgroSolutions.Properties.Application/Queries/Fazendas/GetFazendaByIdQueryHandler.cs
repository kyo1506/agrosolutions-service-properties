using AgroSolutions.Properties.Domain.Interfaces;
using AgroSolutions.Properties.Shared.DTOs;
using AutoMapper;
using MediatR;

namespace AgroSolutions.Properties.Application.Queries.Fazendas;

public class GetFazendaByIdQueryHandler(IFazendaRepository repository, IMapper mapper)
    : IRequestHandler<GetFazendaByIdQuery, FazendaDto?>
{
    public async Task<FazendaDto?> Handle(
        GetFazendaByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var fazenda = await repository.GetByIdAsync(request.Id, cancellationToken);
        return fazenda == null ? null : mapper.Map<FazendaDto>(fazenda);
    }
}
