using AgroSolutions.Properties.Domain.Interfaces;
using AgroSolutions.Properties.Shared.DTOs;
using AutoMapper;
using MediatR;

namespace AgroSolutions.Properties.Application.Queries.Talhoes;

public class GetAllTalhoesQueryHandler(ITalhaoRepository repository, IMapper mapper)
    : IRequestHandler<GetAllTalhoesQuery, IEnumerable<TalhaoDto>>
{
    public async Task<IEnumerable<TalhaoDto>> Handle(
        GetAllTalhoesQuery request,
        CancellationToken cancellationToken
    )
    {
        var talhoes = await repository.GetAllAsync(cancellationToken);
        return mapper.Map<IEnumerable<TalhaoDto>>(talhoes);
    }
}
