using MediatR;

namespace AgroSolutions.Properties.Application.Commands.Sensores;

public class UpdateSensorCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
    public string? Modelo { get; set; }
    public string? Fabricante { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public int? IntervaloLeituraMinutos { get; set; }
    public string? Status { get; set; }
}
