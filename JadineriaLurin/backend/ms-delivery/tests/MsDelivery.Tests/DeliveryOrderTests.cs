using System;
using MsDelivery.Domain.Entities;
using MsDelivery.Domain.Enums;
using Xunit;

namespace MsDelivery.Tests;

public class DeliveryOrderTests
{
    [Fact]
    public void DeliveryOrder_CreatedWithPendingStatus()
    {
        var order = new DeliveryOrder
        {
            Id = Guid.NewGuid(),
            OrderCode = ""ORD-001"",
            CustomerName = ""Test"",
            CustomerPhone = ""999999999"",
            DeliveryAddress = ""Address"",
            District = ""Lurin"",
            ItemsSummary = ""[]"",
            Status = DeliveryStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        Assert.Equal(DeliveryStatus.Pending, order.Status);
    }
}
