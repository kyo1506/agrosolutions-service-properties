namespace AgroSolutions.Properties.Domain.Enums;

/// <summary>
/// Tipo de operação publicada no evento de sensor — alinhada ao contrato do Worker/Ingestion
/// </summary>
public enum TypeOperation
{
    Create = 1,
    Update = 2,
    Delete = 3,
}
