using AgroSolutions.Properties.Domain.Enums;

namespace AgroSolutions.Properties.Domain.Entities;

/// <summary>
/// Representa um talhão (subdivisão da fazenda onde há plantio)
/// </summary>
public class Talhao : BaseEntity
{
    public string Nome { get; set; } = string.Empty;
    public decimal Area { get; set; } // em hectares
    public string? Cultura { get; set; } // ex: "Soja", "Milho", "Café"
    public DateTime? DataPlantio { get; set; }
    public TalhaoStatus Status { get; set; } = TalhaoStatus.Normal;
    public string? Observacoes { get; set; }

    // Chave estrangeira
    public Guid FazendaId { get; set; }

    // Relacionamentos
    public Fazenda Fazenda { get; set; } = null!;
    public ICollection<Sensor> Sensores { get; set; } = new List<Sensor>();
}
