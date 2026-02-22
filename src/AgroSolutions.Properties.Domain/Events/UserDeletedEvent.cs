namespace AgroSolutions.Identity.Domain.Events;

/// <summary>
/// Contrato do evento publicado pelo serviço Identity quando um usuário é deletado (soft delete).
/// O namespace deve coincidir com o do serviço de origem para MassTransit deserializar corretamente.
/// </summary>
public class UserDeletedEvent
{
    public Guid UserId { get; set; }
    public DateTime Timestamp { get; set; }
}
