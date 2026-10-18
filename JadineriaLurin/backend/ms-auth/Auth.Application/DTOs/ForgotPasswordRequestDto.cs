using System.ComponentModel.DataAnnotations;

namespace Auth.Application.DTOs;

public class ForgotPasswordRequestDto
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    public string Email { get; set; } = string.Empty;
}
