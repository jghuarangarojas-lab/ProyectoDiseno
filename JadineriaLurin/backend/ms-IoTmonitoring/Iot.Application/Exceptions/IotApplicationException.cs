namespace Iot.Application.Exceptions;

public class IotApplicationException : Exception
{
    public IotApplicationException(string message)
        : base(message)
    {
    }

    public IotApplicationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
