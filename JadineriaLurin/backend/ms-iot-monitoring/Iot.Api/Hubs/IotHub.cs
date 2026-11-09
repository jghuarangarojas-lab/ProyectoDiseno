using Microsoft.AspNetCore.SignalR;

namespace Iot.Api.Hubs;

public sealed class IotHub : Hub
{
    public const string DashboardGroup = "dashboard";

    public static string DeviceGroup(string deviceCode)
    {
        return $"device:{deviceCode.Trim().ToLowerInvariant()}";
    }

    public Task JoinDevice(string deviceCode)
    {
        if (string.IsNullOrWhiteSpace(deviceCode))
        {
            throw new HubException("DeviceCode es obligatorio para suscribirse a sus actualizaciones.");
        }

        return Groups.AddToGroupAsync(Context.ConnectionId, DeviceGroup(deviceCode));
    }

    public Task LeaveDevice(string deviceCode)
    {
        if (string.IsNullOrWhiteSpace(deviceCode))
        {
            throw new HubException("DeviceCode es obligatorio para abandonar sus actualizaciones.");
        }

        return Groups.RemoveFromGroupAsync(Context.ConnectionId, DeviceGroup(deviceCode));
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
