namespace AgroSolutions.Properties.Domain.Events;

/// <summary>
/// Evento consumido do serviço Identity quando um produtor é excluído (soft delete)
/// </summary>
public class ProdutorDeletedEvent
{
    public Guid ProdutorId { get; set; }
    public DateTime Timestamp { get; set; }
}
