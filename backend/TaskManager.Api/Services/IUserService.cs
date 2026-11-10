using TaskManager.Api.DTOs.Users;

namespace TaskManager.Api.Services;

public interface IUserService
{
    Task<List<UserResponse>> GetAllAsync();
    Task<UserResponse> CreateAsync(CreateUserRequest request);
}
