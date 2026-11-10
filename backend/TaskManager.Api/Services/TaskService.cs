using System.Text.Json;
using System.Text.Json.Serialization;
using TaskManager.Api.DTOs.Tasks;
using TaskManager.Api.Entities;
using TaskManager.Api.Exceptions;
using TaskManager.Api.Repositories;

namespace TaskManager.Api.Services;

public class TaskService(ITaskRepository tasks, IUserRepository users) : ITaskService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<List<TaskResponse>> GetAllAsync(int? userId, TaskState? status, string? priority)
    {
        var list = await tasks.GetAllAsync(userId, status, priority);
        return list.Select(ToResponse).ToList();
    }

    public async Task<TaskResponse> GetByIdAsync(int id)
    {
        var task = await tasks.GetByIdAsync(id)
            ?? throw new NotFoundException($"No existe la tarea {id}");

        return ToResponse(task);
    }

    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request)
    {
        if (!await users.ExistsAsync(request.UserId))
            throw new BusinessRuleException($"El usuario {request.UserId} no existe");

        var task = new TaskItem
        {
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            UserId = request.UserId,
            AdditionalInfo = request.AdditionalInfo is null
                ? null
                : JsonSerializer.Serialize(request.AdditionalInfo, JsonOptions)
        };

        await tasks.AddAsync(task);
        return ToResponse(task);
    }

    public async Task<TaskResponse> ChangeStatusAsync(int id, TaskState newStatus)
    {
        var task = await tasks.GetByIdAsync(id)
            ?? throw new NotFoundException($"No existe la tarea {id}");

        if (task.Status == TaskState.Pending && newStatus == TaskState.Done)
            throw new BusinessRuleException("Una tarea en Pending no puede pasar directo a Done, primero debe estar InProgress");

        if (task.Status != newStatus)
        {
            task.Status = newStatus;
            task.UpdatedAt = DateTime.UtcNow;
            await tasks.SaveChangesAsync();
        }

        return ToResponse(task);
    }

    private static TaskResponse ToResponse(TaskItem t) => new(
        t.Id,
        t.Title,
        t.Description,
        t.Status,
        t.UserId,
        t.User?.Name ?? string.Empty,
        t.AdditionalInfo is null ? null : JsonSerializer.Deserialize<TaskInfoDto>(t.AdditionalInfo, JsonOptions),
        DateTime.SpecifyKind(t.CreatedAt, DateTimeKind.Utc),
        t.UpdatedAt is null ? null : DateTime.SpecifyKind(t.UpdatedAt.Value, DateTimeKind.Utc));
}
