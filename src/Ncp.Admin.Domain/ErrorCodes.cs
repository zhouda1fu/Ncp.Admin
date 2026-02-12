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
}
