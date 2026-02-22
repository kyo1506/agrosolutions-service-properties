using AgroSolutions.Identity.Domain.Events;
using AgroSolutions.Properties.Domain.Entities;
using AgroSolutions.Properties.Domain.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace AgroSolutions.Properties.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumer MassTransit para sincronizar criação de produtores a partir do evento UserCreatedEvent do serviço Identity.
/// Mapeia UserId → Produtor.Id e FirstName+LastName → Nome.
/// </summary>
public class UserCreatedEventConsumer(
    IProdutorRepository produtorRepository,
    ILogger<UserCreatedEventConsumer> logger
) : IConsumer<UserCreatedEvent>
{
    public async Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "Sincronizando novo produtor a partir do Identity UserId={UserId} Username={Username}",
            message.UserId,
            message.Username
        );

        // Verificar se já existe
        var existingProdutor = await produtorRepository.GetByIdAsync(
            message.UserId,
            context.CancellationToken
        );
        if (existingProdutor != null)
        {
            logger.LogWarning(
                "Produtor {UserId} já existe no banco. Ignorando evento de criação.",
                message.UserId
            );
            return;
        }

        var nome = $"{message.FirstName} {message.LastName}".Trim();

        var produtor = new Produtor
        {
            Id = message.UserId,
            Nome = string.IsNullOrWhiteSpace(nome) ? message.Username : nome,
            Email = message.Email,
            CreatedAt = message.Timestamp,
            IsActive = message.IsEnabled,
        };

        await produtorRepository.AddAsync(produtor, context.CancellationToken);

        logger.LogInformation(
            "Produtor {UserId} ({Nome}) sincronizado com sucesso",
            message.UserId,
            produtor.Nome
        );
    }
}

/// <summary>
/// Consumer MassTransit para atualizar produtores a partir do evento UserUpdatedEvent do serviço Identity.
/// </summary>
public class UserUpdatedEventConsumer(
    IProdutorRepository produtorRepository,
    ILogger<UserUpdatedEventConsumer> logger
) : IConsumer<UserUpdatedEvent>
{
    public async Task Consume(ConsumeContext<UserUpdatedEvent> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "Atualizando produtor a partir do Identity UserId={UserId}",
            message.UserId
        );

        var produtor = await produtorRepository.GetByIdAsync(
            message.UserId,
            context.CancellationToken
        );
        if (produtor == null)
        {
            logger.LogWarning(
                "Produtor {UserId} não encontrado. Criando novo registro a partir de UserUpdatedEvent.",
                message.UserId
            );

            produtor = new Produtor
            {
                Id = message.UserId,
                CreatedAt = DateTime.UtcNow,
                IsActive = message.IsEnabled,
            };
        }

        // Atualizar campos vindos do Identity
        var nome = $"{message.FirstName} {message.LastName}".Trim();
        if (!string.IsNullOrWhiteSpace(nome))
        {
            produtor.Nome = nome;
        }

        produtor.Email = message.Email;
        produtor.IsActive = message.IsEnabled;
        produtor.UpdatedAt = message.Timestamp;

        if (produtor.CreatedAt == default)
        {
            await produtorRepository.AddAsync(produtor, context.CancellationToken);
        }
        else
        {
            await produtorRepository.UpdateAsync(produtor, context.CancellationToken);
        }

        logger.LogInformation("Produtor {UserId} atualizado com sucesso", message.UserId);
    }
}

/// <summary>
/// Consumer MassTransit para soft-delete de produtores a partir do evento UserDeletedEvent do serviço Identity.
/// </summary>
public class UserDeletedEventConsumer(
    IProdutorRepository produtorRepository,
    ILogger<UserDeletedEventConsumer> logger
) : IConsumer<UserDeletedEvent>
{
    public async Task Consume(ConsumeContext<UserDeletedEvent> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "Deletando produtor a partir do Identity UserId={UserId}",
            message.UserId
        );

        var produtor = await produtorRepository.GetByIdAsync(
            message.UserId,
            context.CancellationToken
        );
        if (produtor == null)
        {
            logger.LogWarning("Produtor {UserId} não encontrado para deleção", message.UserId);
            return;
        }

        // Soft delete
        await produtorRepository.DeleteAsync(message.UserId, context.CancellationToken);

        logger.LogInformation("Produtor {UserId} marcado como inativo com sucesso", message.UserId);
    }
}
