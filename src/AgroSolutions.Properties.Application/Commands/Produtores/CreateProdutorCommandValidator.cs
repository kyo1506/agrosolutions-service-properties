using FluentValidation;

namespace AgroSolutions.Properties.Application.Commands.Produtores;

public class CreateProdutorCommandValidator : AbstractValidator<CreateProdutorCommand>
{
    public CreateProdutorCommandValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty()
            .WithMessage("Nome é obrigatório")
            .MaximumLength(200)
            .WithMessage("Nome deve ter no máximo 200 caracteres");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Email inválido");
    }
}
