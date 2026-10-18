using Auth.Domain.Common;

namespace Auth.Domain.Entities;

public class PasswordRecoveryToken : BaseEntity
{
    public int UserId { get; private set; }
    public User User { get; private set; } = null!;

    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; private set; }
    public bool IsUsed { get; private set; }

    public bool IsValid => !IsUsed && ExpiresAtUtc > DateTime.UtcNow;

    private PasswordRecoveryToken()
    {
    }

    public static PasswordRecoveryToken Create(int userId, string token, DateTime expiresAtUtc)
    {
        return new PasswordRecoveryToken
        {
            UserId = userId,
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            IsUsed = false
        };
    }

    public void MarkAsUsed() => IsUsed = true;
}
