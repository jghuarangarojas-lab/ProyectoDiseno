namespace Iot.Domain.Exceptions;

public class IotDomainException : Exception
{
    public IotDomainException(string message)
        : base(message)
    {
    }

    public IotDomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
