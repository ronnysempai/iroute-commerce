using System.Text.RegularExpressions;
using IRouteCommerce.Application.DTOs;
using IRouteCommerce.Application.Exceptions;
using IRouteCommerce.Application.Interfaces;
using IRouteCommerce.Domain.Entities;

namespace IRouteCommerce.Application.Services;

/// <summary>
/// Orquesta los casos de uso del comercio. No conoce detalles de acceso a datos
/// ni de infraestructura; depende de las abstracciones (puertos).
/// </summary>
public class CommerceService : ICommerceService
{
    // Formato exigido por el reto: commerce_DDMMYYYY.csv
    private static readonly Regex FileNamePattern =
        new(@"^commerce_\d{8}\.csv$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly ICommerceRepository _repository;
    private readonly ICsvService _csvService;

    public CommerceService(ICommerceRepository repository, ICsvService csvService)
    {
        _repository = repository;
        _csvService = csvService;
    }

    public async Task<UploadResultDto> UploadAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new AppValidationException("El nombre del archivo es obligatorio.");

        if (!FileNamePattern.IsMatch(fileName))
            throw new AppValidationException("El archivo debe llamarse commerce_DDMMYYYY.csv (por ejemplo commerce_13102026.csv).");

        if (fileStream is null || !fileStream.CanRead)
            throw new AppValidationException("El archivo recibido no es valido.");

        if (fileStream.CanSeek && fileStream.Length == 0)
            throw new AppValidationException("El archivo se encuentra vacio.");

        var rows = _csvService.Parse(fileStream);

        if (rows.Count == 0)
            throw new AppValidationException("El archivo no contiene registros para cargar.");

        var inserted = await _repository.CreateBulkAsync(rows, cancellationToken);

        return new UploadResultDto
        {
            RegistrosCargados = inserted,
            Mensaje = $"Se cargaron {inserted} registros correctamente."
        };
    }

    public async Task<ProcessResultDto> ProcessAsync(DateOnly processDate, CancellationToken cancellationToken = default)
    {
        if (processDate == default)
            throw new AppValidationException("La fecha de proceso es obligatoria.");

        var moved = await _repository.ProcessAsync(processDate, cancellationToken);

        return new ProcessResultDto
        {
            PcProcessdate = processDate,
            RegistrosEnCuarentena = moved,
            Mensaje = $"Se enviaron {moved} registros a cuarentena para la fecha {processDate:yyyy-MM-dd}."
        };
    }

    public async Task<IReadOnlyList<QuarantineDto>> GetQuarantineAsync(CancellationToken cancellationToken = default)
    {
        var quarantined = await _repository.GetQuarantineAsync(cancellationToken);

        return quarantined.Select(MapToDto).ToList();
    }

    private static QuarantineDto MapToDto(CommerceQuarantine entity) => new()
    {
        Id = entity.Id,
        PcId = entity.PcId,
        PcNomcomred = entity.PcNomcomred,
        PcNumdoc = entity.PcNumdoc,
        PcTipdoc = entity.PcTipdoc,
        PcDireccion = entity.PcDireccion,
        PcDepartamento = entity.PcDepartamento,
        PcProvincia = entity.PcProvincia,
        PcDistrito = entity.PcDistrito,
        PcTelefono = entity.PcTelefono,
        PcEmail = entity.PcEmail,
        PcProcessdate = entity.PcProcessdate,
        Motivo = entity.Motivo,
        FechaCuarentena = entity.FechaCuarentena
    };
}
