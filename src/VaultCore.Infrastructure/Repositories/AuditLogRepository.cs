using Microsoft.EntityFrameworkCore;
using VaultCore.Domain.Entities;
using VaultCore.Domain.Interfaces;
using VaultCore.Infrastructure.Data;

namespace VaultCore.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly ApplicationDbContext _context;

    public AuditLogRepository(ApplicationDbContext context) => _context = context;

    public async Task<AuditLog> AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        await _context.AuditLogs.AddAsync(auditLog, cancellationToken);
        return auditLog;
    }

    public async Task<IReadOnlyList<AuditLog>> GetPagedAsync(int page, int pageSize, Guid? userId = null, string? action = null, CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.AsQueryable();
        if (userId.HasValue) query = query.Where(a => a.UserId == userId);
        if (!string.IsNullOrWhiteSpace(action)) query = query.Where(a => a.Action == action);
        return await query.OrderByDescending(a => a.TimestampUtc).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(Guid? userId = null, string? action = null, CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.AsQueryable();
        if (userId.HasValue) query = query.Where(a => a.UserId == userId);
        if (!string.IsNullOrWhiteSpace(action)) query = query.Where(a => a.Action == action);
        return await query.CountAsync(cancellationToken);
    }
}
