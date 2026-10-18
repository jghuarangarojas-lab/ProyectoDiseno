namespace Irrigation.Application.Exceptions;

public sealed class IrrigationNotFoundException : IrrigationApplicationException
{
    public IrrigationNotFoundException(string message)
        : base(message)
    {
    }
}
