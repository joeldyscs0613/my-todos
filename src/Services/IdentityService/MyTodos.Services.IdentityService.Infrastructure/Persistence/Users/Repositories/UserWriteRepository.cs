using MyTodos.Services.IdentityService.Application.Users.Contracts;
using MyTodos.Services.IdentityService.Domain.UserAggregate;
using MyTodos.Services.IdentityService.Infrastructure.Persistence;

namespace MyTodos.Services.IdentityService.Infrastructure.UserAggregate.Repositories;

/// <summary>
/// Write repository for User aggregate mutations.
/// </summary>
public sealed class UserWriteRepository : IUserWriteRepository
{
    private readonly IdentityServiceDbContext _context;

    public UserWriteRepository(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await _context.Users.AddAsync(user, ct);
    }

    public Task UpdateAsync(User user, CancellationToken ct = default)
    {
        _context.Users.Update(user);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(User user, CancellationToken ct = default)
    {
        _context.Users.Remove(user);
        return Task.CompletedTask;
    }
}
