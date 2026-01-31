namespace AgroSolutions.Properties.Domain.Events;

/// <summary>
/// Evento publicado quando um sensor é criado ou atualizado
/// </summary>
public class SensorUpdatedEvent
{
    public Guid SensorId { get; set; }
    public string CodigoIdentificacao { get; set; } = string.Empty;
    public Guid TalhaoId { get; set; }
    public Guid FazendaId { get; set; }
    public Guid ProdutorId { get; set; }
    public string TipoSensor { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
