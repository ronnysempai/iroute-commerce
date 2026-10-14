using IRouteCommerce.Application.DTOs;

namespace IRouteCommerce.Application.Interfaces;

/// <summary>Servicio de lectura del archivo CSV de comercios.</summary>
public interface ICsvService
{
    /// <summary>Lee el stream del CSV y devuelve las filas crudas.</summary>
    IReadOnlyList<CommerceRowDto> Parse(Stream stream);
}
