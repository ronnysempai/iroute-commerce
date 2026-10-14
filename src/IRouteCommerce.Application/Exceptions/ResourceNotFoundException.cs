namespace IRouteCommerce.Application.Exceptions;

/// <summary>
/// Recurso no encontrado. La capa Api lo traduce a una respuesta HTTP 404.
/// </summary>
public class ResourceNotFoundException : Exception
{
    public ResourceNotFoundException(string message) : base(message)
    {
    }
}
