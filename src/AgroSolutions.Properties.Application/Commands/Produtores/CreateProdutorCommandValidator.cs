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

        RuleFor(x => x.Cpf)
            .NotEmpty()
            .WithMessage("CPF é obrigatório")
            .Length(11)
            .WithMessage("CPF deve ter 11 dígitos")
            .Matches(@"^\d{11}$")
            .WithMessage("CPF deve conter apenas números");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Email inválido");

        RuleFor(x => x.Telefone)
            .MaximumLength(20)
            .When(x => !string.IsNullOrEmpty(x.Telefone))
            .WithMessage("Telefone deve ter no máximo 20 caracteres");
    }
}
