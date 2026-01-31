using FluentValidation;

namespace AgroSolutions.Properties.Application.Commands.Sensores;

public class CreateSensorCommandValidator : AbstractValidator<CreateSensorCommand>
{
    public CreateSensorCommandValidator()
    {
        RuleFor(x => x.CodigoIdentificacao)
            .NotEmpty()
            .WithMessage("Código de identificação é obrigatório")
            .MaximumLength(50)
            .WithMessage("Código deve ter no máximo 50 caracteres");

        RuleFor(x => x.Tipo)
            .NotEmpty()
            .WithMessage("Tipo do sensor é obrigatório")
            .Must(BeValidSensorType)
            .WithMessage("Tipo de sensor inválido");

        RuleFor(x => x.TalhaoId).NotEmpty().WithMessage("TalhaoId é obrigatório");

        RuleFor(x => x.IntervaloLeituraMinutos)
            .GreaterThan(0)
            .When(x => x.IntervaloLeituraMinutos.HasValue)
            .WithMessage("Intervalo de leitura deve ser maior que zero");
    }

    private bool BeValidSensorType(string tipo)
    {
        return Enum.TryParse<Domain.Enums.TipoSensor>(tipo, true, out _);
    }
}
