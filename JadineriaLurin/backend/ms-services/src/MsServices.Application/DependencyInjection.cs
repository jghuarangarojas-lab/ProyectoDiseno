using Microsoft.Extensions.DependencyInjection;
using MsServices.Application.Interfaces.Services;
using MsServices.Application.Services;

namespace MsServices.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        return services;
    }
}
