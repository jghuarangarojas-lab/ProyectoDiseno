namespace MsDelivery.Domain.Exceptions;

public class ProofOfDeliveryMissingException : DomainException
{
    public ProofOfDeliveryMissingException()
        : base("No se puede marcar la orden como DELIVERED sin una foto de evidencia o una firma digital del cliente.")
    {
    }
}
