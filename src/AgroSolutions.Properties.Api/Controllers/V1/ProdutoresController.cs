using AgroSolutions.Properties.Application.Queries.Produtores;
using AgroSolutions.Properties.Shared.DTOs;
using AgroSolutions.Properties.Shared.Models;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroSolutions.Properties.Api.Controllers.V1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/produtores")]
[ApiController]
[Authorize]
public class ProdutoresController(IMediator mediator, ILogger<ProdutoresController> logger)
    : ControllerBase
{
    /// <summary>
    /// Obtém todos os produtores
    /// </summary>
    /// <remarks>
    /// Produtores são sincronizados automaticamente do serviço Identity via eventos.
    /// Este endpoint é apenas para consulta.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProdutorDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProdutorDto>>>> GetAll(
        CancellationToken cancellationToken
    )
    {
        try
        {
            var query = new GetAllProdutoresQuery();
            var result = await mediator.Send(query, cancellationToken);
            return Ok(ApiResponse<IEnumerable<ProdutorDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting all produtores");
            return StatusCode(
                500,
                ApiResponse<IEnumerable<ProdutorDto>>.ErrorResponse("Erro ao buscar produtores")
            );
        }
    }
}
