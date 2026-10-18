namespace Auth.Application.Common.Models;

public class FrontendSettings
{
    public const string SectionName = "Frontend";

    /// <summary>URL base de la pantalla de restablecimiento en el frontend.</summary>
    public string ResetPasswordUrl { get; set; } = "http://localhost:5173/reset-password";
}
