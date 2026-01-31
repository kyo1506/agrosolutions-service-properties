using MediatR;

namespace AgroSolutions.Properties.Application.Commands.Sensores;

public class CreateSensorCommand : IRequest<Guid>
{
    public string CodigoIdentificacao { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty; // Enum como string para flexibilidade na API
    public string? Modelo { get; set; }
    public string? Fabricante { get; set; }
    public Guid TalhaoId { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public int? IntervaloLeituraMinutos { get; set; }
}
