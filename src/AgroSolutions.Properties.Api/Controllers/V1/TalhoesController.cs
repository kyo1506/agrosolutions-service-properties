using AgroSolutions.Properties.Application.Commands.Talhoes;
using AgroSolutions.Properties.Application.Queries.Talhoes;
using AgroSolutions.Properties.Shared.DTOs;
using AgroSolutions.Properties.Shared.Models;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroSolutions.Properties.Api.Controllers.V1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/talhoes")]
[ApiController]
[Authorize]
public class TalhoesController(IMediator mediator, ILogger<TalhoesController> logger)
    : ControllerBase
{
    /// <summary>
    /// Obtém todos os talhões
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TalhaoDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<TalhaoDto>>>> GetAll(
        CancellationToken cancellationToken
    )
    {
        try
        {
            var query = new GetAllTalhoesQuery();
            var result = await mediator.Send(query, cancellationToken);
            return Ok(ApiResponse<IEnumerable<TalhaoDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting all talhoes");
            return StatusCode(
                500,
                ApiResponse<IEnumerable<TalhaoDto>>.ErrorResponse("Erro ao buscar talhões")
            );
        }
    }

    /// <summary>
    /// Obtém um talhão por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TalhaoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TalhaoDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TalhaoDto>>> GetById(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var query = new GetTalhaoByIdQuery { Id = id };
            var result = await mediator.Send(query, cancellationToken);

            if (result == null)
                return NotFound(ApiResponse<TalhaoDto>.ErrorResponse("Talhão não encontrado"));

            return Ok(ApiResponse<TalhaoDto>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting talhao {Id}", id);
            return StatusCode(500, ApiResponse<TalhaoDto>.ErrorResponse("Erro ao buscar talhão"));
        }
    }

    /// <summary>
    /// Obtém todos os talhões de uma fazenda
    /// </summary>
    [HttpGet("fazenda/{fazendaId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TalhaoDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<TalhaoDto>>>> GetByFazenda(
        Guid fazendaId,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var query = new GetTalhoesByFazendaQuery { FazendaId = fazendaId };
            var result = await mediator.Send(query, cancellationToken);
            return Ok(ApiResponse<IEnumerable<TalhaoDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting talhoes for fazenda {FazendaId}", fazendaId);
            return StatusCode(
                500,
                ApiResponse<IEnumerable<TalhaoDto>>.ErrorResponse("Erro ao buscar talhões")
            );
        }
    }

    /// <summary>
    /// Cria um novo talhão
    /// </summary>
    /// <remarks>
    /// Permite cadastrar sensores junto com o talhão.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<Guid>>> Create(
        [FromBody] CreateTalhaoCommand command,
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
            logger.LogError(ex, "Error creating talhao");
            return BadRequest(ApiResponse<Guid>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Atualiza um talhão existente
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTalhaoCommand command,
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
            logger.LogWarning(ex, "Talhao {Id} not found", id);
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating talhao {Id}", id);
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Exclui um talhão (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new DeleteTalhaoCommand { Id = id };
            await mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting talhao {Id}", id);
            return NotFound(ApiResponse<object>.ErrorResponse("Talhão não encontrado"));
        }
    }
}
