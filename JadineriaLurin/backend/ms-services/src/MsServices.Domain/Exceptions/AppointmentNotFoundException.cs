namespace MsServices.Domain.Exceptions;

public class AppointmentNotFoundException : DomainException
{
    public AppointmentNotFoundException(Guid appointmentId)
        : base($"La cita {appointmentId} no existe o fue eliminada.")
    {
    }
}
