using System.Security.Cryptography;
using Auth.Application.Common.Interfaces;
using Auth.Application.Common.Models;
using Auth.Application.DTOs;
using Auth.Domain.Entities;
using Auth.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Auth.Application.Services;

public class AuthService : IAuthService
{
    private const int RecoveryTokenValidityMinutes = 15;

    private readonly IAuthDbContext _dbContext;
    private readonly IJwtProvider _jwtProvider;
    private readonly IEmailSender _emailSender;
    private readonly FrontendSettings _frontendSettings;

    public AuthService(
        IAuthDbContext dbContext,
        IJwtProvider jwtProvider,
        IEmailSender emailSender,
        IOptions<FrontendSettings> frontendSettings)
    {
        _dbContext = dbContext;
        _jwtProvider = jwtProvider;
        _emailSender = emailSender;
        _frontendSettings = frontendSettings.Value;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var normalizedUsername = User.NormalizeUsername(request.Username);

        var user = await _dbContext.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == normalizedUsername, cancellationToken);

        var passwordIsValid = user is not null
            && user.IsActive
            && BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!passwordIsValid)
        {
            if (user is not null && user.IsActive)
            {
                user.IncrementFailedAttempts();
                _dbContext.LoginHistories.Add(LoginHistory.Failed(user.Id, ipAddress));
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            throw new UnauthorizedAccessException("Usuario o contraseña incorrectos.");
        }

        user!.ResetFailedAttempts();
        _dbContext.LoginHistories.Add(LoginHistory.Successful(user.Id, ipAddress));

        var jwt = _jwtProvider.GenerateToken(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new LoginResponseDto
        {
            Token = jwt.Token,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role!.Name,
            ExpiresAtUtc = jwt.ExpiresAtUtc
        };
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail && u.IsActive, cancellationToken);

        // Por seguridad no se revela si el correo existe o no.
        if (user is null)
        {
            return;
        }

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64)).ToLowerInvariant();
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(RecoveryTokenValidityMinutes);

        _dbContext.PasswordRecoveryTokens.Add(PasswordRecoveryToken.Create(user.Id, token, expiresAtUtc));
        await _dbContext.SaveChangesAsync(cancellationToken);

        var resetUrl = $"{_frontendSettings.ResetPasswordUrl}?token={token}";

        var body = $"""
            <h2>Recuperaci&oacute;n de contrase&ntilde;a</h2>
            <p>Hola <b>{user.Username}</b>, recibimos una solicitud para restablecer tu contrase&ntilde;a.</p>
            <p>Haz clic en el siguiente enlace para elegir una nueva contrase&ntilde;a:</p>
            <p><a href="{resetUrl}">Restablecer mi contrase&ntilde;a</a></p>
            <p>O copia este c&oacute;digo manualmente en la pantalla de restablecimiento:</p>
            <p style="font-size:20px;font-weight:bold;letter-spacing:2px;">{token}</p>
            <p>Este enlace vence en {RecoveryTokenValidityMinutes} minutos. Si no solicitaste este cambio, ignora este correo.</p>
            """;

        await _emailSender.SendAsync(
            user.Email,
            "Recuperación de contraseña - Jardinería Lurín",
            body,
            cancellationToken);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
        {
            throw new DomainException("La nueva contraseña debe tener al menos 8 caracteres.");
        }

        var recovery = await _dbContext.PasswordRecoveryTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == request.Token, cancellationToken);

        if (recovery is null || !recovery.IsValid)
        {
            throw new DomainException("El token de recuperación es inválido o ha expirado.");
        }

        recovery.MarkAsUsed();
        recovery.User.ChangePassword(BCrypt.Net.BCrypt.HashPassword(request.NewPassword));
        recovery.User.ResetFailedAttempts();

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
