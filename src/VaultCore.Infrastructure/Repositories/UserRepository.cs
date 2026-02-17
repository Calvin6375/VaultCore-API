using Microsoft.EntityFrameworkCore;
using VaultCore.Domain.Entities;
using VaultCore.Domain.Interfaces;
using VaultCore.Infrastructure.Data;

namespace VaultCore.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context) => _context = context;

    public async Task<User?> GetByIdAsync(Guid id, bool includeRoles = false, bool includeWallet = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Users.AsQueryable();
        if (includeRoles) query = query.Include(u => u.UserRoles).ThenInclude(ur => ur.Role);
        if (includeWallet) query = query.Include(u => u.Wallet);
        return await query.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, bool includeRoles = false, bool includeWallet = false, CancellationToken cancellationToken = default)
    {
        var normalized = email.Normalize().ToLowerInvariant();
        var query = _context.Users.AsQueryable();
        if (includeRoles) query = query.Include(u => u.UserRoles).ThenInclude(ur => ur.Role);
        if (includeWallet) query = query.Include(u => u.Wallet);
        return await query.FirstOrDefaultAsync(u => u.Email == normalized, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetPagedAsync(int page, int pageSize, string? search = null, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).AsQueryable();
        if (!includeDeleted) query = query.Where(u => !u.IsDeleted);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = $"%{search.Trim()}%";
            query = query.Where(u =>
                EF.Functions.ILike(u.Email, term) ||
                EF.Functions.ILike(u.FirstName, term) ||
                EF.Functions.ILike(u.LastName, term) ||
                (u.PhoneNumber != null && EF.Functions.ILike(u.PhoneNumber, term)));
        }
        return await query.OrderBy(u => u.CreatedAtUtc).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(string? search = null, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Users.AsQueryable();
        if (!includeDeleted) query = query.Where(u => !u.IsDeleted);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = $"%{search.Trim()}%";
            query = query.Where(u =>
                EF.Functions.ILike(u.Email, term) ||
                EF.Functions.ILike(u.FirstName, term) ||
                EF.Functions.ILike(u.LastName, term) ||
                (u.PhoneNumber != null && EF.Functions.ILike(u.PhoneNumber, term)));
        }
        return await query.CountAsync(cancellationToken);
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        return user;
    }

    public void Update(User user) => _context.Users.Update(user);
}
