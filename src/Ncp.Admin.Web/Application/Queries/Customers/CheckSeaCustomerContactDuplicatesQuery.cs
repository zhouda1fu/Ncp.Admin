using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries.Customers;

/// <summary>
/// 公海录入：检查联系方式（电话/QQ/微信）是否与客户表重复
/// </summary>
/// <param name="MainContactPhone">主联系人电话</param>
/// <param name="ContactQq">QQ</param>
/// <param name="ContactWechat">微信</param>
public record CheckSeaCustomerContactDuplicatesQuery(
    string MainContactPhone,
    string ContactQq,
    string ContactWechat) : IQuery<IReadOnlyList<SeaCustomerDuplicateContactItem>>;

/// <summary>
/// 重复联系方式命中项
/// </summary>
/// <param name="CustomerId">命中的客户 ID</param>
/// <param name="CustomerName">命中的客户名称（优先全称/简称，否则用主联系人姓名兜底）</param>
/// <param name="CustomerSourceName">客户来源名称</param>
/// <param name="OwnerName">负责人姓名（公海未领用等场景可能为空）</param>
/// <param name="DuplicatePhones">重复的电话列表</param>
/// <param name="DuplicateQqs">重复的 QQ 列表</param>
/// <param name="DuplicateWechats">重复的微信列表</param>
public record SeaCustomerDuplicateContactItem(
    CustomerId CustomerId,
    string CustomerName,
    string CustomerSourceName,
    string OwnerName,
    IReadOnlyList<string> DuplicatePhones,
    IReadOnlyList<string> DuplicateQqs,
    IReadOnlyList<string> DuplicateWechats);

public class CheckSeaCustomerContactDuplicatesQueryValidator : AbstractValidator<CheckSeaCustomerContactDuplicatesQuery>
{
    public CheckSeaCustomerContactDuplicatesQueryValidator()
    {
        RuleFor(x => x).Must(HasAnyContact).WithMessage("请至少填写一种联系方式（电话、QQ 或微信）");
    }

    private static bool HasAnyContact(CheckSeaCustomerContactDuplicatesQuery q) =>
        !string.IsNullOrWhiteSpace(q.MainContactPhone)
        || !string.IsNullOrWhiteSpace(q.ContactQq)
        || !string.IsNullOrWhiteSpace(q.ContactWechat);
}

public class CheckSeaCustomerContactDuplicatesQueryHandler(ApplicationDbContext context)
    : IQueryHandler<CheckSeaCustomerContactDuplicatesQuery, IReadOnlyList<SeaCustomerDuplicateContactItem>>
{
    public async Task<IReadOnlyList<SeaCustomerDuplicateContactItem>> Handle(
        CheckSeaCustomerContactDuplicatesQuery request,
        CancellationToken cancellationToken)
    {
        var phone = ContactNormalize.NormalizePhoneForCompare(request.MainContactPhone);
        var qq = ContactNormalize.NormalizeIdForCompare(request.ContactQq);
        var wechat = ContactNormalize.NormalizeIdForCompare(request.ContactWechat);

        if (string.IsNullOrEmpty(phone) && string.IsNullOrEmpty(qq) && string.IsNullOrEmpty(wechat))
            return Array.Empty<SeaCustomerDuplicateContactItem>();

        // 与库内字段比对时须使用 EF 可翻译表达式（勿调用自定义静态方法）。
        // 电话侧额外匹配「86 + 11 位」以覆盖库内存 86138… 而录入侧规范化后为 138… 的情况。
        var phone86 = phone.Length == 11 ? "86" + phone : null;

        IQueryable<Customer> q = context.Customers.AsNoTracking();

        q = q.Where(c =>
            (!string.IsNullOrEmpty(phone) && (
                (c.MainContactPhone ?? "").Trim().Replace(" ", "").Replace("-", "").Replace("+", "") == phone
                || (phone86 != null
                    && (c.MainContactPhone ?? "").Trim().Replace(" ", "").Replace("-", "").Replace("+", "") == phone86)
                || c.Contacts.Any(cc =>
                    (cc.Mobile ?? "").Trim().Replace(" ", "").Replace("-", "").Replace("+", "") == phone
                    || (phone86 != null
                        && (cc.Mobile ?? "").Trim().Replace(" ", "").Replace("-", "").Replace("+", "") == phone86)
                    || (cc.Phone ?? "").Trim().Replace(" ", "").Replace("-", "").Replace("+", "") == phone
                    || (phone86 != null
                        && (cc.Phone ?? "").Trim().Replace(" ", "").Replace("-", "").Replace("+", "") == phone86))))
            || (!string.IsNullOrEmpty(qq) && (
                (c.ContactQq ?? "").Trim().Replace(" ", "").ToLower() == qq
                || c.Contacts.Any(cc => (cc.Qq ?? "").Trim().Replace(" ", "").ToLower() == qq)))
            || (!string.IsNullOrEmpty(wechat) && (
                (c.ContactWechat ?? "").Trim().Replace(" ", "").ToLower() == wechat
                || c.Contacts.Any(cc => (cc.Wechat ?? "").Trim().Replace(" ", "").ToLower() == wechat)))
        );

        var candidates = await q
            .Select(c => new
            {
                c.Id,
                c.FullName,
                c.ShortName,
                c.MainContactName,
                c.CustomerSourceName,
                c.OwnerName,
                c.MainContactPhone,
                c.ContactQq,
                c.ContactWechat,
                Contacts = c.Contacts.Select(cc => new { cc.Mobile, cc.Phone, cc.Qq, cc.Wechat }).ToList(),
            })
            .ToListAsync(cancellationToken);

        var result = new List<SeaCustomerDuplicateContactItem>(capacity: candidates.Count);
        foreach (var c in candidates)
        {
            var dupPhones = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var dupQqs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var dupWechats = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrEmpty(phone))
            {
                if (ContactNormalize.NormalizePhoneForCompare(c.MainContactPhone) == phone)
                    dupPhones.Add(c.MainContactPhone ?? string.Empty);

                foreach (var cc in c.Contacts)
                {
                    if (ContactNormalize.NormalizePhoneForCompare(cc.Mobile) == phone)
                        dupPhones.Add(cc.Mobile ?? string.Empty);
                    if (ContactNormalize.NormalizePhoneForCompare(cc.Phone) == phone)
                        dupPhones.Add(cc.Phone ?? string.Empty);
                }
            }

            if (!string.IsNullOrEmpty(qq))
            {
                if (ContactNormalize.NormalizeIdForCompare(c.ContactQq) == qq)
                    dupQqs.Add(c.ContactQq ?? string.Empty);
                foreach (var cc in c.Contacts)
                {
                    if (ContactNormalize.NormalizeIdForCompare(cc.Qq) == qq)
                        dupQqs.Add(cc.Qq ?? string.Empty);
                }
            }

            if (!string.IsNullOrEmpty(wechat))
            {
                if (ContactNormalize.NormalizeIdForCompare(c.ContactWechat) == wechat)
                    dupWechats.Add(c.ContactWechat ?? string.Empty);
                foreach (var cc in c.Contacts)
                {
                    if (ContactNormalize.NormalizeIdForCompare(cc.Wechat) == wechat)
                        dupWechats.Add(cc.Wechat ?? string.Empty);
                }
            }

            if (dupPhones.Count == 0 && dupQqs.Count == 0 && dupWechats.Count == 0)
                continue;

            var name = string.Empty;
            if (!string.IsNullOrWhiteSpace(c.FullName))
                name = c.FullName;
            else if (!string.IsNullOrWhiteSpace(c.ShortName))
                name = c.ShortName;
            else
                name = c.MainContactName ?? string.Empty;

            result.Add(new SeaCustomerDuplicateContactItem(
                c.Id,
                name,
                c.CustomerSourceName ?? string.Empty,
                c.OwnerName ?? string.Empty,
                dupPhones.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList(),
                dupQqs.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList(),
                dupWechats.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList()));
        }

        return result;
    }
}

internal static class ContactNormalize
{
    internal static string NormalizePhoneForCompare(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var s = input.Trim();
        s = s.Replace(" ", string.Empty)
            .Replace("-", string.Empty)
            .Replace("+", string.Empty);
        if (s.StartsWith("86") && s.Length == 13)
            s = s[2..];
        return s;
    }

    internal static string NormalizeIdForCompare(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        return input.Trim().Replace(" ", string.Empty).ToLowerInvariant();
    }
}

