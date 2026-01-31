namespace AgroSolutions.Properties.Domain.Entities;

/// <summary>
/// Representa um produtor rural (proprietário das fazendas)
/// </summary>
public class Produtor : BaseEntity
{
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? Endereco { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
    public string? Cep { get; set; }

    // Relacionamentos
    public ICollection<Fazenda> Fazendas { get; set; } = new List<Fazenda>();
}
