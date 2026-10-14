using CsvHelper.Configuration;
using IRouteCommerce.Application.DTOs;

namespace IRouteCommerce.Infrastructure.Csv;

/// <summary>
/// Mapeo explicito entre las cabeceras del CSV (pc_*) y las propiedades del DTO.
/// </summary>
public sealed class CommerceRowMap : ClassMap<CommerceRowDto>
{
    public CommerceRowMap()
    {
        Map(m => m.PcId).Name("pc_id");
        Map(m => m.PcNomcomred).Name("pc_nomcomred");
        Map(m => m.PcNumdoc).Name("pc_numdoc");
        Map(m => m.PcTipdoc).Name("pc_tipdoc");
        Map(m => m.PcDireccion).Name("pc_direccion");
        Map(m => m.PcDepartamento).Name("pc_departamento");
        Map(m => m.PcProvincia).Name("pc_provincia");
        Map(m => m.PcDistrito).Name("pc_distrito");
        Map(m => m.PcTelefono).Name("pc_telefono");
        Map(m => m.PcEmail).Name("pc_email");
        Map(m => m.PcProcessdate).Name("pc_processdate");
    }
}
