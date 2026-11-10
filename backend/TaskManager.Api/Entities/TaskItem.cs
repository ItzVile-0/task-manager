namespace TaskManager.Api.Entities;

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskState Status { get; set; } = TaskState.Pending;
    public int UserId { get; set; }
    public User? User { get; set; }
    public string? AdditionalInfo { get; set; }

    // columna calculada en la bd a partir del json, solo lectura
    public string? Priority { get; private set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
