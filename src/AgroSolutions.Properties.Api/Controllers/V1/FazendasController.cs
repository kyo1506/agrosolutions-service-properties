using AgroSolutions.Properties.Application.Commands.Fazendas;
using AgroSolutions.Properties.Application.Queries.Fazendas;
using AgroSolutions.Properties.Shared.DTOs;
using AgroSolutions.Properties.Shared.Models;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroSolutions.Properties.Api.Controllers.V1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/fazendas")]
[ApiController]
[Authorize]
public class FazendasController(IMediator mediator, ILogger<FazendasController> logger)
    : ControllerBase
{
    /// <summary>
    /// Obtém todas as fazendas
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<FazendaDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<FazendaDto>>>> GetAll(
        CancellationToken cancellationToken
    )
    {
        try
        {
            var query = new GetAllFazendasQuery();
            var result = await mediator.Send(query, cancellationToken);
            return Ok(ApiResponse<IEnumerable<FazendaDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting all fazendas");
            return StatusCode(
                500,
                ApiResponse<IEnumerable<FazendaDto>>.ErrorResponse("Erro ao buscar fazendas")
            );
        }
    }

    /// <summary>
    /// Obtém uma fazenda por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<FazendaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FazendaDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<FazendaDto>>> GetById(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var query = new GetFazendaByIdQuery { Id = id };
            var result = await mediator.Send(query, cancellationToken);

            if (result == null)
                return NotFound(ApiResponse<FazendaDto>.ErrorResponse("Fazenda não encontrada"));

            return Ok(ApiResponse<FazendaDto>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting fazenda {Id}", id);
            return StatusCode(500, ApiResponse<FazendaDto>.ErrorResponse("Erro ao buscar fazenda"));
        }
    }

    /// <summary>
    /// Obtém todas as fazendas de um produtor
    /// </summary>
    [HttpGet("produtor/{produtorId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<FazendaDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<FazendaDto>>>> GetByProdutor(
        Guid produtorId,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var query = new GetFazendasByProdutorQuery { ProdutorId = produtorId };
            var result = await mediator.Send(query, cancellationToken);
            return Ok(ApiResponse<IEnumerable<FazendaDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting fazendas for produtor {ProdutorId}", produtorId);
            return StatusCode(
                500,
                ApiResponse<IEnumerable<FazendaDto>>.ErrorResponse("Erro ao buscar fazendas")
            );
        }
    }

    /// <summary>
    /// Cria uma nova fazenda
    /// </summary>
    /// <remarks>
    /// Permite cadastrar talhões e sensores junto com a fazenda.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<Guid>>> Create(
        [FromBody] CreateFazendaCommand command,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var id = await mediator.Send(command, cancellationToken);
            return CreatedAtAction(
                nameof(GetById),
                new { id },
                ApiResponse<Guid>.SuccessResponse(id)
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating fazenda");
            return BadRequest(ApiResponse<Guid>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Atualiza uma fazenda existente
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateFazendaCommand command,
        CancellationToken cancellationToken
    )
    {
        try
        {
            if (id != command.Id)
                return BadRequest(ApiResponse<object>.ErrorResponse("IDs não correspondem"));

            await mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Fazenda {Id} not found", id);
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating fazenda {Id}", id);
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Exclui uma fazenda (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new DeleteFazendaCommand { Id = id };
            await mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting fazenda {Id}", id);
            return NotFound(ApiResponse<object>.ErrorResponse("Fazenda não encontrada"));
        }
    }
}
