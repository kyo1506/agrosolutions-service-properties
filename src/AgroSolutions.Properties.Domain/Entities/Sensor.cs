using AgroSolutions.Properties.Domain.Enums;

namespace AgroSolutions.Properties.Domain.Entities;

/// <summary>
/// Representa um sensor IoT instalado em um talhão
/// </summary>
public class Sensor : BaseEntity
{
    public string CodigoIdentificacao { get; set; } = string.Empty; // Código único do hardware
    public TipoSensor Tipo { get; set; }
    public string? Modelo { get; set; }
    public string? Fabricante { get; set; }
    public DateTime DataInstalacao { get; set; }
    public StatusSensor Status { get; set; } = StatusSensor.Ativo;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public int? IntervaloLeituraMinutos { get; set; } = 15; // Intervalo padrão de 15 minutos

    // Chave estrangeira
    public Guid TalhaoId { get; set; }

    // Relacionamentos
    public Talhao Talhao { get; set; } = null!;
}
