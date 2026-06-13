using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.PositionAggregate;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.DomainEvents;

namespace Ncp.Admin.Domain.AggregatesModel.UserAggregate;

public partial record UserId : IInt64StronglyTypedId
{
    /// <summary>
    /// 未分配标识（哨兵值）
    /// </summary>
    public static UserId Unassigned { get; } = new(0);
}

public class User : Entity<UserId>, IAggregateRoot
{
    protected User()
    {
    }

    /// <summary>登录用户名</summary>
    public string Name { get; private set; } = string.Empty;
    /// <summary>邮箱地址</summary>
    public string Email { get; private set; } = string.Empty;
    /// <summary>手机号</summary>
    public string Phone { get; private set; } = string.Empty;
    /// <summary>真实姓名</summary>
    public string RealName { get; private set; } = string.Empty;
    /// <summary>用户状态（0=禁用，1=启用）</summary>
    public int Status { get; private set; }
    /// <summary>密码哈希</summary>
    public string PasswordHash { get; private set; } = string.Empty;
    /// <summary>是否激活</summary>
    public bool IsActive { get; private set; } = true;
    /// <summary>创建时间（UTC）</summary>
    public DateTimeOffset CreatedAt { get; init; }
    /// <summary>
    /// 创建人用户ID
    /// </summary>
    public UserId CreatorId { get; private set; } = UserId.Unassigned;
    /// <summary>最后修改人用户ID</summary>
    public UserId ModifierId { get; private set; } = UserId.Unassigned;
    /// <summary>删除人用户ID（软删时写入）</summary>
    public UserId DeleterId { get; private set; } = UserId.Unassigned;
    /// <summary>最后登录时间</summary>
    public DateTimeOffset LastLoginTime { get; private set; } = DateTimeOffset.MinValue;
    /// <summary>最后登录IP</summary>
    public string LastLoginIp { get; private set; } = string.Empty;
    /// <summary>最后更新时间（UTC）</summary>
    public UpdateTime UpdateTime { get; private set; } = new UpdateTime(DateTimeOffset.UtcNow);
    /// <summary>行版本（并发）</summary>
    public RowVersion RowVersion { get; private set; } = new RowVersion();
    /// <summary>是否已删除（软删标记）</summary>
    public Deleted IsDeleted { get; private set; } = new Deleted(false);
    /// <summary>删除时间（UTC）</summary>
    public DeletedTime DeletedAt { get; private set; } = new DeletedTime(DateTimeOffset.UtcNow);
    /// <summary>性别</summary>
    public string Gender { get; private set; } = string.Empty;
    /// <summary>出生日期</summary>
    public DateTimeOffset BirthDate { get; private set; } = DateTimeOffset.MinValue;
    /// <summary>身份证号</summary>
    public string IdCardNumber { get; private set; } = string.Empty;
    /// <summary>地址</summary>
    public string Address { get; private set; } = string.Empty;
    /// <summary>学历</summary>
    public string Education { get; private set; } = string.Empty;
    /// <summary>毕业院校</summary>
    public string GraduateSchool { get; private set; } = string.Empty;
    /// <summary>头像地址</summary>
    public string AvatarUrl { get; private set; } = string.Empty;
    /// <summary>是否不订餐（true=不订餐）</summary>
    public bool NotOrderMeal { get; private set; }
    /// <summary>订餐排序（数字越小越靠前）</summary>
    public int OrderMealSort { get; private set; }
    /// <summary>唯一码（如微信 GUID）</summary>
    public string WechatGuid { get; private set; } = string.Empty;
    /// <summary>是否离职</summary>
    public bool IsResigned { get; private set; }
    /// <summary>离职时间</summary>
    public DateTimeOffset ResignedTime { get; private set; } = DateTimeOffset.MinValue;
    /// <summary>
    /// 是否需要参与考勤计算。
    /// 老 OA User_List.Height=-1 表示“不记录考勤”，迁移后会写成 false。
    /// </summary>
    public bool AttendanceRequired { get; private set; } = true;
    /// <summary>
    /// 考勤规则业务编码。
    /// 老 OA User_List.Height 大于 0 时对应 AttendanceSetting_List.pk_AttendanceSettings；
    /// 新 OA 通过该值和 AttendanceRule.LegacyCode 关联，按用户原规则计算迟到、踩点、早退和旷工。
    /// </summary>
    public int AttendanceRuleCode { get; private set; }

    /// <summary>用户拥有的角色集合</summary>
    public virtual ICollection<UserRole> Roles { get; } = [];

    /// <summary>所属部门</summary>
    public virtual UserDept Dept { get; private set; } = default!;

    /// <summary>所属岗位（可选，一对一；无岗位时为 default）</summary>
    public virtual UserPosition Position { get; private set; } = default!;

    /// <summary>刷新令牌集合（用于续签登录）</summary>
    public virtual ICollection<UserRefreshToken> RefreshTokens { get; } = [];

    public User(
        string name,
        string phone,
        string password,
        IEnumerable<UserRole> roles,
        string realName,
        int status,
        string email,
        string gender,
        DateTimeOffset birthDate,
        UserId creatorId,
        string idCardNumber,
        string address,
        string education,
        string graduateSchool,
        string avatarUrl,
        bool notOrderMeal = false,
        string wechatGuid = "",
        bool isResigned = false,
        DateTimeOffset? resignedTime = null,
        bool attendanceRequired = true,
        int attendanceRuleCode = 0)
    {
        CreatedAt = DateTimeOffset.UtcNow;
        Name = name;
        Phone = phone;
        PasswordHash = password;
        RealName = realName;
        Status = status;
        Email = email;
        Gender = gender;
        BirthDate = birthDate;
        CreatorId = creatorId;
        ModifierId = creatorId;
        DeleterId = UserId.Unassigned; // 未删除时置为 0，软删时由 SoftDelete 写入实际删除人
        IdCardNumber = idCardNumber;
        Address = address;
        Education = education;
        GraduateSchool = graduateSchool;
        AvatarUrl = avatarUrl;
        NotOrderMeal = notOrderMeal;
        OrderMealSort = 0;
        WechatGuid = wechatGuid;
        IsResigned = isResigned;
        ResignedTime = resignedTime ?? DateTimeOffset.MinValue;
        AttendanceRequired = attendanceRequired;
        AttendanceRuleCode = attendanceRuleCode;
        foreach (var userRole in roles)
        {
            Roles.Add(userRole);
        }

        AddDomainEvent(new UserCreatedDomainEvent(this));
    }

    public void SoftDelete(UserId deleterId)
    {
        var now = DateTimeOffset.UtcNow;
        IsDeleted = true;
        DeletedAt = new DeletedTime(now);
        DeleterId = deleterId;
        ModifierId = deleterId;
        UpdateTime = new UpdateTime(now);
        AddDomainEvent(new UserSoftDeletedDomainEvent(Id));
    }

    public void PasswordReset(string password)
    {
        PasswordHash = password;
    }

    public void SetUserRefreshToken(string refreshToken)
    {
        var refreshTokenInfo = new UserRefreshToken(refreshToken);
        RefreshTokens.Add(refreshTokenInfo);
    }

    public void UpdateLastLoginTime(DateTimeOffset loginTime, string? loginIp = null)
    {
        var isFirstLogin = LastLoginTime == DateTimeOffset.MinValue;
        LastLoginTime = loginTime;
        if (!string.IsNullOrWhiteSpace(loginIp))
        {
            LastLoginIp = loginIp ?? string.Empty;
        }
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);

        if (isFirstLogin)
        {
            // LastLoginTime 的哨兵值只会在首次成功登录前出现，因此这里作为入职通知的唯一触发点。
            AddDomainEvent(new UserFirstLoggedInDomainEvent(Id, loginTime));
        }
    }

    public static int CalculateAge(DateTimeOffset birthDate)
    {
        var today = DateTimeOffset.UtcNow.Date;
        int age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age))
        {
            age--;
        }
        return age;
    }

    /// <summary>
    /// 年龄（派生属性；由 <see cref="BirthDate"/> 计算，不持久化）
    /// </summary>
    public int Age => CalculateAge(BirthDate);

    public void UpdateUserInfo(
        string name,
        string phone,
        string realName,
        int status,
        string email,
        string gender,
        DateTimeOffset birthDate,
        string idCardNumber,
        string address,
        string education,
        string graduateSchool,
        string avatarUrl,
        bool notOrderMeal,
        int? orderMealSort,
        string wechatGuid,
        bool isResigned,
        DateTimeOffset? resignedTime,
        UserId modifierId,
        bool attendanceRequired = true,
        int attendanceRuleCode = 0)
    {
        Name = name;
        Phone = phone;
        RealName = realName;
        Status = status;
        Email = email;
        Gender = gender;
        BirthDate = birthDate;
        IdCardNumber = idCardNumber;
        Address = address;
        Education = education;
        GraduateSchool = graduateSchool;
        AvatarUrl = avatarUrl;
        NotOrderMeal = notOrderMeal;
        if (orderMealSort.HasValue)
        {
            OrderMealSort = orderMealSort.Value;
        }
        WechatGuid = wechatGuid;
        IsResigned = isResigned;
        ResignedTime = isResigned ? resignedTime ?? DateTimeOffset.MinValue : DateTimeOffset.MinValue;
        AttendanceRequired = attendanceRequired;
        AttendanceRuleCode = attendanceRuleCode;
        ModifierId = modifierId;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
        if (isResigned)
        {
            AddDomainEvent(new UserResignedDomainEvent(Id));
        }
    }

    /// <summary>
    /// 更新月订餐汇总显示排序（与用户列表排序无关）。
    /// </summary>
    public void UpdateOrderMealSort(int orderMealSort, UserId modifierId)
    {
        OrderMealSort = orderMealSort;
        ModifierId = modifierId;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 绑定微信公众号 OpenID。
    /// </summary>
    public void BindWechat(string openId, UserId modifierId)
    {
        WechatGuid = string.IsNullOrWhiteSpace(openId) ? string.Empty : openId.Trim();
        ModifierId = modifierId;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 解除微信公众号绑定。
    /// </summary>
    public void UnbindWechat(UserId modifierId)
    {
        WechatGuid = string.Empty;
        ModifierId = modifierId;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    public void UpdateRoleInfo(RoleId roleId, string roleName)
    {
        var savedRole = Roles.FirstOrDefault(r => r.RoleId == roleId);
        savedRole?.UpdateRoleInfo(roleName);
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    public void UpdatePassword(string newPasswordHash)
    {
        if (!string.IsNullOrEmpty(newPasswordHash))
        {
            PasswordHash = newPasswordHash;
            UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
        }
    }

    /// <summary>
    /// 更新当前用户头像。
    /// </summary>
    public void UpdateAvatar(string avatarUrl, UserId modifierId)
    {
        AvatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? string.Empty : avatarUrl.Trim();
        ModifierId = modifierId;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    public void UpdateRoles(IEnumerable<UserRole> rolesToBeAssigned)
    {
        var currentRoleMap = Roles.ToDictionary(r => r.RoleId);
        var targetRoleMap = (rolesToBeAssigned ?? Enumerable.Empty<UserRole>())
            .GroupBy(r => r.RoleId)
            .ToDictionary(g => g.Key, g => g.First());

        var roleIdsToRemove = currentRoleMap.Keys.Except(targetRoleMap.Keys);
        foreach (var roleId in roleIdsToRemove)
        {
            Roles.Remove(currentRoleMap[roleId]);
        }

        var roleIdsToAdd = targetRoleMap.Keys.Except(currentRoleMap.Keys);
        foreach (var roleId in roleIdsToAdd)
        {
            var targetRole = targetRoleMap[roleId];
            Roles.Add(targetRole);
        }
    }

    /// <summary>
    /// 分配部门（使用当前用户主键作为 <see cref="UserDept"/> 的标识，调用方无需再传 userId）
    /// </summary>
    /// <param name="deptId">部门 ID</param>
    /// <param name="deptName">部门名称</param>
    public void AssignDept(DeptId deptId, string deptName)
    {
        var dept = new UserDept(deptId, deptName);
        Dept = dept;
    }

    /// <summary>
    /// 请求把当前用户追加为指定部门负责人。
    /// 该方法只发布跨聚合协作事件，负责人集合仍由部门聚合负责校验和写入。
    /// </summary>
    /// <param name="deptId">目标部门 ID。</param>
    /// <param name="setAsDefault">是否同步设为默认负责人。</param>
    public void RequestDeptResponsibleUserAssignment(DeptId deptId, bool setAsDefault)
    {
        if (deptId == DeptId.Unassigned)
        {
            throw new KnownException("设为部门负责人时必须选择部门", ErrorCodes.DeptNotFound);
        }

        AddDomainEvent(new UserDeptResponsibleUserAssignmentRequestedDomainEvent(Id, deptId, setAsDefault));
    }

    /// <summary>
    /// 请求清理当前用户的部门负责人关联。
    /// 该方法只发布协作事件，由应用层事件处理器转为部门负责人清理命令。
    /// </summary>
    public void RequestDeptResponsibleUserClear()
    {
        AddDomainEvent(new UserDeptResponsibleUserClearRequestedDomainEvent(Id));
    }

    /// <summary>
    /// 更新部门名称
    /// </summary>
    /// <param name="deptName">新的部门名称</param>
    public void UpdateDeptName(string deptName)
    {
        if (Dept == null)
        {
            return;
        }

        Dept.UpdateDeptName(deptName);
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 分配岗位
    /// </summary>
    /// <param name="positionId">岗位 ID</param>
    /// <param name="positionName">岗位名称</param>
    public void AssignPosition(PositionId positionId, string positionName)
    {
        Position = new UserPosition(positionId, positionName);
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 清除岗位
    /// </summary>
    public void ClearPosition()
    {
        Position = default!;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 更新岗位名称
    /// </summary>
    /// <param name="positionName">新的岗位名称</param>
    public void UpdatePositionName(string positionName)
    {
        if (Position == null)
        {
            return;
        }

        Position.UpdatePositionName(positionName);
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 撤销所有未过期的刷新令牌（用于退出登录）
    /// </summary>
    public void RevokeAllRefreshTokens()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var token in RefreshTokens)
        {
            // 只撤销未过期且未使用的令牌
            if (!token.IsRevoked && !token.IsUsed && token.ExpiresTime > now)
            {
                token.Revoke();
            }
        }
        UpdateTime = new UpdateTime(now);
    }
}

