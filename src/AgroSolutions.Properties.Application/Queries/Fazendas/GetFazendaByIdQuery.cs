using AgroSolutions.Properties.Shared.DTOs;
using MediatR;

namespace AgroSolutions.Properties.Application.Queries.Fazendas;

public class GetFazendaByIdQuery : IRequest<FazendaDto?>
{
    public Guid Id { get; set; }
}
