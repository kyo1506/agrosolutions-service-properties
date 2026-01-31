using FluentValidation;

namespace AgroSolutions.Properties.Application.Commands.Fazendas;

public class CreateFazendaCommandValidator : AbstractValidator<CreateFazendaCommand>
{
    public CreateFazendaCommandValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty()
            .WithMessage("Nome é obrigatório")
            .MaximumLength(200)
            .WithMessage("Nome deve ter no máximo 200 caracteres");

        RuleFor(x => x.AreaTotal).GreaterThan(0).WithMessage("Área total deve ser maior que zero");

        RuleFor(x => x.ProdutorId).NotEmpty().WithMessage("ProdutorId é obrigatório");

        RuleForEach(x => x.Talhoes)
            .ChildRules(talhao =>
            {
                talhao
                    .RuleFor(t => t.Nome)
                    .NotEmpty()
                    .WithMessage("Nome do talhão é obrigatório")
                    .MaximumLength(200)
                    .WithMessage("Nome do talhão deve ter no máximo 200 caracteres");

                talhao
                    .RuleFor(t => t.Area)
                    .GreaterThan(0)
                    .WithMessage("Área do talhão deve ser maior que zero");

                talhao
                    .RuleForEach(t => t.Sensores)
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
            });
    }

    private bool BeValidSensorType(string tipo)
    {
        return Enum.TryParse<Domain.Enums.TipoSensor>(tipo, out _);
    }
}
