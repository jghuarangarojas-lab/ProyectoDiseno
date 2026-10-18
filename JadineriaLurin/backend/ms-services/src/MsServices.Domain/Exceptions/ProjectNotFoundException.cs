namespace MsServices.Domain.Exceptions;

public class ProjectNotFoundException : DomainException
{
    public ProjectNotFoundException(Guid projectId)
        : base($"El proyecto {projectId} no existe o fue eliminado.")
    {
    }
}
