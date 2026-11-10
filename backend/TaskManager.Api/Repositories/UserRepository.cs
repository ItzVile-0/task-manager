using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.Entities;

namespace TaskManager.Api.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<List<User>> GetAllAsync() =>
        db.Users.AsNoTracking().OrderBy(u => u.Name).ToListAsync();

    public Task<bool> ExistsAsync(int id) =>
        db.Users.AnyAsync(u => u.Id == id);

    public Task<bool> EmailExistsAsync(string email) =>
        db.Users.AnyAsync(u => u.Email == email);

    public async Task AddAsync(User user)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }
}
