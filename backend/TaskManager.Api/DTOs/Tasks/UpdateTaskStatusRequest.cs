using System.ComponentModel.DataAnnotations;
using TaskManager.Api.Entities;

namespace TaskManager.Api.DTOs.Tasks;

public record UpdateTaskStatusRequest(
    [Required(ErrorMessage = "El estado es obligatorio")]
    TaskState? Status);
