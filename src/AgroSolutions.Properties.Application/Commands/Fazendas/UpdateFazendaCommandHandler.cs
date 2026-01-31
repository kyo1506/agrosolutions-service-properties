using AgroSolutions.Properties.Domain.Interfaces;
using MediatR;

namespace AgroSolutions.Properties.Application.Commands.Fazendas;

public class UpdateFazendaCommandHandler(IFazendaRepository fazendaRepository)
    : IRequestHandler<UpdateFazendaCommand, Unit>
{
    public async Task<Unit> Handle(
        UpdateFazendaCommand request,
        CancellationToken cancellationToken
    )
    {
        var fazenda =
            await fazendaRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Fazenda {request.Id} não encontrada");

        fazenda.Nome = request.Nome;
        fazenda.AreaTotal = request.AreaTotal;
        fazenda.Localizacao = request.Localizacao;
        fazenda.Latitude = request.Latitude;
        fazenda.Longitude = request.Longitude;
        fazenda.Cidade = request.Cidade;
        fazenda.Estado = request.Estado;
        fazenda.UpdatedAt = DateTime.UtcNow;

        await fazendaRepository.UpdateAsync(fazenda, cancellationToken);

        return Unit.Value;
    }
}
