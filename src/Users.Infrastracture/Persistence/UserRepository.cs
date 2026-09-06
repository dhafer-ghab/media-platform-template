using Users.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SharedKernal.Messaging;
using SharedKernal.Results;
using Users.Domain;

namespace Users.Infrastracture.Persistence;

internal class UserRepository(UsersDbContext context, IDomainEventDispatcher dispatcher) : IUserRepository
{
    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await context.Users.AddAsync(user, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Users.ToListAsync(cancellationToken);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {

        return await context.Users.FindAsync([id], cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        email = email.Trim().ToLowerInvariant();

        return await context.Users
            .FirstOrDefaultAsync(u => u.Email.Value == email, cancellationToken);
    }

    public void Remove(User user)
    {
        context.Users.Remove(user);
    }

    public async Task<Result<int>> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var aggregates = context.ChangeTracker
            .Entries<User>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = aggregates
            .SelectMany(a => a.DomainEvents)
            .ToList();

        aggregates.ForEach(a => a.ClearDomainEvents());

        int result;
        try
        {
            result = await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "UX_Users_Email"
            })
        {
            context.ChangeTracker.Clear();
            return Error.Conflict("User.EmailExists", "A user with that email already exists.");
        }

        await dispatcher.DispatchAsync(domainEvents, cancellationToken);

        return result;
    }

    public void Update(User user)
    {
        context.Users.Update(user);
    }
}
