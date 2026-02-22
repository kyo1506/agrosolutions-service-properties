namespace AgroSolutions.Identity.Domain.Events;

/// <summary>
/// Contrato do evento publicado pelo serviço Identity quando um usuário é criado.
/// O namespace deve coincidir com o do serviço de origem para MassTransit deserializar corretamente.
/// </summary>
public class UserCreatedEvent
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Role { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime Timestamp { get; set; }
}
