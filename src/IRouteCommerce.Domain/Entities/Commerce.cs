namespace IRouteCommerce.Domain.Entities;

/// <summary>
/// Registro de comercio tal como viene en el archivo commerce_DDMMYYYY.csv.
/// La estructura de columnas corresponde 1:1 con la tabla dbo.commerce.
/// </summary>
public class Commerce
{
    public int PcId { get; set; }
    public string? PcNomcomred { get; set; }
    public string? PcNumdoc { get; set; }
    public string? PcTipdoc { get; set; }
    public string? PcDireccion { get; set; }
    public string? PcDepartamento { get; set; }
    public string? PcProvincia { get; set; }
    public string? PcDistrito { get; set; }
    public string? PcTelefono { get; set; }
    public string? PcEmail { get; set; }
    public DateOnly PcProcessdate { get; set; }
}
