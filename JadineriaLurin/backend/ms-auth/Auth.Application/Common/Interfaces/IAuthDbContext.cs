using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application.Common.Interfaces;

public interface IAuthDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<LoginHistory> LoginHistories { get; }
    DbSet<PasswordRecoveryToken> PasswordRecoveryTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
