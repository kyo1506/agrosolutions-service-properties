using AgroSolutions.Properties.Domain.Interfaces;
using MediatR;

namespace AgroSolutions.Properties.Application.Commands.Talhoes;

public class UpdateTalhaoCommandHandler(ITalhaoRepository talhaoRepository)
    : IRequestHandler<UpdateTalhaoCommand, Unit>
{
    public async Task<Unit> Handle(UpdateTalhaoCommand request, CancellationToken cancellationToken)
    {
        var talhao =
            await talhaoRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Talhão {request.Id} não encontrado");

        talhao.Nome = request.Nome;
        talhao.Area = request.Area;
        talhao.Cultura = request.Cultura;
        talhao.DataPlantio = request.DataPlantio.HasValue
            ? DateTime.SpecifyKind(request.DataPlantio.Value, DateTimeKind.Utc)
            : null;
        talhao.Observacoes = request.Observacoes;
        talhao.UpdatedAt = DateTime.UtcNow;

        await talhaoRepository.UpdateAsync(talhao, cancellationToken);

        return Unit.Value;
    }
}
