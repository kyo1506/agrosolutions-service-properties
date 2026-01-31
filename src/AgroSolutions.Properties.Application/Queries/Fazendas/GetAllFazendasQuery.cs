using AgroSolutions.Properties.Shared.DTOs;
using MediatR;

namespace AgroSolutions.Properties.Application.Queries.Fazendas;

public class GetAllFazendasQuery : IRequest<IEnumerable<FazendaDto>> { }
