namespace Irrigation.Domain.Exceptions;

public class IrrigationDomainException : Exception
{
    public IrrigationDomainException(string message)
        : base(message)
    {
    }

    public IrrigationDomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
