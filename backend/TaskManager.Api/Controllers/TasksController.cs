using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.DTOs.Tasks;
using TaskManager.Api.Entities;
using TaskManager.Api.Services;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/tasks")]
public class TasksController(ITaskService taskService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<TaskResponse>>> GetAll(
        [FromQuery] int? userId,
        [FromQuery] TaskState? status,
        [FromQuery] string? priority) =>
        Ok(await taskService.GetAllAsync(userId, status, priority));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskResponse>> GetById(int id) =>
        Ok(await taskService.GetByIdAsync(id));

    [HttpPost]
    public async Task<ActionResult<TaskResponse>> Create(CreateTaskRequest request)
    {
        var task = await taskService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<TaskResponse>> UpdateStatus(int id, UpdateTaskStatusRequest request) =>
        Ok(await taskService.ChangeStatusAsync(id, request.Status!.Value));
}
