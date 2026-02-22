using AgroSolutions.Properties.Domain.Entities;
using AgroSolutions.Properties.Domain.Interfaces;
using MediatR;

namespace AgroSolutions.Properties.Application.Commands.Produtores;

public class CreateProdutorCommandHandler(IProdutorRepository repository)
    : IRequestHandler<CreateProdutorCommand, Guid>
{
    public async Task<Guid> Handle(
        CreateProdutorCommand request,
        CancellationToken cancellationToken
    )
    {
        var produtor = new Produtor
        {
            Id = Guid.NewGuid(),
            Nome = request.Nome,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
        };

        await repository.AddAsync(produtor, cancellationToken);

        return produtor.Id;
    }
}
