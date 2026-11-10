using TaskManager.Api.Entities;

namespace TaskManager.Api.DTOs.Tasks;

public record TaskResponse(
    int Id,
    string Title,
    string? Description,
    TaskState Status,
    int UserId,
    string UserName,
    TaskInfoDto? AdditionalInfo,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
