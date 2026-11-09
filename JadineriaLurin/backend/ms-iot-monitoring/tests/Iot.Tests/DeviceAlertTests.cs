using System;
using Iot.Domain.Entities;
using Iot.Domain.Enums;
using Xunit;

namespace Iot.Tests;

public class DeviceAlertTests
{
    [Fact]
    public void DeviceAlert_CanBeCreated()
    {
        var alert = new DeviceAlert
        {
            Id = Guid.NewGuid(),
            IotDeviceId = Guid.NewGuid(),
            IssueType = AlertIssueType.ConnectionLost,
            Message = ""Sin conexion"",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        Assert.False(alert.IsRead);
    }
}
