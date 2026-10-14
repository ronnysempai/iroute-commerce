namespace IRouteCommerce.Application.Exceptions;

/// <summary>
/// Error de regla de negocio o validacion controlada.
/// La capa Api lo traduce a una respuesta HTTP 400.
/// </summary>
public class AppValidationException : Exception
{
    public AppValidationException(string message) : base(message)
    {
    }
}
