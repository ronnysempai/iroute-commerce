using IRouteCommerce.Application.DTOs;
using IRouteCommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IRouteCommerce.Api.Controllers;

/// <summary>
/// Endpoints del comercio: carga del CSV, procesamiento por fecha y
/// consulta de los registros enviados a cuarentena.
/// </summary>
[ApiController]
[Route("api/commerce")]
[Authorize]
public class CommerceController : ControllerBase
{
    private const long MaxFileSizeBytes = 20 * 1024 * 1024;

    private readonly ICommerceService _commerceService;

    public CommerceController(ICommerceService commerceService)
    {
        _commerceService = commerceService;
    }

    /// <summary>Carga un archivo commerce_DDMMYYYY.csv e inserta sus registros.</summary>
    [HttpPost("upload")]
    [RequestSizeLimit(MaxFileSizeBytes)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UploadResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UploadResultDto>> Upload(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "El archivo se encuentra vacio." });

        await using var stream = file.OpenReadStream();
        var result = await _commerceService.UploadAsync(stream, file.FileName, cancellationToken);
        return Ok(result);
    }

    /// <summary>Procesa los registros de una fecha y mueve los invalidos a cuarentena.</summary>
    [HttpPost("process")]
    [ProducesResponseType(typeof(ProcessResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProcessResultDto>> Process(
        [FromBody] ProcessRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _commerceService.ProcessAsync(request.PcProcessdate, cancellationToken);
        return Ok(result);
    }

    /// <summary>Lista los registros que quedaron en cuarentena con su motivo.</summary>
    [HttpGet("quarantine")]
    [ProducesResponseType(typeof(IReadOnlyList<QuarantineDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<QuarantineDto>>> GetQuarantine(
        CancellationToken cancellationToken)
    {
        var result = await _commerceService.GetQuarantineAsync(cancellationToken);
        return Ok(result);
    }
}
