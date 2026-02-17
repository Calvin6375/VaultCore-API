namespace VaultCore.Application.DTOs;

/// <summary>
/// Paged list result.
/// </summary>
/// <param name="Items">Page items.</param>
/// <param name="TotalCount">Total count of items.</param>
/// <param name="Page">Current page (1-based).</param>
/// <param name="PageSize">Page size.</param>
public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);
