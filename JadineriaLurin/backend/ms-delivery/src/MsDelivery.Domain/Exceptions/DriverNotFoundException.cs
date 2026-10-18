namespace MsDelivery.Domain.Exceptions;

public class DriverNotFoundException : DomainException
{
    public DriverNotFoundException(Guid id)
        : base($"No existe un repartidor con id '{id}'.")
    {
    }
}
