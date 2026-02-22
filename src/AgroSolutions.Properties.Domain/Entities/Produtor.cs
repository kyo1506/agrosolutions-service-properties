namespace AgroSolutions.Properties.Domain.Entities;

/// <summary>
/// Representa um produtor rural (proprietário das fazendas).
/// Sincronizado automaticamente a partir do serviço Identity via eventos.
/// </summary>
public class Produtor : BaseEntity
{
    public string Nome { get; set; } = string.Empty;
    public string? Email { get; set; }

    // Relacionamentos
    public ICollection<Fazenda> Fazendas { get; set; } = new List<Fazenda>();
}
