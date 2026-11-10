using TaskManager.Api.DTOs.Users;
using TaskManager.Api.Entities;
using TaskManager.Api.Exceptions;
using TaskManager.Api.Repositories;

namespace TaskManager.Api.Services;

public class UserService(IUserRepository users) : IUserService
{
    public async Task<List<UserResponse>> GetAllAsync()
    {
        var list = await users.GetAllAsync();
        return list.Select(ToResponse).ToList();
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        // shortcut: dos requests simultaneos con el mismo correo pasarian este chequeo y el segundo
        // terminaria en 500 por el UNIQUE de la bd. Si hiciera falta, capturar el error 2627 y devolver 409.
        if (await users.EmailExistsAsync(email))
            throw new ConflictException($"Ya existe un usuario con el correo {email}");

        var user = new User { Name = request.Name.Trim(), Email = email };
        await users.AddAsync(user);

        return ToResponse(user);
    }

    // la bd guarda utc, se marca para que el json salga con Z y el front lo convierta a hora local
    private static UserResponse ToResponse(User u) =>
        new(u.Id, u.Name, u.Email, DateTime.SpecifyKind(u.CreatedAt, DateTimeKind.Utc));
}
