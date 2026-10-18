using System.Text.RegularExpressions;
using Auth.Domain.Common;
using Auth.Domain.Exceptions;

namespace Auth.Domain.Entities;

public class User : BaseEntity
{
    // Formato requerido: inicial del nombre + apellido + "@JL" + ROL
    // Ejemplo: jzapata@JLGARDENER (se acepta cualquier combinación de mayúsculas/minúsculas)
    private static readonly Regex UsernameRegex = new(
        @"^[a-z]+@JL(ADMIN|BOTANIST|GARDENER|CASHIER|WAREHOUSE|DELIVERY)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Username { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public int FailedAttempts { get; private set; }

    public int RoleId { get; private set; }
    public Role Role { get; private set; } = null!;

    public ICollection<LoginHistory> LoginHistories { get; private set; } = new List<LoginHistory>();
    public ICollection<PasswordRecoveryToken> PasswordRecoveryTokens { get; private set; } = new List<PasswordRecoveryToken>();

    private User()
    {
    }

    /// <summary>
    /// Forma canónica: nombre/apellido en minúsculas, "@JL" + ROL en mayúsculas.
    /// Ejemplo: JHuaranga@JLadmin -> jhuaranga@JLADMIN
    /// </summary>
    public static string NormalizeUsername(string username)
    {
        var normalized = username.Trim();
        var separatorIndex = normalized.IndexOf("@JL", StringComparison.OrdinalIgnoreCase);

        if (separatorIndex < 0)
        {
            return normalized.ToLowerInvariant();
        }

        return string.Concat(
            normalized[..separatorIndex].ToLowerInvariant(),
            "@JL",
            normalized[(separatorIndex + 3)..].ToUpperInvariant());
    }

    public static User Create(string username, string passwordHash, string email, string roleName)
    {
        var normalizedUsername = username?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedUsername) || !UsernameRegex.IsMatch(normalizedUsername))
        {
            throw new DomainException(
                $"El username '{username}' no cumple el formato requerido (inicial + apellido + @JL + ROL). Ejemplo: jzapata@JLGARDENER.");
        }

        if (string.IsNullOrWhiteSpace(roleName) || !Role.Names.All.Contains(roleName))
        {
            throw new DomainException($"El rol '{roleName}' no es válido.");
        }

        if (!normalizedUsername.EndsWith($"@JL{roleName}", StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException("El rol embebido en el username no coincide con el rol asignado.");
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException("El hash de la contraseña es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            throw new DomainException($"El correo '{email}' no es válido.");
        }

        return new User
        {
            Username = NormalizeUsername(normalizedUsername),
            PasswordHash = passwordHash,
            Email = email.Trim().ToLowerInvariant(),
            IsActive = true,
            FailedAttempts = 0
        };
    }

    public void IncrementFailedAttempts() => FailedAttempts++;

    public void ResetFailedAttempts() => FailedAttempts = 0;

    public void ChangePassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException("El hash de la contraseña es obligatorio.");
        }

        PasswordHash = passwordHash;
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;
}
