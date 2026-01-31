namespace AgroSolutions.Properties.Domain.Entities;

/// <summary>
/// Entidade Outbox para implementar o padrão Transactional Outbox
/// Garante exatamente-uma-vez na entrega de eventos
/// </summary>
public class OutboxMessage : BaseEntity
{
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime? ProcessedAt { get; set; }
    public int RetryCount { get; set; } = 0;
    public string? ErrorMessage { get; set; }
}
