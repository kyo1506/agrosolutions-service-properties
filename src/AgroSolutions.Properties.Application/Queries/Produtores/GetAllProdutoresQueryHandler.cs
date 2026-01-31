using AgroSolutions.Properties.Domain.Interfaces;
using AgroSolutions.Properties.Shared.DTOs;
using AutoMapper;
using MediatR;

namespace AgroSolutions.Properties.Application.Queries.Produtores;

public class GetAllProdutoresQueryHandler(IProdutorRepository repository, IMapper mapper)
    : IRequestHandler<GetAllProdutoresQuery, IEnumerable<ProdutorDto>>
{
    public async Task<IEnumerable<ProdutorDto>> Handle(
        GetAllProdutoresQuery request,
        CancellationToken cancellationToken
    )
    {
        var produtores = await repository.GetAllAsync(cancellationToken);
        return mapper.Map<IEnumerable<ProdutorDto>>(produtores);
    }
}
