using Auth.Domain.Common;
using Auth.Domain.Enums;

namespace Auth.Domain.Entities;

public class LoginHistory : BaseEntity
{
    public int UserId { get; private set; }
    public User User { get; private set; } = null!;

    public LoginStatus Status { get; private set; }
    public string? IpAddress { get; private set; }
    public DateTime TimestampUtc { get; private set; }

    private LoginHistory()
    {
    }

    private LoginHistory(int userId, LoginStatus status, string? ipAddress)
    {
        UserId = userId;
        Status = status;
        IpAddress = ipAddress;
        TimestampUtc = DateTime.UtcNow;
    }

    public static LoginHistory Successful(int userId, string? ipAddress) =>
        new(userId, LoginStatus.SUCCESS, ipAddress);

    public static LoginHistory Failed(int userId, string? ipAddress) =>
        new(userId, LoginStatus.FAILED, ipAddress);
}
