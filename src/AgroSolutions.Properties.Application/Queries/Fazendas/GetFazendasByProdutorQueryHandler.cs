using AgroSolutions.Properties.Domain.Interfaces;
using AgroSolutions.Properties.Shared.DTOs;
using AutoMapper;
using MediatR;

namespace AgroSolutions.Properties.Application.Queries.Fazendas;

public class GetFazendasByProdutorQueryHandler(IFazendaRepository repository, IMapper mapper)
    : IRequestHandler<GetFazendasByProdutorQuery, IEnumerable<FazendaDto>>
{
    public async Task<IEnumerable<FazendaDto>> Handle(
        GetFazendasByProdutorQuery request,
        CancellationToken cancellationToken
    )
    {
        var fazendas = await repository.GetByProdutorIdAsync(request.ProdutorId, cancellationToken);
        return mapper.Map<IEnumerable<FazendaDto>>(fazendas);
    }
}
