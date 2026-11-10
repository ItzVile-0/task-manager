using TaskManager.Api.Entities;

namespace TaskManager.Api.Repositories;

public interface IUserRepository
{
    Task<List<User>> GetAllAsync();
    Task<bool> ExistsAsync(int id);
    Task<bool> EmailExistsAsync(string email);
    Task AddAsync(User user);
}
