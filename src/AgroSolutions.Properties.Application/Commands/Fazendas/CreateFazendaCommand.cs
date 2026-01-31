using MediatR;

namespace AgroSolutions.Properties.Application.Commands.Fazendas;

public class CreateFazendaCommand : IRequest<Guid>
{
    public string Nome { get; set; } = string.Empty;
    public decimal AreaTotal { get; set; }
    public string? Localizacao { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
    public Guid ProdutorId { get; set; }

    // Permite cadastrar talhões junto com a fazenda
    public List<CreateTalhaoDto>? Talhoes { get; set; }
}

public class CreateTalhaoDto
{
    public string Nome { get; set; } = string.Empty;
    public decimal Area { get; set; }
    public string? Cultura { get; set; }
    public DateTime? DataPlantio { get; set; }
    public string? Observacoes { get; set; }

    // Permite cadastrar sensores junto com o talhão
    public List<CreateSensorDto>? Sensores { get; set; }
}

public class CreateSensorDto
{
    public string CodigoIdentificacao { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string? Modelo { get; set; }
    public string? Fabricante { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public int? IntervaloLeituraMinutos { get; set; }
}
