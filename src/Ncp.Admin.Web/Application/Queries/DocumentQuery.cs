using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.DocumentAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 文档版本查询 DTO
/// </summary>
public record DocumentVersionQueryDto(
    Guid Id,
    int VersionNumber,
    string FileName,
    long FileSize,
    DateTimeOffset CreatedAt);

/// <summary>
/// 文档查询 DTO
/// </summary>
public record DocumentQueryDto(
    DocumentId Id,
    string Title,
    UserId CreatorId,
    DateTimeOffset CreatedAt,
    int VersionCount,
    DocumentVersionQueryDto? CurrentVersion);

/// <summary>
/// 文档分页查询入参
/// </summary>
public class DocumentQueryInput : PageRequest
{
    /// <summary>
    /// 标题关键字
    /// </summary>
    public string? Title { get; set; }
}

/// <summary>
/// 文档查询服务
/// </summary>
public class DocumentQuery(ApplicationDbContext dbContext) : IQuery
{
    /// <summary>
    /// 按 ID 查询文档（含版本列表）
    /// </summary>
    public async Task<DocumentQueryDto?> GetByIdAsync(DocumentId id, CancellationToken cancellationToken = default)
    {
        var doc = await dbContext.Documents
            .AsNoTracking()
            .Include(x => x.Versions)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (doc == null) return null;
        var current = doc.CurrentVersion;
        return new DocumentQueryDto(
            doc.Id,
            doc.Title,
            doc.CreatorId,
            doc.CreatedAt,
            doc.Versions.Count,
            current == null
                ? null
                : new DocumentVersionQueryDto(
                    current.Id.Id,
                    current.VersionNumber,
                    current.FileName,
                    current.FileSize,
                    current.CreatedAt));
    }

    /// <summary>
    /// 分页查询文档
    /// </summary>
    public async Task<PagedData<DocumentQueryDto>> GetPagedAsync(
        DocumentQueryInput input,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Document> query = dbContext.Documents.AsNoTracking().Include(x => x.Versions);
        if (!string.IsNullOrWhiteSpace(input.Title))
            query = query.Where(d => d.Title.Contains(input.Title.Trim()));
        return await query
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new DocumentQueryDto(
                d.Id,
                d.Title,
                d.CreatorId,
                d.CreatedAt,
                d.Versions.Count,
                d.Versions.OrderByDescending(v => v.VersionNumber).Select(v => new DocumentVersionQueryDto(
                    v.Id.Id,
                    v.VersionNumber,
                    v.FileName,
                    v.FileSize,
                    v.CreatedAt)).FirstOrDefault()))
            .ToPagedDataAsync(input, cancellationToken);
    }

    /// <summary>
    /// 按版本 ID 获取存储键与文件名（用于下载）
    /// </summary>
    public async Task<(string FileStorageKey, string FileName)?> GetVersionDownloadInfoAsync(
        DocumentVersionId versionId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.DocumentVersions
            .AsNoTracking()
            .Where(v => v.Id == versionId)
            .Select(v => new { v.FileStorageKey, v.FileName })
            .FirstOrDefaultAsync(cancellationToken) is { } x
            ? (x.FileStorageKey, x.FileName)
            : null;
    }
}
