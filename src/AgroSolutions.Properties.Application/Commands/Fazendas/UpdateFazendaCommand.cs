using MediatR;

namespace AgroSolutions.Properties.Application.Commands.Fazendas;

public class UpdateFazendaCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal AreaTotal { get; set; }
    public string? Localizacao { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
}
