using AgroSolutions.Properties.Domain.Interfaces;
using AgroSolutions.Properties.Shared.DTOs;
using AutoMapper;
using MediatR;

namespace AgroSolutions.Properties.Application.Queries.Talhoes;

public class GetTalhaoByIdQueryHandler(ITalhaoRepository repository, IMapper mapper)
    : IRequestHandler<GetTalhaoByIdQuery, TalhaoDto?>
{
    public async Task<TalhaoDto?> Handle(
        GetTalhaoByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var talhao = await repository.GetByIdAsync(request.Id, cancellationToken);
        return talhao == null ? null : mapper.Map<TalhaoDto>(talhao);
    }
}
