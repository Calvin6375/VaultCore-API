using AutoMapper;
using VaultCore.Application.DTOs;
using VaultCore.Domain.Interfaces;

namespace VaultCore.Application.Services;

/// <summary>
/// Read-only audit log queries (admin).
/// </summary>
public class AuditQueryService : IAuditQueryService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public AuditQueryService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PagedResult<AuditLogDto>> GetPagedAsync(int page, int pageSize, Guid? userId, string? action, CancellationToken cancellationToken = default)
    {
        var total = await _uow.AuditLogs.CountAsync(userId, action, cancellationToken);
        var list = await _uow.AuditLogs.GetPagedAsync(page, pageSize, userId, action, cancellationToken);
        return new PagedResult<AuditLogDto>(list.Select(_mapper.Map<AuditLogDto>).ToList(), total, page, pageSize);
    }
}
