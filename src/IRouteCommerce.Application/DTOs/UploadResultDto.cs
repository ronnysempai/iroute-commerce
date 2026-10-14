namespace IRouteCommerce.Application.DTOs;

/// <summary>Resultado de la carga de un archivo CSV.</summary>
public class UploadResultDto
{
    public int RegistrosCargados { get; set; }
    public string Mensaje { get; set; } = string.Empty;
}
