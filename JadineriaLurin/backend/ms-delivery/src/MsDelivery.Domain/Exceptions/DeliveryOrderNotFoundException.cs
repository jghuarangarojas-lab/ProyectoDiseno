namespace MsDelivery.Domain.Exceptions;

public class DeliveryOrderNotFoundException : DomainException
{
    public DeliveryOrderNotFoundException(Guid id)
        : base($"No existe una orden de entrega con id '{id}'.")
    {
    }
}
