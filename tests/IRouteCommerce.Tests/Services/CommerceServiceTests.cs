using System.Text;
using IRouteCommerce.Application.DTOs;
using IRouteCommerce.Application.Exceptions;
using IRouteCommerce.Application.Services;
using IRouteCommerce.Domain.Entities;
using IRouteCommerce.Tests.Fakes;

namespace IRouteCommerce.Tests.Services;

/// <summary>Pruebas de la logica de negocio de CommerceService.</summary>
public class CommerceServiceTests
{
    private readonly FakeCommerceRepository _repository = new();
    private readonly FakeCsvService _csvService = new();
    private readonly CommerceService _service;

    public CommerceServiceTests()
    {
        _service = new CommerceService(_repository, _csvService);
    }

    private static MemoryStream NonEmptyStream() => new(Encoding.UTF8.GetBytes("contenido"));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UploadAsync_NombreVacio_LanzaAppValidationException(string fileName)
    {
        await Assert.ThrowsAsync<AppValidationException>(
            () => _service.UploadAsync(NonEmptyStream(), fileName));
    }

    [Theory]
    [InlineData("datos.csv")]
    [InlineData("commerce_2026.csv")]
    [InlineData("commerce_13102026.txt")]
    [InlineData("commerce_1310202.csv")]
    public async Task UploadAsync_NombreInvalido_LanzaAppValidationException(string fileName)
    {
        _csvService.Rows = new[] { new CommerceRowDto() };

        await Assert.ThrowsAsync<AppValidationException>(
            () => _service.UploadAsync(NonEmptyStream(), fileName));
    }

    [Fact]
    public async Task UploadAsync_ArchivoVacio_LanzaAppValidationException()
    {
        await Assert.ThrowsAsync<AppValidationException>(
            () => _service.UploadAsync(new MemoryStream(), "commerce_13102026.csv"));
    }

    [Fact]
    public async Task UploadAsync_SinRegistros_LanzaAppValidationException()
    {
        _csvService.Rows = Array.Empty<CommerceRowDto>();

        await Assert.ThrowsAsync<AppValidationException>(
            () => _service.UploadAsync(NonEmptyStream(), "commerce_13102026.csv"));
    }

    [Fact]
    public async Task UploadAsync_Ok_RetornaRegistrosCargados()
    {
        _csvService.Rows = new[] { new CommerceRowDto(), new CommerceRowDto(), new CommerceRowDto() };
        _repository.CreateBulkResult = 3;

        var result = await _service.UploadAsync(NonEmptyStream(), "commerce_13102026.csv");

        Assert.Equal(3, result.RegistrosCargados);
        Assert.Contains("3", result.Mensaje);
        Assert.Equal(3, _repository.LastInsertedRows!.Count);
    }

    [Fact]
    public async Task ProcessAsync_FechaDefault_LanzaAppValidationException()
    {
        await Assert.ThrowsAsync<AppValidationException>(() => _service.ProcessAsync(default));
    }

    [Fact]
    public async Task ProcessAsync_Ok_RetornaCantidadEnCuarentena()
    {
        var date = new DateOnly(2026, 10, 13);
        _repository.ProcessResult = 4;

        var result = await _service.ProcessAsync(date);

        Assert.Equal(4, result.RegistrosEnCuarentena);
        Assert.Equal(date, result.PcProcessdate);
        Assert.Equal(date, _repository.LastProcessedDate);
    }

    [Fact]
    public async Task GetQuarantineAsync_MapeaEntidadesADtos()
    {
        var entity = new CommerceQuarantine
        {
            Id = 1,
            PcId = 5,
            PcNomcomred = "Comercial",
            PcNumdoc = "12AB5678",
            PcProcessdate = new DateOnly(2026, 10, 13),
            Motivo = "El numero (numdoc) contiene letras",
            FechaCuarentena = new DateTime(2026, 10, 13, 10, 0, 0, DateTimeKind.Utc)
        };
        _repository.QuarantineResult = new[] { entity };

        var result = await _service.GetQuarantineAsync();

        var dto = Assert.Single(result);
        Assert.Equal(5, dto.PcId);
        Assert.Equal("Comercial", dto.PcNomcomred);
        Assert.Equal("12AB5678", dto.PcNumdoc);
        Assert.Equal("El numero (numdoc) contiene letras", dto.Motivo);
        Assert.Equal(new DateOnly(2026, 10, 13), dto.PcProcessdate);
    }

    [Fact]
    public async Task GetQuarantineAsync_SinRegistros_RetornaListaVacia()
    {
        _repository.QuarantineResult = Array.Empty<CommerceQuarantine>();

        var result = await _service.GetQuarantineAsync();

        Assert.Empty(result);
    }
}
