using System;
using Irrigation.Domain.Entities;
using Irrigation.Domain.Enums;
using Xunit;

namespace Irrigation.Tests;

public class IrrigationActivityTests
{
    [Fact]
    public void IrrigationActivity_DefaultStatus_IsPending()
    {
        var activity = new IrrigationActivity
        {
            Id = Guid.NewGuid(),
            SectorId = ""S1"",
            DurationMinutes = 10,
            TriggerType = TriggerType.Manual,
            RequestedAt = DateTime.UtcNow
        };
        Assert.Equal(IrrigationStatus.Pending, activity.Status);
    }
}
