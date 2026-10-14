namespace IRouteCommerce.Application.DTOs;

/// <summary>Resultado del proceso de validacion de una fecha.</summary>
public class ProcessResultDto
{
    public DateOnly PcProcessdate { get; set; }
    public int RegistrosEnCuarentena { get; set; }
    public string Mensaje { get; set; } = string.Empty;
}
