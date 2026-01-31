using FluentValidation;

namespace AgroSolutions.Properties.Application.Commands.Sensores;

public class UpdateSensorCommandValidator : AbstractValidator<UpdateSensorCommand>
{
    public UpdateSensorCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id é obrigatório");

        RuleFor(x => x.Status)
            .Must(BeValidStatus)
            .When(x => !string.IsNullOrEmpty(x.Status))
            .WithMessage("Status inválido");

        RuleFor(x => x.IntervaloLeituraMinutos)
            .GreaterThan(0)
            .When(x => x.IntervaloLeituraMinutos.HasValue)
            .WithMessage("Intervalo de leitura deve ser maior que zero");
    }

    private bool BeValidStatus(string? status)
    {
        if (string.IsNullOrEmpty(status))
            return true;
        return Enum.TryParse<Domain.Enums.StatusSensor>(status, out _);
    }
}
