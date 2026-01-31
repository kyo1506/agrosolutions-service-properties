namespace AgroSolutions.Properties.Domain.Events;

/// <summary>
/// Evento publicado quando um sensor é deletado (soft delete)
/// </summary>
public class SensorDeletedEvent
{
    public Guid SensorId { get; set; }
    public string CodigoIdentificacao { get; set; } = string.Empty;
    public Guid TalhaoId { get; set; }
    public Guid FazendaId { get; set; }
    public Guid ProdutorId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
