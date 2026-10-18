using Microsoft.AspNetCore.SignalR;

namespace Irrigation.Api.Hubs;

public sealed class IrrigationHub : Hub
{
    public const string DashboardGroup = "dashboard";

    public static string DeviceGroup(string deviceId)
    {
        return $"device:{deviceId.Trim().ToLowerInvariant()}";
    }

    public Task JoinDevice(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            throw new HubException("DeviceId es obligatorio para suscribirse a sus actualizaciones.");
        }

        return Groups.AddToGroupAsync(Context.ConnectionId, DeviceGroup(deviceId));
    }

    public Task LeaveDevice(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            throw new HubException("DeviceId es obligatorio para abandonar sus actualizaciones.");
        }

        return Groups.RemoveFromGroupAsync(Context.ConnectionId, DeviceGroup(deviceId));
    }

    public Task JoinDashboard()
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, DashboardGroup);
    }

    public Task LeaveDashboard()
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, DashboardGroup);
    }
}
