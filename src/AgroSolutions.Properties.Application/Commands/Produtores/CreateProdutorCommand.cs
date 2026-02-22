using MediatR;

namespace AgroSolutions.Properties.Application.Commands.Produtores;

public class CreateProdutorCommand : IRequest<Guid>
{
    public string Nome { get; set; } = string.Empty;
    public string? Email { get; set; }
}
