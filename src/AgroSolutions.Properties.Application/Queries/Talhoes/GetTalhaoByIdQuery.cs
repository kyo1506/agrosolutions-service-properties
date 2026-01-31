using AgroSolutions.Properties.Shared.DTOs;
using MediatR;

namespace AgroSolutions.Properties.Application.Queries.Talhoes;

public class GetTalhaoByIdQuery : IRequest<TalhaoDto?>
{
    public Guid Id { get; set; }
}
