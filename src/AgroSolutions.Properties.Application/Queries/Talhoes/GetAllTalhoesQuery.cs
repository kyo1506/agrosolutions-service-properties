using AgroSolutions.Properties.Shared.DTOs;
using MediatR;

namespace AgroSolutions.Properties.Application.Queries.Talhoes;

public class GetAllTalhoesQuery : IRequest<IEnumerable<TalhaoDto>> { }
