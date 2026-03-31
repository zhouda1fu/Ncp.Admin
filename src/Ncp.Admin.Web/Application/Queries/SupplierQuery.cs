using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.SupplierAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 供应商 DTO（用于下拉与列表）
/// </summary>
public record SupplierDto(SupplierId Id, string FullName, string ShortName, string Contact, string Phone);

/// <summary>
/// 供应商详情 DTO（用于编辑回填）
/// </summary>
public record SupplierDetailDto(SupplierId Id, string FullName, string ShortName, string Contact, string Phone, string Email, string Address, string Remark);

/// <summary>
/// 供应商查询
/// </summary>
public class SupplierQuery(ApplicationDbContext dbContext) : IQuery
{
    /// <summary>
    /// 按 ID 获取供应商详情
    /// </summary>
    public async Task<SupplierDetailDto?> GetByIdAsync(SupplierId id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Suppliers
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new SupplierDetailDto(s.Id, s.FullName, s.ShortName, s.Contact, s.Phone, s.Email, s.Address, s.Remark))
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// 获取供应商列表（用于产品表单下拉等）
    /// </summary>
    public async Task<IReadOnlyList<SupplierDto>> GetListAsync(string? keyword = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Suppliers.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var k = keyword.Trim();
            query = query.Where(s => s.FullName.Contains(k) || s.ShortName.Contains(k) || s.Contact.Contains(k));
        }
        return await query
            .OrderBy(s => s.FullName)
            .Select(s => new SupplierDto(s.Id, s.FullName, s.ShortName, s.Contact, s.Phone))
            .ToListAsync(cancellationToken);
    }
}
