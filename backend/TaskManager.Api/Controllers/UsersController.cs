using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.DTOs.Users;
using TaskManager.Api.Services;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<UserResponse>>> GetAll() =>
        Ok(await userService.GetAllAsync());

    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request)
    {
        var user = await userService.CreateAsync(request);
        return StatusCode(StatusCodes.Status201Created, user);
    }
}
