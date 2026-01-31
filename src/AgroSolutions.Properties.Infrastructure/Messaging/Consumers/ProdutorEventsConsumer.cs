using AgroSolutions.Properties.Domain.Entities;
using AgroSolutions.Properties.Domain.Events;
using AgroSolutions.Properties.Domain.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace AgroSolutions.Properties.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumer para sincronizar produtores do serviço Identity
/// </summary>
public class ProdutorEventsConsumer(
    IProdutorRepository produtorRepository,
    ILogger<ProdutorEventsConsumer> logger
)
    : IConsumer<ProdutorCreatedEvent>,
        IConsumer<ProdutorUpdatedEvent>,
        IConsumer<ProdutorDeletedEvent>
{
    public async Task Consume(ConsumeContext<ProdutorCreatedEvent> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "Sincronizando novo produtor {ProdutorId} - {Nome}",
            message.ProdutorId,
            message.Nome
        );

        try
        {
            // Verificar se já existe
            var existingProdutor = await produtorRepository.GetByIdAsync(message.ProdutorId);
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

            await produtorRepository.AddAsync(produtor);

            logger.LogInformation(
                "Produtor {ProdutorId} sincronizado com sucesso",
                message.ProdutorId
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao sincronizar produtor {ProdutorId}", message.ProdutorId);
            throw;
        }
    }

    public async Task Consume(ConsumeContext<ProdutorUpdatedEvent> context)
    {
        var message = context.Message;

        logger.LogInformation("Atualizando produtor {ProdutorId}", message.ProdutorId);

        try
        {
            var produtor = await produtorRepository.GetByIdAsync(message.ProdutorId);
            if (produtor == null)
            {
                logger.LogWarning(
                    "Produtor {ProdutorId} não encontrado para atualização. Criando novo registro.",
                    message.ProdutorId
                );

                // Criar se não existir (cenário de reprocessamento)
                produtor = new Produtor
                {
                    Id = message.ProdutorId,
                    Cpf = "00000000000", // CPF não vem no update, usar valor dummy
                    CreatedAt = message.Timestamp,
                    IsActive = true,
                };
            }

            // Atualizar dados
            produtor.Nome = message.Nome;
            produtor.Email = message.Email;
            produtor.Telefone = message.Telefone;
            produtor.Endereco = message.Endereco;
            produtor.Cidade = message.Cidade;
            produtor.Estado = message.Estado;
            produtor.Cep = message.Cep;
            produtor.UpdatedAt = message.Timestamp;

            await produtorRepository.UpdateAsync(produtor);

            logger.LogInformation(
                "Produtor {ProdutorId} atualizado com sucesso",
                message.ProdutorId
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao atualizar produtor {ProdutorId}", message.ProdutorId);
            throw;
        }
    }

    public async Task Consume(ConsumeContext<ProdutorDeletedEvent> context)
    {
        var message = context.Message;

        logger.LogInformation("Excluindo (soft delete) produtor {ProdutorId}", message.ProdutorId);

        try
        {
            var produtor = await produtorRepository.GetByIdAsync(message.ProdutorId);
            if (produtor == null)
            {
                logger.LogWarning(
                    "Produtor {ProdutorId} não encontrado para exclusão",
                    message.ProdutorId
                );
                return;
            }

            produtor.IsActive = false;
            produtor.UpdatedAt = message.Timestamp;

            await produtorRepository.UpdateAsync(produtor);

            logger.LogInformation("Produtor {ProdutorId} excluído com sucesso", message.ProdutorId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao excluir produtor {ProdutorId}", message.ProdutorId);
            throw;
        }
    }
}
