namespace Irrigation.Application.Exceptions;

public class IrrigationApplicationException : Exception
{
    public IrrigationApplicationException(string message)
        : base(message)
    {
    }

    public IrrigationApplicationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
