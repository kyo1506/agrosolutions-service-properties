namespace AgroSolutions.Properties.Shared.DTOs;

public class TalhaoDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Area { get; set; }
    public string? Cultura { get; set; }
    public DateTime? DataPlantio { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Observacoes { get; set; }
    public Guid FazendaId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
