namespace AgroSolutions.Properties.Shared.DTOs;

public class FazendaDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal AreaTotal { get; set; }
    public string? Localizacao { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
    public Guid ProdutorId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
