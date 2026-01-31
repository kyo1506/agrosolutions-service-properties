using AgroSolutions.Properties.Domain.Interfaces;
using MediatR;

namespace AgroSolutions.Properties.Application.Commands.Talhoes;

public class DeleteTalhaoCommandHandler(ITalhaoRepository talhaoRepository)
    : IRequestHandler<DeleteTalhaoCommand, Unit>
{
    public async Task<Unit> Handle(DeleteTalhaoCommand request, CancellationToken cancellationToken)
    {
        await talhaoRepository.DeleteAsync(request.Id, cancellationToken);
        return Unit.Value;
    }
}
