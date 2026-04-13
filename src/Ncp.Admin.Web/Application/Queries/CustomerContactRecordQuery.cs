using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.IndustryAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

public record CustomerContactRecordListItemDto(
    CustomerContactRecordId Id,
    CustomerId CustomerId,
    string CustomerName,
    IReadOnlyList<string> IndustryNames,
    UserId OwnerId,
    string OwnerName,
    string OwnerDeptName,
    string RegionName,
    CustomerContactRecordType RecordType,
    DateTimeOffset RecordAt,
    DateTimeOffset? NextVisitAt,
    CustomerContactRecordStatus Status,
    string Title,
    string Content,
    IReadOnlyList<CustomerContactId> CustomerContactIds,
    string Remark,
    int ReminderIntervalDays,
    int ReminderCount,
    string FilePath,
    string CustomerAddress,
    string VisitAddress,
    UserId CreatorId,
    DateTimeOffset CreatedAt,
    UserId ModifierId,
    DateTimeOffset ModifiedAt);

public class CustomerContactRecordQueryInput : PageRequest
{
    public string? Keyword { get; set; }
    /// <summary>联系类型：1电话 2出差 3微信 4其他</summary>
    public int? RecordTypeId { get; set; }
    /// <summary>状态：0待选择 1有效 2无效</summary>
    public int? StatusId { get; set; }
    /// <summary>按本条联络负责人过滤</summary>
    public UserId? OwnerId { get; set; }
    public DateTimeOffset? RecordAtFrom { get; set; }
    public DateTimeOffset? RecordAtTo { get; set; }
    public DateTimeOffset? NextVisitAtFrom { get; set; }
    public DateTimeOffset? NextVisitAtTo { get; set; }
}

public class CustomerContactRecordQuery(ApplicationDbContext dbContext) : IQuery
{
    public async Task<PagedData<CustomerContactRecordListItemDto>> GetPagedAsync(
        CustomerContactRecordQueryInput input,
        CancellationToken cancellationToken)
    {
        var records = dbContext.CustomerContactRecords.AsNoTracking();
        var customers = dbContext.Customers.AsNoTracking();

        var query =
            from r in records
            join c in customers on r.CustomerId equals c.Id
            select new
            {
                Record = r,
                Customer = c
            };

        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            var kw = input.Keyword.Trim();
            query = query.Where(x =>
                x.Customer.FullName.Contains(kw)
                || (x.Customer.ShortName != null && x.Customer.ShortName.Contains(kw))
                || x.Record.OwnerName.Contains(kw));
        }

        if (input.RecordTypeId.HasValue)
        {
            var v = (CustomerContactRecordType)input.RecordTypeId.Value;
            query = query.Where(x => x.Record.RecordType == v);
        }

        if (input.StatusId.HasValue)
        {
            var s = (CustomerContactRecordStatus)input.StatusId.Value;
            query = query.Where(x => x.Record.Status == s);
        }

        if (input.OwnerId != null)
        {
            query = query.Where(x => x.Record.OwnerId == input.OwnerId);
        }

        if (input.RecordAtFrom.HasValue)
            query = query.Where(x => x.Record.RecordAt >= input.RecordAtFrom.Value);
        if (input.RecordAtTo.HasValue)
            query = query.Where(x => x.Record.RecordAt <= input.RecordAtTo.Value);

        if (input.NextVisitAtFrom.HasValue)
            query = query.Where(x => x.Record.NextVisitAt != null && x.Record.NextVisitAt >= input.NextVisitAtFrom.Value);
        if (input.NextVisitAtTo.HasValue)
            query = query.Where(x => x.Record.NextVisitAt != null && x.Record.NextVisitAt <= input.NextVisitAtTo.Value);

        var page = await query
            .OrderByDescending(x => x.Record.RecordAt)
            .Select(x => new
            {
                x.Record.Id,
                x.Record.CustomerId,
                CustomerName = x.Customer.FullName,
                x.Record.OwnerId,
                x.Record.OwnerName,
                x.Record.OwnerDeptName,
                RegionName = (x.Customer.ProvinceName ?? "")
                             + (string.IsNullOrEmpty(x.Customer.CityName) ? "" : $"/{x.Customer.CityName}")
                             + (string.IsNullOrEmpty(x.Customer.DistrictName) ? "" : $"/{x.Customer.DistrictName}"),
                x.Record.RecordType,
                x.Record.RecordAt,
                x.Record.NextVisitAt,
                x.Record.Status,
                x.Record.Title,
                x.Record.Content,
                CustomerContactIds = x.Record.Contacts.Select(c => c.ContactId).ToList(),
                x.Record.Remark,
                x.Record.ReminderIntervalDays,
                x.Record.ReminderCount,
                x.Record.FilePath,
                x.Record.CustomerAddress,
                x.Record.VisitAddress,
                x.Record.CreatorId,
                x.Record.CreatedAt,
                x.Record.ModifierId,
                x.Record.ModifiedAt,
                IndustryIds = x.Customer.Industries.Select(i => i.IndustryId).ToList()
            })
            .ToPagedDataAsync(input, cancellationToken);

        var allIndustryIds = page.Items.SelectMany(x => x.IndustryIds).Distinct().ToList();
        var industryMap = allIndustryIds.Count == 0
            ? new Dictionary<IndustryId, string>()
            : await dbContext.Industries.AsNoTracking()
                .Where(i => allIndustryIds.Contains(i.Id))
                .Select(i => new { i.Id, i.Name })
                .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var items = page.Items.Select(x =>
        {
            var names = x.IndustryIds
                .Select(id => industryMap.TryGetValue(id, out var n) ? n : string.Empty)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct()
                .ToList();
            return new CustomerContactRecordListItemDto(
                x.Id,
                x.CustomerId,
                x.CustomerName,
                names,
                x.OwnerId,
                x.OwnerName ?? string.Empty,
                x.OwnerDeptName ?? string.Empty,
                x.RegionName,
                x.RecordType,
                x.RecordAt,
                x.NextVisitAt,
                x.Status,
                x.Title ?? string.Empty,
                x.Content ?? string.Empty,
                x.CustomerContactIds,
                x.Remark ?? string.Empty,
                x.ReminderIntervalDays,
                x.ReminderCount,
                x.FilePath ?? string.Empty,
                x.CustomerAddress ?? string.Empty,
                x.VisitAddress ?? string.Empty,
                x.CreatorId,
                x.CreatedAt,
                x.ModifierId,
                x.ModifiedAt);
        }).ToList();

        return new PagedData<CustomerContactRecordListItemDto>(items, page.Total, input.PageIndex, input.PageSize);
    }
}
