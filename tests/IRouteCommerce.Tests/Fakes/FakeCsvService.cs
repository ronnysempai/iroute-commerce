using IRouteCommerce.Application.DTOs;
using IRouteCommerce.Application.Interfaces;

namespace IRouteCommerce.Tests.Fakes;

/// <summary>Doble de prueba de ICsvService que devuelve filas predefinidas.</summary>
internal sealed class FakeCsvService : ICsvService
{
    public IReadOnlyList<CommerceRowDto> Rows { get; set; } = Array.Empty<CommerceRowDto>();

    public bool WasCalled { get; private set; }

    public IReadOnlyList<CommerceRowDto> Parse(Stream stream)
    {
        WasCalled = true;
        return Rows;
    }
}
