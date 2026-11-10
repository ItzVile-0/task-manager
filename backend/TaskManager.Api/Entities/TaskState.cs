namespace TaskManager.Api.Entities;

// no se llama TaskStatus para no chocar con System.Threading.Tasks.TaskStatus
public enum TaskState
{
    Pending,
    InProgress,
    Done
}
