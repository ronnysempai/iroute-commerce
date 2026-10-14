using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using IRouteCommerce.Application.DTOs;
using IRouteCommerce.Application.Interfaces;
using IRouteCommerce.Infrastructure.Csv;

namespace IRouteCommerce.Infrastructure.Services;

/// <summary>
/// Lectura del archivo commerce_DDMMYYYY.csv usando CsvHelper.
/// Conserva los valores crudos como texto.
/// </summary>
public class CsvService : ICsvService
{
    public IReadOnlyList<CommerceRowDto> Parse(Stream stream)
    {
        using var reader = new StreamReader(stream, detectEncodingFromByteOrderMarks: true);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            TrimOptions = TrimOptions.Trim,
            HeaderValidated = null,
            MissingFieldFound = null,
            BadDataFound = null,
            IgnoreBlankLines = true
        });

        csv.Context.RegisterClassMap<CommerceRowMap>();

        var rows = csv.GetRecords<CommerceRowDto>().ToList();
        return rows;
    }
}
