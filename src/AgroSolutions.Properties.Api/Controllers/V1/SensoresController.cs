using AgroSolutions.Properties.Application.Commands.Sensores;
using AgroSolutions.Properties.Application.Queries.Sensores;
using AgroSolutions.Properties.Shared.DTOs;
using AgroSolutions.Properties.Shared.Models;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroSolutions.Properties.Api.Controllers.V1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/sensores")]
[ApiController]
[Authorize]
public class SensoresController(IMediator mediator, ILogger<SensoresController> logger)
    : ControllerBase
{
    /// <summary>
    /// Obtém todos os sensores de um talhão
    /// </summary>
    [HttpGet("talhao/{talhaoId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SensorDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<SensorDto>>>> GetByTalhao(
        Guid talhaoId,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var query = new GetSensoresByTalhaoQuery { TalhaoId = talhaoId };
            var result = await mediator.Send(query, cancellationToken);
            return Ok(ApiResponse<IEnumerable<SensorDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting sensores for talhao {TalhaoId}", talhaoId);
            return StatusCode(
                500,
                ApiResponse<IEnumerable<SensorDto>>.ErrorResponse("Erro ao buscar sensores")
            );
        }
    }

    /// <summary>
    /// Obtém um sensor por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SensorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SensorDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SensorDto>>> GetById(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var query = new GetSensorByIdQuery { Id = id };
            var result = await mediator.Send(query, cancellationToken);

            if (result == null)
                return NotFound(ApiResponse<SensorDto>.ErrorResponse("Sensor não encontrado"));

            return Ok(ApiResponse<SensorDto>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting sensor {Id}", id);
            return StatusCode(500, ApiResponse<SensorDto>.ErrorResponse("Erro ao buscar sensor"));
        }
    }

    /// <summary>
    /// Cria um novo sensor
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<Guid>>> Create(
        [FromBody] CreateSensorCommand command,
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
            logger.LogError(ex, "Error creating sensor");
            return BadRequest(ApiResponse<Guid>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Atualiza um sensor existente
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateSensorCommand command,
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
            logger.LogWarning(ex, "Sensor {Id} not found", id);
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating sensor {Id}", id);
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Exclui um sensor (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new DeleteSensorCommand { Id = id };
            await mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting sensor {Id}", id);
            return NotFound(ApiResponse<object>.ErrorResponse("Sensor não encontrado"));
        }
    }
}
