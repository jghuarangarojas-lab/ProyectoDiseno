namespace MsInventory.Infrastructure.Hubs;

public sealed class InventoryHub : Microsoft.AspNetCore.SignalR.Hub
{
}

public static class InventoryHubEvents
{
    public const string LowStockAlert = "LowStockAlert";
}
