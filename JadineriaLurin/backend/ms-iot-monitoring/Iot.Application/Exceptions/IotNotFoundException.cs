namespace Iot.Application.Exceptions;

public class IotNotFoundException : Exception
{
    public IotNotFoundException(string message)
        : base(message)
    {
    }

    public IotNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
