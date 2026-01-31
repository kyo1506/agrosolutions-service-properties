namespace AgroSolutions.Properties.Domain.Entities;

/// <summary>
/// Entidade base com propriedades comuns
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}
