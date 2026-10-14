namespace IRouteCommerce.Application.DTOs;

/// <summary>Registro en cuarentena expuesto al frontend.</summary>
public class QuarantineDto
{
    public int Id { get; set; }
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
    public string Motivo { get; set; } = string.Empty;
    public DateTime FechaCuarentena { get; set; }
}
