using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.Entities;

namespace TaskManager.Api.Repositories;

public class TaskRepository(AppDbContext db) : ITaskRepository
{
    public async Task<List<TaskItem>> GetAllAsync(int? userId, TaskState? status, string? priority)
    {
        var query = db.Tasks.AsNoTracking().Include(t => t.User).AsQueryable();

        if (userId.HasValue)
            query = query.Where(t => t.UserId == userId.Value);

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        // Priority es la columna calculada con JSON_VALUE, usa el indice IX_Tasks_Priority
        if (!string.IsNullOrWhiteSpace(priority))
            query = query.Where(t => t.Priority == priority);

        return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
    }

    public Task<TaskItem?> GetByIdAsync(int id) =>
        db.Tasks.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == id);

    public async Task AddAsync(TaskItem task)
    {
        db.Tasks.Add(task);
        await db.SaveChangesAsync();
        await db.Entry(task).Reference(t => t.User).LoadAsync();
    }

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
