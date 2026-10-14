using IRouteCommerce.Application.DTOs;
using IRouteCommerce.Domain.Entities;

namespace IRouteCommerce.Application.Interfaces;

/// <summary>
/// Puerto de acceso a datos para los procesos de comercio.
/// La implementacion (Infrastructure) invoca los stored procedures.
/// </summary>
public interface ICommerceRepository
{
    /// <summary>Inserta en masa las filas del CSV. Retorna la cantidad insertada.</summary>
    Task<int> CreateBulkAsync(IReadOnlyList<CommerceRowDto> rows, CancellationToken cancellationToken = default);

    /// <summary>Valida una fecha y mueve los invalidos a cuarentena. Retorna la cantidad movida.</summary>
    Task<int> ProcessAsync(DateOnly processDate, CancellationToken cancellationToken = default);

    /// <summary>Obtiene todos los registros en cuarentena.</summary>
    Task<IReadOnlyList<CommerceQuarantine>> GetQuarantineAsync(CancellationToken cancellationToken = default);
}
