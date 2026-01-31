namespace AgroSolutions.Properties.Domain.Events;

/// <summary>
/// Evento consumido do serviço Identity quando um produtor é criado
/// </summary>
public class ProdutorCreatedEvent
{
    public Guid ProdutorId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? Endereco { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
    public string? Cep { get; set; }
    public DateTime Timestamp { get; set; }
}
