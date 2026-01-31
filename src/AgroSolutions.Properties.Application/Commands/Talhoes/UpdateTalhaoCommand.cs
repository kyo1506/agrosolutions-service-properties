using MediatR;

namespace AgroSolutions.Properties.Application.Commands.Talhoes;

public class UpdateTalhaoCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Area { get; set; }
    public string? Cultura { get; set; }
    public DateTime? DataPlantio { get; set; }
    public string? Observacoes { get; set; }
}
