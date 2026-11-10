using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.DTOs.Tasks;

public record CreateTaskRequest(
    [Required(ErrorMessage = "El título es obligatorio")]
    [MaxLength(200, ErrorMessage = "El título no puede superar 200 caracteres")]
    string Title,

    [MaxLength(1000, ErrorMessage = "La descripción no puede superar 1000 caracteres")]
    string? Description,

    [Range(1, int.MaxValue, ErrorMessage = "La tarea debe tener un usuario asignado")]
    int UserId,

    TaskInfoDto? AdditionalInfo);
