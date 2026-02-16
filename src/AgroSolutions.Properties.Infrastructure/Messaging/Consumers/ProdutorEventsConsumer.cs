using AgroSolutions.Properties.Domain.Entities;
using AgroSolutions.Properties.Domain.Events;
using AgroSolutions.Properties.Domain.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace AgroSolutions.Properties.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumer MassTransit para sincronizar criação de produtores do serviço Identity
/// </summary>
public class ProdutorCreatedEventConsumer(
    IProdutorRepository produtorRepository,
    ILogger<ProdutorCreatedEventConsumer> logger
) : IConsumer<ProdutorCreatedEvent>
{
    public async Task Consume(ConsumeContext<ProdutorCreatedEvent> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "Sincronizando novo produtor {ProdutorId} - {Nome}",
            message.ProdutorId,
            message.Nome
        );

        // Verificar se já existe
        var existingProdutor = await produtorRepository.GetByIdAsync(
            message.ProdutorId,
            context.CancellationToken
        );
        if (existingProdutor != null)
        {
            logger.LogWarning(
                "Produtor {ProdutorId} já existe no banco. Ignorando evento de criação.",
                message.ProdutorId
            );
            return;
        }

        var produtor = new Produtor
        {
            Id = message.ProdutorId,
            Nome = message.Nome,
            Cpf = message.Cpf,
            Email = message.Email,
            Telefone = message.Telefone,
            Endereco = message.Endereco,
            Cidade = message.Cidade,
            Estado = message.Estado,
            Cep = message.Cep,
            CreatedAt = message.Timestamp,
            IsActive = true,
        };

        await produtorRepository.AddAsync(produtor, context.CancellationToken);

        logger.LogInformation("Produtor {ProdutorId} sincronizado com sucesso", message.ProdutorId);
    }
}

/// <summary>
/// Consumer MassTransit para atualizar produtores do serviço Identity
/// </summary>
public class ProdutorUpdatedEventConsumer(
    IProdutorRepository produtorRepository,
    ILogger<ProdutorUpdatedEventConsumer> logger
) : IConsumer<ProdutorUpdatedEvent>
{
    public async Task Consume(ConsumeContext<ProdutorUpdatedEvent> context)
    {
        var message = context.Message;

        logger.LogInformation("Atualizando produtor {ProdutorId}", message.ProdutorId);

        var produtor = await produtorRepository.GetByIdAsync(
            message.ProdutorId,
            context.CancellationToken
        );
        if (produtor == null)
        {
            logger.LogWarning(
                "Produtor {ProdutorId} não encontrado. Criando novo registro.",
                message.ProdutorId
            );

            produtor = new Produtor
            {
                Id = message.ProdutorId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
            };
        }

        // Atualizar campos
        produtor.Nome = message.Nome;
        produtor.Email = message.Email;
        produtor.Telefone = message.Telefone;
        produtor.Endereco = message.Endereco;
        produtor.Cidade = message.Cidade;
        produtor.Estado = message.Estado;
        produtor.Cep = message.Cep;
        produtor.UpdatedAt = message.Timestamp;

        if (produtor.CreatedAt == default)
        {
            await produtorRepository.AddAsync(produtor, context.CancellationToken);
        }
        else
        {
            await produtorRepository.UpdateAsync(produtor, context.CancellationToken);
        }

        logger.LogInformation("Produtor {ProdutorId} atualizado com sucesso", message.ProdutorId);
    }
}

/// <summary>
/// Consumer MassTransit para deletar produtores do serviço Identity
/// </summary>
public class ProdutorDeletedEventConsumer(
    IProdutorRepository produtorRepository,
    ILogger<ProdutorDeletedEventConsumer> logger
) : IConsumer<ProdutorDeletedEvent>
{
    public async Task Consume(ConsumeContext<ProdutorDeletedEvent> context)
    {
        var message = context.Message;

        logger.LogInformation("Deletando produtor {ProdutorId}", message.ProdutorId);

        var produtor = await produtorRepository.GetByIdAsync(
            message.ProdutorId,
            context.CancellationToken
        );
        if (produtor == null)
        {
            logger.LogWarning("Produtor {ProdutorId} não encontrado", message.ProdutorId);
            return;
        }

        // Soft delete
        await produtorRepository.DeleteAsync(message.ProdutorId, context.CancellationToken);

        logger.LogInformation(
            "Produtor {ProdutorId} marcado como inativo com sucesso",
            message.ProdutorId
        );
    }
}
