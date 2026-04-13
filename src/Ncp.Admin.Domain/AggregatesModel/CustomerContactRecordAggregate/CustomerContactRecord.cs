using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.CustomerContactRecordAggregate;

/// <summary>
/// 客户联系记录 ID（强类型）
/// </summary>
public partial record CustomerContactRecordId : IGuidStronglyTypedId;

/// <summary>
/// 客户联系记录聚合根；通过 <see cref="CustomerId"/> 归属客户，与 <see cref="Customer"/> 生命周期解耦。
/// </summary>
public class CustomerContactRecord : Entity<CustomerContactRecordId>, IAggregateRoot
{
    protected CustomerContactRecord() { }

    public CustomerId CustomerId { get; private set; } = default!;

    /// <summary>拜访时间</summary>
    public DateTimeOffset RecordAt { get; private set; }

    /// <summary>联系类型</summary>
    public CustomerContactRecordType RecordType { get; private set; }

    public string Title { get; private set; } = string.Empty;

    /// <summary>联系内容</summary>
    public string Content { get; private set; } = string.Empty;

    public DateTimeOffset? NextVisitAt { get; private set; }

    public CustomerContactRecordStatus Status { get; private set; }

    public virtual ICollection<CustomerContactRecordContact> Contacts { get; } = [];

    /// <summary>负责人</summary>
    public UserId OwnerId { get; private set; } = new UserId(0);

    public string OwnerName { get; private set; } = string.Empty;

    public DeptId OwnerDeptId { get; private set; } = new DeptId(0);

    public string OwnerDeptName { get; private set; } = string.Empty;

    public UserId CreatorId { get; private set; } = new UserId(0);

    public DateTimeOffset CreatedAt { get; private set; }

    public UserId ModifierId { get; private set; } = new UserId(0);

    public DateTimeOffset ModifiedAt { get; private set; }

    public string Remark { get; private set; } = string.Empty;

    /// <summary>提醒间隔（天）：1、2、5</summary>
    public int ReminderIntervalDays { get; private set; } = (int)CustomerContactRecordReminderIntervalDays.OneDay;

    /// <summary>提醒次数</summary>
    public int ReminderCount { get; private set; } = 1;

    public string FilePath { get; private set; } = string.Empty;

    public string CustomerAddress { get; private set; } = string.Empty;

    public string VisitAddress { get; private set; } = string.Empty;

    public Deleted IsDeleted { get; private set; } = new Deleted(false);

    public DeletedTime DeletedAt { get; private set; } = new DeletedTime(DateTimeOffset.UtcNow);

    public UserId DeleterId { get; private set; } = new UserId(0);

    /// <summary>
    /// 新建联系记录；关联联系人通过 <see cref="CustomerContactRecordContact"/> 挂入 <see cref="Contacts"/>，
    /// <c>RecordId</c> 由 EF 在持久化时根据导航关系写入（与用户角色关联中由 EF 填充 <c>UserId</c> 的方式一致）。EF 使用无参 <see cref="CustomerContactRecord" />。
    /// </summary>
    public CustomerContactRecord(
        CustomerId customerId,
        DateTimeOffset recordAt,
        CustomerContactRecordType recordType,
        string title,
        string content,
        DateTimeOffset? nextVisitAt,
        CustomerContactRecordStatus status,
        UserId ownerId,
        string ownerName,
        DeptId ownerDeptId,
        string ownerDeptName,
        UserId creatorId,
        string remark,
        int reminderIntervalDays,
        int reminderCount,
        string filePath,
        string customerAddress,
        string visitAddress,
        IEnumerable<CustomerContactId>? customerContactIds = null)
    {
        var now = DateTimeOffset.UtcNow;
        CustomerId = customerId;
        RecordAt = recordAt.ToUniversalTime();
        RecordType = recordType;
        Title = title ?? string.Empty;
        Content = content ?? string.Empty;
        NextVisitAt = nextVisitAt?.ToUniversalTime();
        Status = status;
        OwnerId = ownerId;
        OwnerName = ownerName ?? string.Empty;
        OwnerDeptId = ownerDeptId;
        OwnerDeptName = ownerDeptName ?? string.Empty;
        CreatorId = creatorId;
        CreatedAt = now;
        ModifierId = creatorId;
        ModifiedAt = now;
        Remark = remark ?? string.Empty;
        ReminderIntervalDays = reminderIntervalDays;
        ReminderCount = reminderCount < 1 ? 1 : reminderCount;
        FilePath = filePath ?? string.Empty;
        CustomerAddress = customerAddress ?? string.Empty;
        VisitAddress = visitAddress ?? string.Empty;
        SetContactIds(customerContactIds);
    }

    public void Update(
        DateTimeOffset recordAt,
        CustomerContactRecordType recordType,
        string title,
        string content,
        DateTimeOffset? nextVisitAt,
        CustomerContactRecordStatus status,
        IEnumerable<CustomerContactId>? customerContactIds,
        UserId ownerId,
        string ownerName,
        DeptId ownerDeptId,
        string ownerDeptName,
        UserId modifierId,
        string remark,
        int reminderIntervalDays,
        int reminderCount,
        string filePath,
        string customerAddress,
        string visitAddress)
    {
        RecordAt = recordAt.ToUniversalTime();
        RecordType = recordType;
        Title = title ?? string.Empty;
        Content = content ?? string.Empty;
        NextVisitAt = nextVisitAt?.ToUniversalTime();
        Status = status;
        OwnerId = ownerId;
        OwnerName = ownerName ?? string.Empty;
        OwnerDeptId = ownerDeptId;
        OwnerDeptName = ownerDeptName ?? string.Empty;
        ModifierId = modifierId;
        ModifiedAt = DateTimeOffset.UtcNow;
        Remark = remark ?? string.Empty;
        ReminderIntervalDays = reminderIntervalDays;
        ReminderCount = reminderCount < 1 ? 1 : reminderCount;
        FilePath = filePath ?? string.Empty;
        CustomerAddress = customerAddress ?? string.Empty;
        VisitAddress = visitAddress ?? string.Empty;
        SetContactIds(customerContactIds);
    }

    public void SoftDelete(UserId deleterId)
    {
        IsDeleted = true;
        DeletedAt = new DeletedTime(DateTimeOffset.UtcNow);
        DeleterId = deleterId;
    }

    /// <summary>
    /// 同步关联联系人集合，差量增删
    /// </summary>
    private void SetContactIds(IEnumerable<CustomerContactId>? customerContactIds)
    {
        var desiredIds = (customerContactIds ?? []).Distinct().ToHashSet();
        var linkByContactId = Contacts.ToDictionary(c => c.ContactId);
        var currentIds = linkByContactId.Keys.ToHashSet();
        if (desiredIds.SetEquals(currentIds))
            return;

        foreach (var contactId in currentIds.Except(desiredIds))
            Contacts.Remove(linkByContactId[contactId]);

        foreach (var contactId in desiredIds.Except(currentIds))
            Contacts.Add(new CustomerContactRecordContact(contactId));
    }
}

/// <summary>
/// 客户联系记录与联系人的关联行；<see cref="RecordId"/> 由 EF 通过父记录 <see cref="CustomerContactRecord.Contacts"/> 导航填充。
/// </summary>
public class CustomerContactRecordContact
{
    protected CustomerContactRecordContact() { }

    /// <param name="contactId">客户联系人 ID</param>
    public CustomerContactRecordContact(CustomerContactId contactId)
    {
        ContactId = contactId;
    }

    public CustomerContactRecordId RecordId { get; private set; } = default!;
    public CustomerContactId ContactId { get; private set; } = default!;
}

