using Microsoft.EntityFrameworkCore;
using VaultCore.Domain.Entities;
using VaultCore.Domain.Enums;
using VaultCore.Domain.Interfaces;
using VaultCore.Infrastructure.Data;

namespace VaultCore.Infrastructure.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly ApplicationDbContext _context;

    public RoleRepository(ApplicationDbContext context) => _context = context;

    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Roles.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<Role?> GetByNameAsync(RoleType name, CancellationToken cancellationToken = default) =>
        await _context.Roles.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Name == name, cancellationToken);

    public async Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Roles.IgnoreQueryFilters().ToListAsync(cancellationToken);
}
