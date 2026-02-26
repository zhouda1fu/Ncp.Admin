namespace Ncp.Admin.Domain;

/// <summary>
/// 错误码定义
/// </summary>
public sealed class ErrorCodes
{
    private ErrorCodes()
    {
        
    }

    #region 用户相关错误 (100xxx)

    /// <summary>
    /// 未找到账户
    /// </summary>
    public const int AccountNotFound = 100001;
    
    /// <summary>
    /// 用户名或密码错误
    /// </summary>
    public const int UserNameOrPasswordError = 100002;
    
    /// <summary>
    /// 未找到用户
    /// </summary>
    public const int UserNotFound = 100003;
    
    /// <summary>
    /// 无效的用户身份
    /// </summary>
    public const int InvalidUserIdentity = 100004;
    
    /// <summary>
    /// 无效的用户
    /// </summary>
    public const int InvalidUser = 100005;
    
    /// <summary>
    /// 无效的令牌
    /// </summary>
    public const int InvalidToken = 100006;
    
    /// <summary>
    /// 无效的刷新令牌
    /// </summary>
    public const int InvalidRefreshToken = 100007;
    
    #endregion
    
    #region 角色相关错误 (110xxx)
    
    /// <summary>
    /// 未找到角色
    /// </summary>
    public const int RoleNotFound = 110001;
    
    /// <summary>
    /// 不能删除管理员角色
    /// </summary>
    public const int CannotDeleteAdminRole = 110002;
    
    /// <summary>
    /// 角色已经被停用
    /// </summary>
    public const int RoleAlreadyDeactivated = 110003;
    
    /// <summary>
    /// 角色已经是激活状态
    /// </summary>
    public const int RoleAlreadyActivated = 110004;
    
    #endregion
    
    #region 部门相关错误 (120xxx)
    
    /// <summary>
    /// 未找到部门
    /// </summary>
    public const int DeptNotFound = 120001;
    
    /// <summary>
    /// 该部门下存在子部门，无法删除
    /// </summary>
    public const int DeptHasChildrenCannotDelete = 120002;
    
    /// <summary>
    /// 部门已经是激活状态
    /// </summary>
    public const int DeptAlreadyActivated = 120003;
    
    /// <summary>
    /// 部门已经被停用
    /// </summary>
    public const int DeptAlreadyDeactivated = 120004;
    
    /// <summary>
    /// 部门已经被删除
    /// </summary>
    public const int DeptAlreadyDeleted = 120005;
    
    /// <summary>
    /// 部门名称不能为空
    /// </summary>
    public const int DeptNameCannotBeEmpty = 120006;
    
    /// <summary>
    /// 子部门不能为空
    /// </summary>
    public const int ChildDeptCannotBeEmpty = 120007;
    
    #endregion

    #region 工作流相关错误 (130xxx)

    /// <summary>
    /// 未找到流程定义
    /// </summary>
    public const int WorkflowDefinitionNotFound = 130001;

    /// <summary>
    /// 流程定义已发布
    /// </summary>
    public const int WorkflowDefinitionAlreadyPublished = 130002;

    /// <summary>
    /// 流程定义已归档
    /// </summary>
    public const int WorkflowDefinitionAlreadyArchived = 130003;

    /// <summary>
    /// 流程定义已删除
    /// </summary>
    public const int WorkflowDefinitionAlreadyDeleted = 130004;

    /// <summary>
    /// 未找到流程实例
    /// </summary>
    public const int WorkflowInstanceNotFound = 130101;

    /// <summary>
    /// 流程未在运行中
    /// </summary>
    public const int WorkflowInstanceNotRunning = 130102;

    /// <summary>
    /// 流程未处于挂起状态
    /// </summary>
    public const int WorkflowInstanceNotSuspended = 130103;

    /// <summary>
    /// 只有发起人可以撤销流程
    /// </summary>
    public const int WorkflowOnlyInitiatorCanCancel = 130104;

    /// <summary>
    /// 未找到工作流任务
    /// </summary>
    public const int WorkflowTaskNotFound = 130201;

    /// <summary>
    /// 工作流任务已处理
    /// </summary>
    public const int WorkflowTaskAlreadyProcessed = 130202;

    /// <summary>
    /// 无法解析审批人（指定角色下无用户、部门主管未配置或发起人未选择审批人）
    /// </summary>
    public const int WorkflowAssigneeResolutionFailed = 130203;

    /// <summary>
    /// 任务并发冲突（该任务已被他人处理）
    /// </summary>
    public const int WorkflowTaskConcurrencyConflict = 130204;

    /// <summary>
    /// 同一业务键已有运行中的流程，请勿重复发起
    /// </summary>
    public const int WorkflowDuplicateBusinessKey = 130205;

    #endregion

    #region 岗位相关错误 (140xxx)

    /// <summary>
    /// 未找到岗位
    /// </summary>
    public const int PositionNotFound = 140001;

    /// <summary>
    /// 岗位已经是激活状态
    /// </summary>
    public const int PositionAlreadyActivated = 140002;

    /// <summary>
    /// 岗位已经被停用
    /// </summary>
    public const int PositionAlreadyDeactivated = 140003;

    /// <summary>
    /// 岗位已经被删除
    /// </summary>
    public const int PositionAlreadyDeleted = 140004;

    /// <summary>
    /// 岗位编码已存在
    /// </summary>
    public const int PositionCodeAlreadyExists = 140005;

    #endregion

    #region 通知相关错误 (150xxx)

    /// <summary>
    /// 未找到通知
    /// </summary>
    public const int NotificationNotFound = 150001;

    /// <summary>
    /// 通知已经被删除
    /// </summary>
    public const int NotificationAlreadyDeleted = 150002;

    #endregion

    #region 请假相关错误 (160xxx)

    /// <summary>
    /// 未找到请假申请
    /// </summary>
    public const int LeaveRequestNotFound = 160001;

    /// <summary>
    /// 只有草稿状态的请假单可以提交/修改
    /// </summary>
    public const int LeaveRequestNotDraft = 160002;

    /// <summary>
    /// 只有待审批状态的请假单可以审批/驳回
    /// </summary>
    public const int LeaveRequestNotPending = 160003;

    /// <summary>
    /// 只有草稿或待审批的请假单可以撤销
    /// </summary>
    public const int LeaveRequestCannotCancel = 160004;

    /// <summary>
    /// 未配置请假审批流程（无已发布的「请假审批」分类流程定义）
    /// </summary>
    public const int LeaveWorkflowNotConfigured = 160005;

    /// <summary>
    /// 未找到请假余额
    /// </summary>
    public const int LeaveBalanceNotFound = 160101;

    /// <summary>
    /// 请假余额不足
    /// </summary>
    public const int LeaveBalanceInsufficient = 160102;

    /// <summary>
    /// 扣减天数必须大于0
    /// </summary>
    public const int LeaveBalanceInvalidDeduct = 160103;

    /// <summary>
    /// 总天数不能小于已使用天数
    /// </summary>
    public const int LeaveBalanceInvalidTotal = 160104;

    #endregion

    #region 公告相关错误 (170xxx)

    /// <summary>
    /// 未找到公告
    /// </summary>
    public const int AnnouncementNotFound = 170001;
    /// <summary>
    /// 只有草稿可修改/发布
    /// </summary>
    public const int AnnouncementNotDraft = 170002;

    #endregion

    #region 考勤相关错误 (180xxx)

    /// <summary>
    /// 未找到考勤记录
    /// </summary>
    public const int AttendanceRecordNotFound = 180001;
    /// <summary>
    /// 今日已签到，请勿重复打卡
    /// </summary>
    public const int AttendanceAlreadyCheckedIn = 180002;
    /// <summary>
    /// 未找到排班
    /// </summary>
    public const int ScheduleNotFound = 180003;

    #endregion

    #region 报销相关错误 (190xxx)

    /// <summary>
    /// 未找到报销单
    /// </summary>
    public const int ExpenseClaimNotFound = 190001;
    /// <summary>
    /// 报销单状态不允许此操作
    /// </summary>
    public const int ExpenseClaimInvalidStatus = 190002;

    #endregion

    #region 会议/预订相关错误 (200xxx)

    /// <summary>
    /// 未找到会议室
    /// </summary>
    public const int MeetingRoomNotFound = 200001;
    /// <summary>
    /// 未找到预订
    /// </summary>
    public const int MeetingBookingNotFound = 200002;
    /// <summary>
    /// 该时段已被预订
    /// </summary>
    public const int MeetingRoomConflict = 200003;

    #endregion

    #region 任务/项目相关错误 (210xxx)

    /// <summary>
    /// 未找到项目
    /// </summary>
    public const int ProjectNotFound = 210001;

    /// <summary>
    /// 未找到任务
    /// </summary>
    public const int TaskNotFound = 210002;

    #endregion

    #region 通讯录相关错误 (220xxx)

    /// <summary>
    /// 未找到联系组
    /// </summary>
    public const int ContactGroupNotFound = 220001;

    /// <summary>
    /// 未找到联系人
    /// </summary>
    public const int ContactNotFound = 220002;

    #endregion

    #region 文档管理相关错误 (230xxx)

    /// <summary>
    /// 未找到文档
    /// </summary>
    public const int DocumentNotFound = 230001;

    /// <summary>
    /// 未找到文档版本
    /// </summary>
    public const int DocumentVersionNotFound = 230002;

    /// <summary>
    /// 未找到共享链接或链接已过期
    /// </summary>
    public const int ShareLinkNotFoundOrExpired = 230003;

    #endregion

    #region 即时通讯相关错误 (240xxx)

    /// <summary>
    /// 未找到聊天组
    /// </summary>
    public const int ChatGroupNotFound = 240001;

    /// <summary>
    /// 未找到聊天消息
    /// </summary>
    public const int ChatMessageNotFound = 240002;

    /// <summary>
    /// 当前用户不在该聊天组
    /// </summary>
    public const int NotMemberOfChatGroup = 240003;

    #endregion

    #region 合同相关错误 (250xxx)

    /// <summary>
    /// 未找到合同
    /// </summary>
    public const int ContractNotFound = 250001;
    /// <summary>
    /// 仅草稿状态可修改
    /// </summary>
    public const int ContractNotDraft = 250002;
    /// <summary>
    /// 仅审批中可通过
    /// </summary>
    public const int ContractNotPendingApproval = 250003;
    /// <summary>
    /// 仅已生效合同可归档
    /// </summary>
    public const int ContractNotApproved = 250004;

    #endregion

    #region 资产相关错误 (260xxx)

    /// <summary>
    /// 未找到资产
    /// </summary>
    public const int AssetNotFound = 260001;
    /// <summary>
    /// 资产状态不允许此操作
    /// </summary>
    public const int AssetInvalidStatus = 260002;
    /// <summary>
    /// 未找到资产领用记录
    /// </summary>
    public const int AssetAllocationNotFound = 260003;

    #endregion

    #region 车辆相关错误 (270xxx)

    /// <summary>
    /// 未找到车辆
    /// </summary>
    public const int VehicleNotFound = 270001;
    /// <summary>
    /// 未找到用车预订
    /// </summary>
    public const int VehicleBookingNotFound = 270002;
    /// <summary>
    /// 该时段车辆已被预订
    /// </summary>
    public const int VehicleBookingConflict = 270003;
    /// <summary>
    /// 预订状态不允许此操作
    /// </summary>
    public const int VehicleBookingInvalidStatus = 270004;

    #endregion
}
