using MsDelivery.Domain.Enums;

namespace MsDelivery.Domain.Exceptions;

public class InvalidStatusTransitionException : DomainException
{
    public InvalidStatusTransitionException(DeliveryStatus current, DeliveryStatus target)
        : base($"Transición de estado no permitida: de {current.ToCode()} a {target.ToCode()}.")
    {
    }
}
