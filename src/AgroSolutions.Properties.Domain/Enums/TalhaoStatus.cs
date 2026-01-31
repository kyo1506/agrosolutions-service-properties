namespace AgroSolutions.Properties.Domain.Enums;

/// <summary>
/// Status do talhão (atualizado pelo worker de alertas)
/// </summary>
public enum TalhaoStatus
{
    Normal = 0,
    Atencao = 1,
    Critico = 2,
    EmManutencao = 3,
}
