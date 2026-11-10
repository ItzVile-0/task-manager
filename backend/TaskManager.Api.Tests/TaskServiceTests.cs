using TaskManager.Api.DTOs.Tasks;
using TaskManager.Api.Entities;
using TaskManager.Api.Exceptions;
using TaskManager.Api.Repositories;
using TaskManager.Api.Services;

namespace TaskManager.Api.Tests;

public class TaskServiceTests
{
    private readonly FakeTaskRepository _tasks = new();
    private readonly TaskService _service;

    public TaskServiceTests()
    {
        _service = new TaskService(_tasks, new FakeUserRepository(existingUserId: 1));
    }

    [Fact]
    public async Task No_permite_pasar_de_Pending_a_Done()
    {
        _tasks.Items.Add(NewTask(1, TaskState.Pending));

        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.ChangeStatusAsync(1, TaskState.Done));
        Assert.Equal(TaskState.Pending, _tasks.Items[0].Status);
    }

    [Theory]
    [InlineData(TaskState.Pending, TaskState.InProgress)]
    [InlineData(TaskState.InProgress, TaskState.Done)]
    [InlineData(TaskState.Done, TaskState.Pending)]
    public async Task Permite_las_demas_transiciones(TaskState from, TaskState to)
    {
        _tasks.Items.Add(NewTask(1, from));

        var result = await _service.ChangeStatusAsync(1, to);

        Assert.Equal(to, result.Status);
        Assert.NotNull(result.UpdatedAt);
    }

    [Fact]
    public async Task Cambiar_estado_de_tarea_inexistente_lanza_NotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.ChangeStatusAsync(99, TaskState.InProgress));
    }

    [Fact]
    public async Task No_crea_tarea_si_el_usuario_no_existe()
    {
        var request = new CreateTaskRequest("Tarea", null, 50, null);

        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CreateAsync(request));
        Assert.Empty(_tasks.Items);
    }

    [Fact]
    public async Task Crea_tarea_en_Pending_y_guarda_la_info_como_json()
    {
        var request = new CreateTaskRequest("  Tarea  ", null, 1, new TaskInfoDto("High", null, ["infra"]));

        var result = await _service.CreateAsync(request);

        Assert.Equal("Tarea", result.Title);
        Assert.Equal(TaskState.Pending, result.Status);
        Assert.Equal("{\"priority\":\"High\",\"tags\":[\"infra\"]}", _tasks.Items[0].AdditionalInfo);
    }

    private static TaskItem NewTask(int id, TaskState status) => new()
    {
        Id = id,
        Title = "Prueba",
        Status = status,
        UserId = 1,
        User = new User { Id = 1, Name = "Ana" }
    };
}

internal class FakeTaskRepository : ITaskRepository
{
    public List<TaskItem> Items { get; } = [];

    public Task<List<TaskItem>> GetAllAsync(int? userId, TaskState? status, string? priority) =>
        Task.FromResult(Items.ToList());

    public Task<TaskItem?> GetByIdAsync(int id) =>
        Task.FromResult(Items.FirstOrDefault(t => t.Id == id));

    public Task AddAsync(TaskItem task)
    {
        Items.Add(task);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync() => Task.CompletedTask;
}

internal class FakeUserRepository(int existingUserId) : IUserRepository
{
    public Task<List<User>> GetAllAsync() => Task.FromResult(new List<User>());
    public Task<bool> ExistsAsync(int id) => Task.FromResult(id == existingUserId);
    public Task<bool> EmailExistsAsync(string email) => Task.FromResult(false);
    public Task AddAsync(User user) => Task.CompletedTask;
}
