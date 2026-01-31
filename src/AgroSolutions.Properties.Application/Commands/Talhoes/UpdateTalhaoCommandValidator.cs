using FluentValidation;

namespace AgroSolutions.Properties.Application.Commands.Talhoes;

public class UpdateTalhaoCommandValidator : AbstractValidator<UpdateTalhaoCommand>
{
    public UpdateTalhaoCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id é obrigatório");

        RuleFor(x => x.Nome)
            .NotEmpty()
            .WithMessage("Nome é obrigatório")
            .MaximumLength(200)
            .WithMessage("Nome deve ter no máximo 200 caracteres");

        RuleFor(x => x.Area).GreaterThan(0).WithMessage("Área deve ser maior que zero");
    }
}
