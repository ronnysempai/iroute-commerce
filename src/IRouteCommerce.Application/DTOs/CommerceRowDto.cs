using System.Text.Json.Serialization;

namespace IRouteCommerce.Application.DTOs;

/// <summary>
/// Representa una fila cruda del archivo commerce_DDMMYYYY.csv.
/// Los valores se mantienen como texto para conservar los datos originales
/// (incluidos los invalidos que luego se mueven a cuarentena).
/// Los nombres JSON coinciden con las columnas que espera sp_create_commerce.
/// </summary>
public class CommerceRowDto
{
    [JsonPropertyName("pc_id")]
    public string? PcId { get; set; }

    [JsonPropertyName("pc_nomcomred")]
    public string? PcNomcomred { get; set; }

    [JsonPropertyName("pc_numdoc")]
    public string? PcNumdoc { get; set; }

    [JsonPropertyName("pc_tipdoc")]
    public string? PcTipdoc { get; set; }

    [JsonPropertyName("pc_direccion")]
    public string? PcDireccion { get; set; }

    [JsonPropertyName("pc_departamento")]
    public string? PcDepartamento { get; set; }

    [JsonPropertyName("pc_provincia")]
    public string? PcProvincia { get; set; }

    [JsonPropertyName("pc_distrito")]
    public string? PcDistrito { get; set; }

    [JsonPropertyName("pc_telefono")]
    public string? PcTelefono { get; set; }

    [JsonPropertyName("pc_email")]
    public string? PcEmail { get; set; }

    [JsonPropertyName("pc_processdate")]
    public string? PcProcessdate { get; set; }
}
