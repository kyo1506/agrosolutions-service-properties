namespace AgroSolutions.Properties.Domain.Events;

/// <summary>
/// Evento consumido do worker de alertas para atualizar status do talhão
/// </summary>
public class StatusChangedEvent
{
    public Guid TalhaoId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Motivo { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
