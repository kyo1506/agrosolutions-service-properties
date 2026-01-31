namespace AgroSolutions.Properties.Domain.Entities;

/// <summary>
/// Representa uma fazenda/propriedade rural
/// </summary>
public class Fazenda : BaseEntity
{
    public string Nome { get; set; } = string.Empty;
    public decimal AreaTotal { get; set; }
    public string? Localizacao { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }

    // Chave estrangeira
    public Guid ProdutorId { get; set; }

    // Relacionamentos
    public Produtor Produtor { get; set; } = null!;
    public ICollection<Talhao> Talhoes { get; set; } = new List<Talhao>();
}
