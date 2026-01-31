namespace AgroSolutions.Properties.Domain.Events;

/// <summary>
/// Evento publicado quando um talhão é criado
/// </summary>
public class TalhaoCreatedEvent
{
    public Guid TalhaoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public Guid FazendaId { get; set; }
    public Guid ProdutorId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
