using FluentValidation;

namespace AgroSolutions.Properties.Application.Commands.Talhoes;

public class CreateTalhaoCommandValidator : AbstractValidator<CreateTalhaoCommand>
{
    public CreateTalhaoCommandValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty()
            .WithMessage("Nome é obrigatório")
            .MaximumLength(200)
            .WithMessage("Nome deve ter no máximo 200 caracteres");

        RuleFor(x => x.Area).GreaterThan(0).WithMessage("Área deve ser maior que zero");

        RuleFor(x => x.FazendaId).NotEmpty().WithMessage("FazendaId é obrigatório");

        RuleForEach(x => x.Sensores)
            .ChildRules(sensor =>
            {
                sensor
                    .RuleFor(s => s.CodigoIdentificacao)
                    .NotEmpty()
                    .WithMessage("Código do sensor é obrigatório")
                    .MaximumLength(50)
                    .WithMessage("Código deve ter no máximo 50 caracteres");

                sensor
                    .RuleFor(s => s.Tipo)
                    .NotEmpty()
                    .WithMessage("Tipo do sensor é obrigatório")
                    .Must(BeValidSensorType)
                    .WithMessage("Tipo de sensor inválido");
            });
    }

    private bool BeValidSensorType(string tipo)
    {
        return Enum.TryParse<Domain.Enums.TipoSensor>(tipo, out _);
    }
}
