using TaskManager.Api.Entities;

namespace TaskManager.Api.Repositories;

public interface ITaskRepository
{
    Task<List<TaskItem>> GetAllAsync(int? userId, TaskState? status, string? priority);
    Task<TaskItem?> GetByIdAsync(int id);
    Task AddAsync(TaskItem task);
    Task SaveChangesAsync();
}
