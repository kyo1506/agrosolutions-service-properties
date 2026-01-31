namespace AgroSolutions.Properties.Shared.DTOs;

public class SensorDto
{
    public Guid Id { get; set; }
    public string CodigoIdentificacao { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string? Modelo { get; set; }
    public string? Fabricante { get; set; }
    public DateTime DataInstalacao { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public int? IntervaloLeituraMinutos { get; set; }
    public Guid TalhaoId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
