using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.DTOs.Tasks;

// lo que se guarda como json en Tasks.AdditionalInfo
public record TaskInfoDto(
    [RegularExpression("Low|Medium|High", ErrorMessage = "La prioridad debe ser Low, Medium o High")]
    string? Priority,
    DateOnly? EstimatedEndDate,
    List<string>? Tags);
