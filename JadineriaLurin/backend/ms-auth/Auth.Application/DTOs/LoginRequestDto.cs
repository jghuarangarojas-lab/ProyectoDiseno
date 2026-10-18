using System.ComponentModel.DataAnnotations;

namespace Auth.Application.DTOs;

public class LoginRequestDto
{
    [Required(ErrorMessage = "El username es obligatorio.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    public string Password { get; set; } = string.Empty;
}
