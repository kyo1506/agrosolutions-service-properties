namespace AgroSolutions.Identity.Domain.Events;

/// <summary>
/// Contrato do evento publicado pelo serviço Identity quando um usuário é atualizado.
/// O namespace deve coincidir com o do serviço de origem para MassTransit deserializar corretamente.
/// </summary>
public class UserUpdatedEvent
{
    public Guid UserId { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Role { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime Timestamp { get; set; }
}
