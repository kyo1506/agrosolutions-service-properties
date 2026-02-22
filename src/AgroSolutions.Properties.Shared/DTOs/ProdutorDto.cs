namespace AgroSolutions.Properties.Shared.DTOs;

public class ProdutorDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
