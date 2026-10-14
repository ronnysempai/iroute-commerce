using IRouteCommerce.Application.DTOs;

namespace IRouteCommerce.Application.Interfaces;

/// <summary>
/// Servicio de aplicacion que orquesta los casos de uso del comercio:
/// carga del CSV, procesamiento por fecha y consulta de cuarentena.
/// </summary>
public interface ICommerceService
{
    Task<UploadResultDto> UploadAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);

    Task<ProcessResultDto> ProcessAsync(DateOnly processDate, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<QuarantineDto>> GetQuarantineAsync(CancellationToken cancellationToken = default);
}
