using AgroSolutions.Properties.Shared.DTOs;
using MediatR;

namespace AgroSolutions.Properties.Application.Queries.Produtores;

public class GetAllProdutoresQuery : IRequest<IEnumerable<ProdutorDto>> { }
