using TaskManager.Api.DTOs.Tasks;
using TaskManager.Api.Entities;

namespace TaskManager.Api.Services;

public interface ITaskService
{
    Task<List<TaskResponse>> GetAllAsync(int? userId, TaskState? status, string? priority);
    Task<TaskResponse> GetByIdAsync(int id);
    Task<TaskResponse> CreateAsync(CreateTaskRequest request);
    Task<TaskResponse> ChangeStatusAsync(int id, TaskState newStatus);
}
