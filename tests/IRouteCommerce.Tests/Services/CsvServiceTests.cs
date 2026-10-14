using System.Text;
using IRouteCommerce.Infrastructure.Services;

namespace IRouteCommerce.Tests.Services;

/// <summary>Pruebas de la lectura del CSV con CsvHelper (mapeo por cabeceras pc_*).</summary>
public class CsvServiceTests
{
    private const string Header =
        "pc_id,pc_nomcomred,pc_numdoc,pc_tipdoc,pc_direccion,pc_departamento," +
        "pc_provincia,pc_distrito,pc_telefono,pc_email,pc_processdate";

    private readonly CsvService _csvService = new();

    [Fact]
    public void Parse_MapeaCamposPorNombreDeCabecera()
    {
        var content =
            Header + "\n" +
            "1,Comercial Uno,12345678,1,Av. Uno,Lima,Lima,Miraflores,987654321,uno@example.com,2026-10-13\n" +
            "2,,10203040,1,Jr. Dos,Lima,Lima,Surco,912345678,dos@example.com,2026-10-13\n";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        var rows = _csvService.Parse(stream);

        Assert.Equal(2, rows.Count);
        Assert.Equal("1", rows[0].PcId);
        Assert.Equal("Comercial Uno", rows[0].PcNomcomred);
        Assert.Equal("12345678", rows[0].PcNumdoc);
        Assert.Equal("uno@example.com", rows[0].PcEmail);
        Assert.Equal("2026-10-13", rows[0].PcProcessdate);
        Assert.True(string.IsNullOrEmpty(rows[1].PcNomcomred));
    }

    [Fact]
    public void Parse_SoloCabecera_RetornaVacio()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(Header + "\n"));

        var rows = _csvService.Parse(stream);

        Assert.Empty(rows);
    }
}
