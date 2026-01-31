using FluentValidation;

namespace AgroSolutions.Properties.Application.Commands.Fazendas;

public class UpdateFazendaCommandValidator : AbstractValidator<UpdateFazendaCommand>
{
    public UpdateFazendaCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id é obrigatório");

        RuleFor(x => x.Nome)
            .NotEmpty()
            .WithMessage("Nome é obrigatório")
            .MaximumLength(200)
            .WithMessage("Nome deve ter no máximo 200 caracteres");

        RuleFor(x => x.AreaTotal).GreaterThan(0).WithMessage("Área total deve ser maior que zero");
    }
}
