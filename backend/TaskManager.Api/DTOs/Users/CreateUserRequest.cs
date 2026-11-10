using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.DTOs.Users;

public record CreateUserRequest(
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(100, ErrorMessage = "El nombre no puede superar 100 caracteres")]
    string Name,

    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido")]
    [MaxLength(150, ErrorMessage = "El correo no puede superar 150 caracteres")]
    string Email);
