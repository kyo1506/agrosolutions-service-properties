namespace AgroSolutions.Properties.Domain.Events;

/// <summary>
/// Evento consumido do serviço Identity quando um produtor é atualizado
/// </summary>
public class ProdutorUpdatedEvent
{
    public Guid ProdutorId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? Endereco { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
    public string? Cep { get; set; }
    public DateTime Timestamp { get; set; }
}
