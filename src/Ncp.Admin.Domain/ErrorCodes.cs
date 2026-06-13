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

    /// <summary>
    /// Excel 文件无效或表头不符合要求
    /// </summary>
    public const int InvalidExcelFile = 100008;

    /// <summary>
    /// 导出用户行数超过上限
    /// </summary>
    public const int UserExportTooManyRows = 100009;

    /// <summary>用户列表表头分面参数无效（如未指定 facetColumn）</summary>
    public const int UserInvalidColumnFacet = 100010;

    /// <summary>
    /// 当前登录会话已被其他设备上的新登录替换
    /// </summary>
    public const int SessionReplaced = 100011;

    /// <summary>
    /// 导出员工档案行数超过上限
    /// </summary>
    public const int EmployeeProfileExportTooManyRows = 100010;

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

    /// <summary>
    /// 该部门下存在用户，无法删除
    /// </summary>
    public const int DeptHasUsersCannotDelete = 120008;

    /// <summary>
    /// 该部门下存在岗位，无法删除
    /// </summary>
    public const int DeptHasPositionsCannotDelete = 120009;

    /// <summary>
    /// 上级部门不能为自己或自己的下级部门
    /// </summary>
    public const int DeptParentCannotBeSelfOrDescendant = 120010;

    /// <summary>
    /// 默认部门负责人必须在部门负责人列表中
    /// </summary>
    public const int DeptResponsibleUserDefaultInvalid = 120011;

    /// <summary>
    /// 部门排序列表无效（重复、缺失或与同级部门不一致）
    /// </summary>
    public const int DeptReorderInvalid = 120012;

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
    /// 流程定义当前状态不允许删除
    /// </summary>
    public const int WorkflowDefinitionCannotDelete = 130005;

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
    /// 无法解析审批人（指定角色下无用户、部门负责人未配置或发起人未选择审批人）
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

    /// <summary>
    /// 无权限操作该任务（非任务处理人或所属角色成员）
    /// </summary>
    public const int WorkflowTaskNotAssignedToOperator = 130206;

    /// <summary>
    /// 流程发起时未生成任何任务（流程配置可能无效）
    /// </summary>
    public const int WorkflowNoTasksOnStart = 130207;

    /// <summary>
    /// 不支持的审批人类型（设计器选项与后端未对齐）
    /// </summary>
    public const int WorkflowUnsupportedAssigneeType = 130208;

    /// <summary>
    /// 流程定义中审批/抄送节点未配置处理人或所选角色下无成员
    /// </summary>
    public const int WorkflowDefinitionInvalidAssigneeConfig = 130209;

    /// <summary>
    /// 工作流业务回写失败
    /// </summary>
    public const int WorkflowBusinessCallbackFailed = 130210;

    /// <summary>
    /// 审批人相对发起人的数据权限不足（数据范围过滤后无可用处理人）
    /// </summary>
    public const int WorkflowAssigneeDataPermissionDenied = 130211;

    /// <summary>
    /// 流程定义导入文件格式或内容与导出约定不一致
    /// </summary>
    public const int WorkflowDefinitionImportInvalid = 130212;

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

    /// <summary>
    /// 无效的面试记录表头分面列
    /// </summary>
    public const int InterviewRecordInvalidColumnFacet = 160105;

    /// <summary>
    /// 未找到人事福利申请
    /// </summary>
    public const int PersonnelBenefitApplicationNotFound = 160201;

    /// <summary>
    /// 只有草稿状态的人事福利申请可修改或提交
    /// </summary>
    public const int PersonnelBenefitApplicationNotDraft = 160202;

    /// <summary>
    /// 只有审批中状态的人事福利申请可审批完结或驳回
    /// </summary>
    public const int PersonnelBenefitApplicationNotPending = 160203;

    /// <summary>
    /// 只有草稿或审批中的申请可撤销
    /// </summary>
    public const int PersonnelBenefitApplicationCannotCancel = 160204;

    /// <summary>
    /// 只有审批通过的申请可登记实际购买/停保信息
    /// </summary>
    public const int PersonnelBenefitApplicationNotApproved = 160205;

    /// <summary>
    /// 未配置对应分类的人事福利审批流程
    /// </summary>
    public const int PersonnelBenefitWorkflowNotConfigured = 160206;

    /// <summary>
    /// 工作流业务类型与申请类型不一致
    /// </summary>
    public const int PersonnelBenefitKindMismatch = 160207;

    /// <summary>
    /// 当前审批节点需先选择购买人（财务部经办）
    /// </summary>
    public const int PersonnelBenefitPurchaserRequired = 160208;

    /// <summary>
    /// 购买人不在财务部可选范围内（含正副负责人已排除）
    /// </summary>
    public const int PersonnelBenefitInvalidPurchaser = 160209;

    /// <summary>
    /// 未找到「财务部」部门，无法解析购买人候选
    /// </summary>
    public const int PersonnelBenefitFinanceDeptNotFound = 160210;

    /// <summary>
    /// 首轮审批节点请勿指定购买人
    /// </summary>
    public const int PersonnelBenefitPurchaserNotAllowedYet = 160211;

    /// <summary>
    /// 购买人已指定，不可重复修改
    /// </summary>
    public const int PersonnelBenefitPurchaserAlreadyAssigned = 160212;

    /// <summary>
    /// 当前不满足分配购买人条件（如首轮审批尚未通过）
    /// </summary>
    public const int PersonnelBenefitCannotAssignPurchaser = 160213;

    /// <summary>
    /// 须先分配财务部购买人后再登记实际信息
    /// </summary>
    public const int PersonnelBenefitPurchaserRequiredBeforeFulfillment = 160214;

    /// <summary>
    /// 当前申请状态不可指定购买人
    /// </summary>
    public const int PersonnelBenefitCannotAssignPurchaserState = 160215;

    /// <summary>
    /// 无权查看或操作该人事福利申请
    /// </summary>
    public const int PersonnelBenefitApplicationAccessDenied = 160216;

    /// <summary>
    /// 当前用户或节点不允许通过列表指定购买人（须具备分配权限且为抄送相关待办/抄送人，且尚未指定购买人）
    /// </summary>
    public const int PersonnelBenefitAssignPurchaserOnlyInWorkflow = 160217;

    /// <summary>
    /// 仅指定的财务部购买人可登记实际购买/停保信息
    /// </summary>
    public const int PersonnelBenefitFulfillmentForbidden = 160218;

    /// <summary>
    /// 实际购买/停保信息已登记，无需重复提交
    /// </summary>
    public const int PersonnelBenefitFulfillmentAlreadyRecorded = 160219;

    #endregion

    #region 人事申请相关错误 (165xxx)

    /// <summary>
    /// 未找到人事申请
    /// </summary>
    public const int PersonnelApplicationNotFound = 165001;

    /// <summary>
    /// 人事申请状态不允许编辑
    /// </summary>
    public const int PersonnelApplicationCannotEdit = 165002;

    /// <summary>
    /// 人事申请状态不允许提交
    /// </summary>
    public const int PersonnelApplicationCannotSubmit = 165003;

    /// <summary>
    /// 人事申请不是审批中
    /// </summary>
    public const int PersonnelApplicationNotPending = 165004;

    /// <summary>
    /// 人事申请状态不允许撤回
    /// </summary>
    public const int PersonnelApplicationCannotCancel = 165005;

    /// <summary>人事申请审批已开始，申请人不可撤回。</summary>
    public const int PersonnelApplicationApprovalStarted = 165006;

    /// <summary>未配置人事申请审批流程</summary>
    public const int PersonnelApplicationWorkflowNotConfigured = 165007;

    /// <summary>人事申请状态不允许作废</summary>
    public const int PersonnelApplicationCannotVoid = 165008;

    /// <summary>人事申请状态不允许删除</summary>
    public const int PersonnelApplicationCannotDelete = 165009;

    /// <summary>已通过出差申请不允许编辑行程</summary>
    public const int PersonnelApplicationCannotEditTrips = 165010;

    /// <summary>出差行程变更校验失败（如删除已发生行程）</summary>
    public const int PersonnelApplicationTripChangeInvalid = 165011;

    /// <summary>当前不允许补充上传人事申请附件</summary>
    public const int PersonnelApplicationCannotEditAttachments = 165012;

    /// <summary>外出申请未勾选产生费用或不适用报销</summary>
    public const int PersonnelApplicationOutingReimbursementNotApplicable = 165013;

    /// <summary>外出报销已审核通过</summary>
    public const int PersonnelApplicationOutingReimbursementAlreadyApproved = 165014;

    /// <summary>报销说明不能为空</summary>
    public const int PersonnelApplicationOutingReimbursementNoteEmpty = 165015;

    /// <summary>无权操作外出报销</summary>
    public const int PersonnelApplicationOutingReimbursementForbidden = 165016;

    /// <summary>报销说明条数超限</summary>
    public const int PersonnelApplicationOutingReimbursementNoteLimit = 165017;

    public const int PersonnelApplicationWithdrawalRecallInvalidStatus = 165018;

    public const int PersonnelApplicationWithdrawalRecallAlreadyPending = 165019;

    public const int PersonnelApplicationWithdrawalRecallReasonRequired = 165020;

    public const int PersonnelApplicationWithdrawalRecallApproverNotFound = 165021;

    public const int PersonnelApplicationWithdrawalRecallNotPending = 165022;

    public const int PersonnelApplicationWithdrawalRecallForbidden = 165023;

    /// <summary>人事申请日期/时段与已有申请冲突</summary>
    public const int PersonnelApplicationScheduleConflict = 165024;

    /// <summary>人事申请日期超出允许范围（每月前三个工作日后不可选上月）</summary>
    public const int PersonnelApplicationDateNotAllowed = 165026;

    /// <summary>无权查看该员工的流程报表</summary>
    public const int WorkflowReportAccessDenied = 165025;

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
    /// <summary>
    /// 只有已发布公告可执行该操作
    /// </summary>
    public const int AnnouncementNotPublished = 170003;
    /// <summary>
    /// 公告标题不能为空
    /// </summary>
    public const int AnnouncementTitleRequired = 170004;
    /// <summary>
    /// 公告内容不能为空
    /// </summary>
    public const int AnnouncementContentRequired = 170005;

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

    /// <summary>
    /// 中国节假日日历数据不可用（接口无数据或请求失败）
    /// </summary>
    public const int ChineseHolidayCalendarUnavailable = 180004;

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
    /// <summary>
    /// 未找到会议
    /// </summary>
    public const int MeetingNotFound = 200004;
    /// <summary>
    /// 未找到会议类型
    /// </summary>
    public const int MeetingTypeOptionNotFound = 200005;
    /// <summary>
    /// 会议类型值重复
    /// </summary>
    public const int MeetingTypeOptionTypeValueDuplicate = 200006;
    /// <summary>
    /// 该时段存在进行中的会议占用，不可发布或申请协调
    /// </summary>
    public const int MeetingRoomInProgressConflict = 200007;

    #endregion

    #region 订餐模块相关错误 (201xxx)

    /// <summary>未找到菜品</summary>
    public const int OrderMealMenuNotFound = 201001;

    /// <summary>未找到个人订餐记录</summary>
    public const int OrderMealMyRecordNotFound = 201002;

    /// <summary>个人订餐记录已取消不可修改</summary>
    public const int OrderMealMyRecordCancelled = 201003;

    /// <summary>未找到取消审核单</summary>
    public const int OrderMealCancelNotFound = 201004;

    /// <summary>取消审核状态不允许此操作</summary>
    public const int OrderMealCancelInvalidState = 201005;

    /// <summary>未找到投票活动</summary>
    public const int OrderMealVoteNotFound = 201006;

    /// <summary>投票状态不允许此操作</summary>
    public const int OrderMealVoteInvalidState = 201007;

    /// <summary>已参与投票</summary>
    public const int OrderMealVoteAlreadySubmitted = 201008;

    /// <summary>投票已结束或未开始</summary>
    public const int OrderMealVoteClosed = 201009;

    /// <summary>未找到日菜单配置</summary>
    public const int OrderMealDayMenuNotFound = 201010;

    /// <summary>人员订餐过滤不满足（不订餐/离职等）</summary>
    public const int OrderMealUserFilterInvalid = 201011;

    /// <summary>无权限操作订餐业务</summary>
    public const int OrderMealPermissionDenied = 201012;

    /// <summary>未配置订餐管理工作流（流程定义 Category 须为 OrderMeal）</summary>
    public const int OrderMealCancelWorkflowNotConfigured = 201013;

    /// <summary>该日已有进行中的订餐取消审批</summary>
    public const int OrderMealCancelDuplicatePending = 201014;

    /// <summary>当前时段无需发起取消审批（如当日 9 点前请直接取消）</summary>
    public const int OrderMealCancelWorkflowNotRequired = 201015;

    /// <summary>日订餐汇总「就餐完毕」确认条件不满足（非当日、早于 12 点等）</summary>
    public const int OrderMealDailyFinishNotAllowed = 201016;

    /// <summary>日订餐汇总行政取消条件不满足（非当日等）</summary>
    public const int OrderMealDailySummaryCancelNotAllowed = 201017;

    /// <summary>无效的订单ID</summary>
    public const int InvalidOrderId = 201018;

    


    #endregion

    #region 任务/项目相关错误 (210xxx)

    /// <summary>
    /// 未找到项目
    /// </summary>
    public const int ProjectNotFound = 210001;

    /// <summary>
    /// 未找到项目联系人
    /// </summary>
    public const int ProjectContactNotFound = 210003;

    /// <summary>
    /// 未找到项目跟进记录
    /// </summary>
    public const int ProjectFollowUpRecordNotFound = 210004;

    /// <summary>
    /// 项目绑定客户无效（为空、重复等）
    /// </summary>
    public const int ProjectCustomerBindingsInvalid = 210005;

    /// <summary>
    /// 所选客户未与本商机建立关联，不能维护该客户下的联系人
    /// </summary>
    public const int ProjectSelectedCustomerNotBound = 210006;

    /// <summary>
    /// 跟进记录所选客户未与本商机建立关联或无效
    /// </summary>
    public const int ProjectFollowUpCustomerNotBound = 210007;

    /// <summary>
    /// 该客户在本商机下已有联系人或跟进记录，不允许解绑
    /// </summary>
    public const int ProjectCustomerCannotUnbind = 210008;

    /// <summary>
    /// 商机名称与当前录入人/销售经理名下已有商机重复
    /// </summary>
    public const int ProjectOpportunityNameDuplicate = 210009;

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

    #region 网盘管理相关错误 (230xxx)

    /// <summary>
    /// 未找到网盘条目
    /// </summary>
    public const int DriveItemNotFound = 230001;

    /// <summary>
    /// 网盘条目名称无效
    /// </summary>
    public const int DriveItemNameInvalid = 230002;

    /// <summary>
    /// 网盘条目操作无效
    /// </summary>
    public const int DriveItemInvalidOperation = 230003;

    /// <summary>
    /// 网盘条目移动无效
    /// </summary>
    public const int DriveItemInvalidMove = 230004;

    /// <summary>
    /// 网盘条目已删除
    /// </summary>
    public const int DriveItemDeleted = 230005;

    /// <summary>
    /// 网盘同目录名称已存在
    /// </summary>
    public const int DriveItemNameDuplicated = 230006;

    /// <summary>
    /// 无权访问网盘条目
    /// </summary>
    public const int DriveItemAccessDenied = 230007;

    /// <summary>
    /// 站内分享授权无效
    /// </summary>
    public const int DriveShareGrantInvalid = 230008;

    /// <summary>
    /// 公开分享链接无效
    /// </summary>
    public const int DriveShareLinkInvalid = 230009;

    /// <summary>
    /// 未找到共享链接或链接已过期
    /// </summary>
    public const int DriveShareLinkNotFoundOrExpired = 230010;

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
    /// <summary>
    /// 合同分享/转交对象须为财务部人员
    /// </summary>
    public const int ContractShareTargetNotInFinanceDept = 250005;
    /// <summary>
    /// 未找到财务部，无法分享/转交
    /// </summary>
    public const int ContractFinanceDeptNotFound = 250006;

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
    /// <summary>
    /// 未找到资产分类
    /// </summary>
    public const int AssetCategoryNotFound = 260004;
    /// <summary>
    /// 资产分类存在子分类，无法删除
    /// </summary>
    public const int AssetCategoryHasChildren = 260005;
    /// <summary>
    /// 资产分类已被资产引用，无法删除
    /// </summary>
    public const int AssetCategoryInUse = 260006;
    /// <summary>
    /// 未找到资产维修记录
    /// </summary>
    public const int AssetRepairRecordNotFound = 260007;
    /// <summary>
    /// 未找到资产转让记录
    /// </summary>
    public const int AssetTransferRecordNotFound = 260008;

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

    /// <summary>
    /// 未找到车辆保险记录
    /// </summary>
    public const int CarInsuranceNotFound = 271001;

    /// <summary>
    /// 未找到车辆年检记录
    /// </summary>
    public const int CarInspectionRecordNotFound = 271002;

    /// <summary>
    /// 未找到用车用途类型选项
    /// </summary>
    public const int CarUsageTypeOptionNotFound = 271003;

    /// <summary>
    /// 用车用途类型值已存在
    /// </summary>
    public const int CarUsageTypeOptionTypeValueDuplicate = 271004;

    /// <summary>
    /// 无效的用车用途类型
    /// </summary>
    public const int CarUsageTypeOptionInvalid = 271005;

    #endregion

    #region 客户相关错误 (280xxx)

    /// <summary>
    /// 未找到客户
    /// </summary>
    public const int CustomerNotFound = 280001;
    /// <summary>
    /// 未找到客户联系人
    /// </summary>
    public const int CustomerContactNotFound = 280002;
    /// <summary>
    /// 客户不在公海
    /// </summary>
    public const int CustomerNotInSea = 280003;
    /// <summary>
    /// 客户已被领用
    /// </summary>
    public const int CustomerAlreadyClaimed = 280004;
    /// <summary>
    /// 未找到客户联系记录
    /// </summary>
    public const int CustomerContactRecordNotFound = 280005;

    /// <summary>
    /// 无效的客户联络表头分面列
    /// </summary>
    public const int CustomerContactRecordInvalidColumnFacet = 280026;

    /// <summary>
    /// 未找到客户来源
    /// </summary>
    public const int CustomerSourceNotFound = 280006;

    /// <summary>
    /// 未找到区域
    /// </summary>
    public const int RegionNotFound = 280007;

    /// <summary>
    /// 所选联系人不属于该客户（联系记录关联校验）
    /// </summary>
    public const int CustomerContactNotBelongsToCustomer = 280014;

    /// <summary>
    /// 客户已作废，不允许执行该操作
    /// </summary>
    public const int CustomerIsVoided = 280015;

    /// <summary>关联客户参数无效（如关联自身）</summary>
    public const int CustomerRelatedCustomerInvalid = 280025;

    /// <summary>全库客户检索关键字长度不足（至少 5 个字符）</summary>
    public const int CustomerGlobalSearchKeywordTooShort = 280026;

    /// <summary>客户全称在当前用户数据权限范围内已存在（全字匹配，与保存前查重口径一致）</summary>
    public const int CustomerFullNameDuplicateInScope = 280027;

    /// <summary>
    /// 未配置客户转交/分享审批流程
    /// </summary>
    public const int CustomerCollaborationWorkflowNotConfigured = 280028;

    /// <summary>
    /// 仅客户负责人可发起转交（被分享仅可见时不可转交）
    /// </summary>
    public const int CustomerCollaborationTransferRequiresOwnership = 280029;

    /// <summary>
    /// 批量代转交：当前用户数据权限为「仅本人」时不允许指定其他负责人
    /// </summary>
    public const int CustomerCollaborationBulkTransferDelegatedRequiresNonSelfDataScope = 280037;

    /// <summary>
    /// 批量转交：提交的客户不在可转交范围内（无数据权限、非指定负责人、公海或已作废）
    /// </summary>
    public const int CustomerCollaborationBulkTransferCustomerNotInScope = 280038;

    /// <summary>
    /// 批量转交：单次申请客户数量超过上限
    /// </summary>
    public const int CustomerCollaborationBulkTransferTooManyCustomers = 280039;

    /// <summary>
    /// 协作分享范围不包含当前操作维度（商机/订单/售后等），禁止写入
    /// </summary>
    public const int CustomerShareScopeWriteDenied = 280030;

    /// <summary>
    /// 未找到营销中心部门（用于客户公海片区分配范围校验）
    /// </summary>
    public const int CustomerSeaRegionAssignMarketingCenterDeptNotFound = 280017;

    /// <summary>
    /// 目标用户不在营销中心及其下级部门范围内（用于客户公海片区分配越权校验）
    /// </summary>
    public const int CustomerSeaRegionAssignUserOutOfMarketingCenter = 280018;

    /// <summary>
    /// 目标用户不在当前登录人的数据权限范围内（用于客户公海片区分配越权校验）
    /// </summary>
    public const int CustomerSeaRegionAssignUserOutOfDataPermission = 280019;

    /// <summary>
    /// 无法解析被分配人的直属上级（按部门父链 + 角色数据权限未匹配到候选人）
    /// </summary>
    public const int CustomerSeaRegionAssignDirectSupervisorNotFound = 280040;

    /// <summary>
    /// 已解析到直属上级，但其未配置任何客户公海片区；禁止同步以免清空被分配人片区
    /// </summary>
    public const int CustomerSeaRegionAssignDirectSupervisorHasNoRegions = 280041;

    /// <summary>
    /// 未配置客户作废审批流程（需发布 Category=CustomerVoid 的已发布流程定义；兼容旧库 CustomerSeaVoid / CustomerArchiveVoid）
    /// </summary>
    public const int CustomerSeaVoidWorkflowNotConfigured = 280020;

    /// <summary>发起客户作废审批时当前用户无任何角色</summary>
    public const int CustomerSeaVoidOperatorHasNoRoles = 280021;

    /// <summary>多角色用户未选择本次作废审批使用的路由角色</summary>
    public const int CustomerSeaVoidRoutingRoleRequired = 280022;

    /// <summary>所选路由角色不属于当前用户</summary>
    public const int CustomerSeaVoidRoutingRoleNotAssignedToUser = 280023;

    /// <summary>仍在公海且未领用的客户仅创建人可直接作废</summary>
    public const int CustomerSeaVoidUnclaimedOnlyCreator = 280031;

    /// <summary>未领用公海线索不应发起作废审批（由创建人在公海列表直接作废）</summary>
    public const int CustomerSeaVoidUnclaimedNoWorkflow = 280032;

    /// <summary>未配置客户作废审批流程（与 <see cref="CustomerSeaVoidWorkflowNotConfigured"/> 同场景；历史错误码名保留）</summary>
    public const int CustomerArchiveVoidWorkflowNotConfigured = 280033;

    /// <summary>档案客户作废审批仅适用于已离公海的客户；公海客户请使用客户公海作废</summary>
    public const int CustomerArchiveVoidUseSeaFlow = 280034;

    /// <summary>仅当前业务负责人可申请档案客户作废审批</summary>
    public const int CustomerArchiveVoidNotOwner = 280035;

    /// <summary>审批完成时客户负责人与申请发起人不一致，无法完成作废</summary>
    public const int CustomerArchiveVoidOwnerChanged = 280036;

    /// <summary>同步商机联系人到客户档案时，该客户已存在同名联系人</summary>
    public const int CustomerContactDuplicateNameWhenSyncFromProject = 280024;

    #endregion

    #region 主数据与选项 (280xxx，编号沿用存量；与上方「客户域」逻辑分区分离)

    /// <summary>
    /// 未找到项目类型
    /// </summary>
    public const int ProjectTypeNotFound = 280008;

    /// <summary>
    /// 未找到项目状态选项
    /// </summary>
    public const int ProjectStatusOptionNotFound = 280009;

    /// <summary>
    /// 未找到项目行业
    /// </summary>
    public const int ProjectIndustryNotFound = 280010;

    /// <summary>
    /// 未找到合同类型选项
    /// </summary>
    public const int ContractTypeOptionNotFound = 280011;

    /// <summary>
    /// 未找到收支类型选项
    /// </summary>
    public const int IncomeExpenseTypeOptionNotFound = 280012;

    /// <summary>
    /// 未找到合同发票
    /// </summary>
    public const int ContractInvoiceNotFound = 280013;

    /// <summary>
    /// 未找到行业（主数据）
    /// </summary>
    public const int IndustryNotFound = 280016;

    #endregion

    #region 证书相关错误 (300xxx)

    /// <summary>
    /// 未找到通用证书
    /// </summary>
    public const int GeneralCertificateNotFound = 300001;

    /// <summary>
    /// 未找到域证书
    /// </summary>
    public const int DomainCertificateNotFound = 300002;

    /// <summary>
    /// 证书已作废
    /// </summary>
    public const int CertificateAlreadyVoided = 300003;

    /// <summary>
    /// 域证书接收人不能为空
    /// </summary>
    public const int CertificateReceiverRequired = 300004;

    /// <summary>
    /// 非创建人不可操作域证书
    /// </summary>
    public const int CertificateOperatorNotCreator = 300005;

    /// <summary>
    /// 域证书来源不能为空
    /// </summary>
    public const int DomainCertificateDataSourceRequired = 300006;

    /// <summary>
    /// 域证书证书名称不能为空
    /// </summary>
    public const int DomainCertificateNameRequired = 300007;

    /// <summary>
    /// 域证书域名不能为空
    /// </summary>
    public const int DomainCertificateDomainNameRequired = 300008;

    /// <summary>
    /// 未找到证书类型
    /// </summary>
    public const int CertificateTypeNotFound = 300009;

    /// <summary>
    /// 同一适用范围下类型值已存在
    /// </summary>
    public const int CertificateTypeValueDuplicate = 300010;

    #endregion

    #region 售后工单工作流 (301xxx)

    /// <summary>未配置售后工单审批流程</summary>
    public const int AfterSalesServiceWorkflowNotConfigured = 301001;

    /// <summary>当前用户未分配角色，无法发起售后工单审批</summary>
    public const int AfterSalesServiceOperatorHasNoRoles = 301002;

    /// <summary>多角色用户未选择本次售后工单审批使用的路由角色</summary>
    public const int AfterSalesServiceRoutingRoleRequired = 301003;

    /// <summary>所选路由角色不属于当前用户</summary>
    public const int AfterSalesServiceRoutingRoleNotAssignedToUser = 301004;

    /// <summary>未配置售后技术申请审批流程</summary>
    public const int AfterSalesServiceTechnologyWorkflowNotConfigured = 301005;

    /// <summary>当前用户未分配角色，无法发起售后技术申请审批</summary>
    public const int AfterSalesServiceTechnologyOperatorHasNoRoles = 301006;

    /// <summary>多角色用户未选择本次售后技术申请审批使用的路由角色</summary>
    public const int AfterSalesServiceTechnologyRoutingRoleRequired = 301007;

    /// <summary>所选路由角色不属于当前用户（售后技术申请）</summary>
    public const int AfterSalesServiceTechnologyRoutingRoleNotAssignedToUser = 301008;

    /// <summary>无权查看该售后工单</summary>
    public const int AfterSalesServiceForbidden = 301009;

    /// <summary>未配置「事务部」部门，无法分配跟进人</summary>
    public const int AfterSalesServiceAffairsDeptNotConfigured = 301010;

    /// <summary>跟进人须为事务部人员</summary>
    public const int AfterSalesServiceAssignUserNotInAffairsDept = 301011;

    /// <summary>未配置售后申请作废审批流程</summary>
    public const int AfterSalesServiceVoidWorkflowNotConfigured = 301012;

    /// <summary>仅跟进人可发起售后申请作废</summary>
    public const int AfterSalesServiceVoidNotFollowUser = 301013;

    /// <summary>仅过程记录创建人可修改或删除该记录</summary>
    public const int AfterSalesServiceRecordForbidden = 301014;

    #endregion

    #region 订单相关错误 (290xxx)

    /// <summary>
    /// 未找到订单
    /// </summary>
    public const int OrderNotFound = 290001;

    /// <summary>
    /// 系统生成的订单合同文件不允许下载或打印
    /// </summary>
    public const int OrderContractFileDownloadNotAllowed = 290003;

    /// <summary>
    /// 客户协作转交后，订单仍归属原业务经理期间，被转交人仅可查看不可修改/删除/提交
    /// </summary>
    public const int OrderEditDeniedCustomerCollaborationViewOnly = 290002;

    /// <summary>
    /// 订单不是审核中状态
    /// </summary>
    public const int OrderNotPendingAudit = 290010;

    /// <summary>
    /// 订单不是已驳回状态
    /// </summary>
    public const int OrderNotRejected = 290011;

    /// <summary>
    /// 未配置订单审批流程
    /// </summary>
    public const int OrderWorkflowNotConfigured = 290012;

    /// <summary>
    /// 只有草稿或已驳回的订单可以提交审批
    /// </summary>
    public const int OrderCannotSubmitForApproval = 290013;

    /// <summary>仅普测订单可发起延迟申请</summary>
    public const int OrderGeneralTestDelayInvalidType = 290026;

    /// <summary>草稿或已驳回的普测订单不可发起延迟申请</summary>
    public const int OrderGeneralTestDelayInvalidStatus = 290027;

    /// <summary>延迟后的结束日期无效</summary>
    public const int OrderGeneralTestDelayInvalidEndDate = 290028;

    /// <summary>无权发起普测延迟申请</summary>
    public const int OrderGeneralTestDelayDenied = 290029;

    /// <summary>未配置普测延迟申请审批流程</summary>
    public const int OrderGeneralTestDelayWorkflowNotConfigured = 290030;

    /// <summary>
    /// 当前订单状态不允许修改
    /// </summary>
    public const int OrderEditDeniedInvalidStatus = 290021;

    /// <summary>
    /// 仅订单发起人可在草稿或已驳回状态下修改
    /// </summary>
    public const int OrderEditDeniedNotInitiator = 290022;

    /// <summary>
    /// 非当前审批/抄送节点处理人，无法在审核中修改订单
    /// </summary>
    public const int OrderEditDeniedNotCurrentWorkflowParticipant = 290023;

    /// <summary>
    /// 订单一键撤回：当前状态不允许撤回为草稿（仅审核中）
    /// </summary>
    public const int OrderRecallToDraftInvalidStatus = 290024;

    /// <summary>
    /// 未找到订单发票类型选项
    /// </summary>
    public const int OrderInvoiceTypeOptionNotFound = 290014;

    /// <summary>
    /// 订单发货：非该单配货人/仓库技术/复核人，不可编辑或确认发货
    /// </summary>
    public const int OrderWarehouseShippingEditDenied = 290030;

    /// <summary>
    /// 订单发货：分配人员须为仓储部成员
    /// </summary>
    public const int OrderWarehouseAssigneeNotInWarehouseDept = 290031;

    /// <summary>
    /// 订单发货：当前用户无权执行该明细操作（非对应负责人）
    /// </summary>
    public const int OrderWarehouseItemActionDenied = 290032;

    /// <summary>
    /// 订单发货：产品明细尚未全部完成配货/软件安装/复核确认，不可发货
    /// </summary>
    public const int OrderWarehouseShippingItemsNotAllConfirmed = 290034;

    /// <summary>
    /// 开票申请：关联订单不存在
    /// </summary>
    public const int OrderInvoiceApplicationOrderNotFound = 290025;

    /// <summary>
    /// 开票申请：订单未勾选需要开票
    /// </summary>
    public const int OrderInvoiceApplicationNeedInvoiceRequired = 290026;

    /// <summary>
    /// 开票申请：仅订单发起人可发起
    /// </summary>
    public const int OrderInvoiceApplicationNotOrderCreator = 290027;

    /// <summary>
    /// 开票申请：该订单已有进行中的开票任务
    /// </summary>
    public const int OrderInvoiceApplicationDuplicateTask = 290028;

    /// <summary>
    /// 开票申请：任务类型必须为开票申请
    /// </summary>
    public const int OrderInvoiceApplicationInvalidTaskType = 290029;

    /// <summary>
    /// 订单任务：当前订单流程节点不允许创建订单类型任务
    /// </summary>
    public const int OrderOfficeTaskWorkflowNodeNotAllowed = 290030;

    /// <summary>
    /// 无权限从订单创建办公任务
    /// </summary>
    public const int OrderOfficeTaskCreateDenied = 290031;

    /// <summary>
    /// 无权限查看订单关联办公任务
    /// </summary>
    public const int OrderRelatedTaskViewDenied = 290032;

    /// <summary>
    /// 订单物流公司 ID 无效
    /// </summary>
    public const int OrderLogisticsCompanyIdInvalid = 290015;

    /// <summary>
    /// 订单物流方式 ID 无效
    /// </summary>
    public const int OrderLogisticsMethodIdInvalid = 290016;

    /// <summary>
    /// 未找到物流公司
    /// </summary>
    public const int OrderLogisticsCompanyNotFound = 290019;

    /// <summary>
    /// 未找到物流方式
    /// </summary>
    public const int OrderLogisticsMethodNotFound = 290020;

    /// <summary>
    /// 未找到订单备注
    /// </summary>
    public const int OrderRemarkNotFound = 290017;

    /// <summary>
    /// 订单备注类型不匹配
    /// </summary>
    public const int OrderRemarkTypeMismatch = 290018;

    /// <summary>
    /// 未找到产品
    /// </summary>
    public const int ProductNotFound = 290002;

    /// <summary>
    /// 未找到产品参数
    /// </summary>
    public const int ProductParameterNotFound = 290003;

    /// <summary>
    /// 未找到产品分类
    /// </summary>
    public const int ProductCategoryNotFound = 290004;

    /// <summary>
    /// 未找到产品类型
    /// </summary>
    public const int ProductTypeNotFound = 290006;

    /// <summary>
    /// 未找到供应商
    /// </summary>
    public const int SupplierNotFound = 290005;

    /// <summary>
    /// 供应商名称已存在
    /// </summary>
    public const int SupplierFullNameDuplicate = 290006;

    #endregion

    #region 片区项目 (291xxx)

    /// <summary>
    /// 未找到片区项目
    /// </summary>
    public const int ZoneProjectNotFound = 291001;

    /// <summary>
    /// 片区项目已删除
    /// </summary>
    public const int ZoneProjectAlreadyDeleted = 291002;

    /// <summary>
    /// 片区项目编号重复
    /// </summary>
    public const int ZoneProjectDuplicateProjectNumber = 291003;

    /// <summary>
    /// 所属名称无效（为空或为“全部”）
    /// </summary>
    public const int ZoneProjectInvalidSheetName = 291004;

    /// <summary>
    /// 片区项目负责人解析失败（所属名称未绑定用户或用户无效）
    /// </summary>
    public const int ZoneProjectOwnerResolveFailed = 291005;

    /// <summary>
    /// 片区项目参数无效（如转移年份越界）
    /// </summary>
    public const int ZoneProjectInvalidArgument = 291006;

    #endregion

    #region 评价相关错误 (320xxx)

    /// <summary>
    /// 未找到评价
    /// </summary>
    public const int EvaluateNotFound = 320001;

    /// <summary>
    /// 评价评分超出允许范围
    /// </summary>
    public const int EvaluateScoreOutOfRange = 320002;

    /// <summary>
    /// 不能评价自己
    /// </summary>
    public const int EvaluateCannotEvaluateSelf = 320003;

    /// <summary>
    /// 无权限操作评价
    /// </summary>
    public const int EvaluatePermissionDenied = 320004;

    /// <summary>
    /// 非任务参与人或任务状态不允许任务评分
    /// </summary>
    public const int EvaluateTaskAccessDenied = 320005;

    /// <summary>
    /// 被评人不在任务参与人范围内
    /// </summary>
    public const int EvaluateTaskTargetNotAllowed = 320006;

    /// <summary>
    /// 评价导出范围无效
    /// </summary>
    public const int EvaluateExportInvalidScope = 320007;

    /// <summary>
    /// 评价导出条数超过上限
    /// </summary>
    public const int EvaluateExportTooManyRows = 320008;

    #endregion

    #region 董事长信箱相关错误 (330xxx)

    /// <summary>
    /// 未找到董事长信箱来信
    /// </summary>
    public const int ChairmanMailboxMessageNotFound = 330001;

    /// <summary>
    /// 来信内容不能为空
    /// </summary>
    public const int ChairmanMailboxMessageRequired = 330002;

    /// <summary>
    /// 来信内容过长
    /// </summary>
    public const int ChairmanMailboxMessageTooLong = 330003;

    /// <summary>
    /// 回复内容不能为空
    /// </summary>
    public const int ChairmanMailboxReplyRequired = 330004;

    /// <summary>
    /// 回复内容过长
    /// </summary>
    public const int ChairmanMailboxReplyTooLong = 330005;

    /// <summary>
    /// 查询码格式无效
    /// </summary>
    public const int ChairmanMailboxTrackingCodeInvalid = 330006;

    /// <summary>
    /// 来信已归档
    /// </summary>
    public const int ChairmanMailboxMessageArchived = 330007;

    /// <summary>
    /// 查询码生成失败
    /// </summary>
    public const int ChairmanMailboxTrackingCodeGenerateFailed = 330008;

    /// <summary>
    /// 匿名访问过于频繁
    /// </summary>
    public const int ChairmanMailboxRateLimited = 330009;

    /// <summary>
    /// 董事长信箱导出行数超过上限
    /// </summary>
    public const int ChairmanMailboxExportTooManyRows = 330010;

    /// <summary>无权访问董事长信箱管理端</summary>
    public const int ChairmanMailboxAdminForbidden = 330011;

    #endregion

    #region 培训 (310xxx)

    /// <summary>
    /// 培训客户 TrainGuid 不能为空
    /// </summary>
    public const int TrainCustomerTrainGuidRequired = 310001;

    /// <summary>
    /// 培训客户名称不能为空
    /// </summary>
    public const int TrainCustomerNameRequired = 310002;

    /// <summary>
    /// 培训联系人姓名不能为空
    /// </summary>
    public const int TrainContactNameRequired = 310003;

    /// <summary>
    /// 培训客户已删除
    /// </summary>
    public const int TrainCustomerAlreadyDeleted = 310004;

    /// <summary>
    /// 未找到培训客户
    /// </summary>
    public const int TrainCustomerNotFound = 310005;

    /// <summary>
    /// 未找到培训缴费记录
    /// </summary>
    public const int TrainPaymentNotFound = 310006;

    /// <summary>
    /// 培训卡数量无效
    /// </summary>
    public const int TrainCardCountInvalid = 310007;

    /// <summary>
    /// 未找到培训联系人
    /// </summary>
    public const int TrainContactNotFound = 310008;

    /// <summary>
    /// 未找到培训课程
    /// </summary>
    public const int TrainCourseNotFound = 310009;

    /// <summary>
    /// 未找到培训拜访/沟通记录
    /// </summary>
    public const int TrainVisitNotFound = 310010;

    /// <summary>
    /// 未找到培训缴费卡
    /// </summary>
    public const int TrainCardNotFound = 310011;

    /// <summary>
    /// 培训缴费记录数量无效
    /// </summary>
    public const int TrainPaymentCountInvalid = 310012;

    public const int TrainCustomerContactRequired = 310013;
    public const int TrainCustomerAddressRequired = 310014;
    public const int TrainCustomerAreaProvinceRequired = 310015;
    public const int TrainCustomerAreaCityRequired = 310016;
    public const int TrainCustomerAreaCountyRequired = 310017;
    public const int TrainCustomerIndustryRequired = 310018;
    public const int TrainCustomerRemarksRequired = 310019;
    public const int TrainCustomerCustomerSourceRequired = 310027;

    public const int TrainCourseNameRequired = 310020;
    public const int TrainCourseMembersRequired = 310021;
    public const int TrainVisitTitleRequired = 310022;
    public const int TrainVisitContactsRequired = 310023;
    public const int TrainPaymentGuidRequired = 310024;
    public const int TrainPaymentTitleRequired = 310025;
    public const int TrainPaymentHasBoundCards = 310028;

    #endregion

    #region 办公任务 (292xxx)

    /// <summary>
    /// 未找到办公任务类型定义
    /// </summary>
    public const int OfficeTaskTypeNotFound = 292001;

    /// <summary>
    /// 办公任务类型已停用
    /// </summary>
    public const int OfficeTaskTypeDisabled = 292002;

    /// <summary>
    /// 未找到办公任务
    /// </summary>
    public const int OfficeTaskNotFound = 292003;

    /// <summary>
    /// 办公任务起止时间无效
    /// </summary>
    public const int OfficeTaskInvalidTimeRange = 292004;

    /// <summary>
    /// 办公任务接收人必填
    /// </summary>
    public const int OfficeTaskReceiversRequired = 292005;

    /// <summary>
    /// 该任务类型要求关联客户
    /// </summary>
    public const int OfficeTaskCustomerRequired = 292006;

    /// <summary>
    /// 办公任务状态不允许的变更
    /// </summary>
    public const int OfficeTaskInvalidStatusTransition = 292007;

    /// <summary>
    /// 办公任务当前状态不可编辑
    /// </summary>
    public const int OfficeTaskNotEditable = 292008;

    /// <summary>
    /// 当前状态不允许回复或上传附件（仅进行中允许）
    /// </summary>
    public const int OfficeTaskInteractionNotAllowed = 292009;

    /// <summary>
    /// 回复内容不能为空
    /// </summary>
    public const int OfficeTaskReplyContentRequired = 292010;

    /// <summary>
    /// 附件元数据无效
    /// </summary>
    public const int OfficeTaskAttachmentInvalid = 292011;

    /// <summary>
    /// 办公任务到期提醒参数无效
    /// </summary>
    public const int OfficeTaskInvalidReminder = 292012;

    /// <summary>
    /// 办公任务类型编码已存在
    /// </summary>
    public const int OfficeTaskTypeDefinitionCodeDuplicate = 292013;

    /// <summary>
    /// 办公任务类型排序列表与当前数据不一致
    /// </summary>
    public const int OfficeTaskTypeDefinitionReorderInvalid = 292014;

    /// <summary>
    /// 仅任务发起人可执行该操作
    /// </summary>
    public const int OfficeTaskOperatorNotCreator = 292015;

    /// <summary>
    /// 仅主接收人可执行接收侧操作（完成、转交）
    /// </summary>
    public const int OfficeTaskOperatorNotReceiver = 292016;

    /// <summary>
    /// 该任务类型不允许转交
    /// </summary>
    public const int OfficeTaskTransferNotAllowed = 292017;

    /// <summary>
    /// 当前接收状态不允许完成或转交
    /// </summary>
    public const int OfficeTaskReceiveActionNotAllowed = 292018;

    /// <summary>
    /// 追加任务时未找到父任务
    /// </summary>
    public const int OfficeTaskParentNotFound = 292019;

    /// <summary>
    /// 原任务未完成或未确认完成，不可追加子任务
    /// </summary>
    public const int OfficeTaskAppendParentNotEligible = 292020;

    /// <summary>
    /// CAD/效果图任务须先完成接收确认
    /// </summary>
    public const int OfficeTaskReceiverAcceptancePending = 292021;

    /// <summary>
    /// 该任务不需要接收确认
    /// </summary>
    public const int OfficeTaskReceiverAcceptanceNotRequired = 292022;

    /// <summary>
    /// 接收确认已处理
    /// </summary>
    public const int OfficeTaskReceiverAcceptanceAlreadyHandled = 292023;

    /// <summary>
    /// 审核人分配的任务不可拒绝
    /// </summary>
    public const int OfficeTaskReceiverAcceptanceMandatory = 292024;

    /// <summary>
    /// 开票申请须上传双章合同附件或勾选延迟上传
    /// </summary>
    public const int OfficeTaskInvoiceDoubleSealRequired = 292030;

    /// <summary>
    /// 当前状态不允许重新分配接收人
    /// </summary>
    public const int OfficeTaskReceiverReassignNotAllowed = 292025;

    /// <summary>
    /// 重新分配接收人目标无效
    /// </summary>
    public const int OfficeTaskReceiverReassignInvalidTargets = 292026;

    /// <summary>
    /// 仅任务审核人可执行该操作
    /// </summary>
    public const int OfficeTaskOperatorNotApprover = 292027;

    /// <summary>
    /// 办公任务未关联订单，无法定制分配
    /// </summary>
    public const int OfficeTaskCustomizationOrderRequired = 292028;

    /// <summary>
    /// 定制分配需选择人员
    /// </summary>
    public const int OfficeTaskCustomizationAssigneeRequired = 292029;

    /// <summary>
    /// 当前用户无权为离职接收人代完结
    /// </summary>
    public const int OfficeTaskResignedReceiverProxyNotAllowed = 292043;

    /// <summary>
    /// 定制分配至少选择一行产品人员
    /// </summary>
    public const int OfficeTaskCustomizationNoAssignee = 292030;

    /// <summary>
    /// 未找到定制分配记录
    /// </summary>
    public const int OfficeTaskCustomizationNotFound = 292031;

    /// <summary>
    /// 定制分配人员不在任务接收人（产品岗位）范围内
    /// </summary>
    public const int OfficeTaskCustomizationAssigneeInvalid = 292032;

    /// <summary>
    /// 未找到定制流程步骤
    /// </summary>
    public const int OfficeTaskCustomizationProcessNotFound = 292033;

    /// <summary>
    /// 无权限操作该定制流程
    /// </summary>
    public const int OfficeTaskCustomizationProcessForbidden = 292034;

    /// <summary>
    /// 请先维护流程预计开始与结束时间
    /// </summary>
    public const int OfficeTaskCustomizationPlanRequired = 292035;

    /// <summary>
    /// 流程计划时间无效
    /// </summary>
    public const int OfficeTaskCustomizationScheduleInvalid = 292036;

    /// <summary>
    /// 请先填写任务进度内容
    /// </summary>
    public const int OfficeTaskCustomizationContentRequired = 292037;

    /// <summary>
    /// 当前用户不是设计师或程序员
    /// </summary>
    public const int OfficeTaskCustomizationNotDesignerOrProgrammer = 292038;

    /// <summary>
    /// 仅产品经理可维护流程计划时间
    /// </summary>
    public const int OfficeTaskCustomizationSchedulePmOnly = 292039;

    /// <summary>
    /// 请先填写审核人
    /// </summary>
    public const int OfficeTaskCustomizationReviewerRequired = 292040;

    /// <summary>
    /// 流程完成前置条件未满足
    /// </summary>
    public const int OfficeTaskCustomizationCompletionBlocked = 292041;

    /// <summary>
    /// 无权查看或操作该定制任务
    /// </summary>
    public const int OfficeTaskCustomizationForbidden = 292042;

    /// <summary>
    /// 方案报价申请：具体事项除默认提示外须至少填写一项
    /// </summary>
    public const int OfficeTaskSchemeQuoteContentIncomplete = 292043;

    /// <summary>
    /// 定制人员维护：用户无效或已离职
    /// </summary>
    public const int OfficeTaskCustomizationRoleMemberUserInvalid = 292044;

    #endregion

    #region 用户反馈错误码

    /// <summary>
    /// 未找到用户反馈
    /// </summary>
    public const int UserFeedbackNotFound = 340001;

    /// <summary>
    /// 用户反馈内容不能为空
    /// </summary>
    public const int UserFeedbackContentRequired = 340002;

    /// <summary>
    /// 用户反馈内容过长
    /// </summary>
    public const int UserFeedbackContentTooLong = 340003;

    /// <summary>
    /// 用户反馈提交人不能为空
    /// </summary>
    public const int UserFeedbackSubmitterRequired = 340004;

    /// <summary>
    /// 用户反馈处理人不能为空
    /// </summary>
    public const int UserFeedbackHandlerRequired = 340005;

    /// <summary>
    /// 用户反馈已归档
    /// </summary>
    public const int UserFeedbackArchived = 340006;

    /// <summary>
    /// 用户反馈已关闭
    /// </summary>
    public const int UserFeedbackClosed = 340007;

    /// <summary>
    /// 用户反馈附件数量超限
    /// </summary>
    public const int UserFeedbackAttachmentCountExceeded = 340008;

    /// <summary>
    /// 用户反馈附件过大
    /// </summary>
    public const int UserFeedbackAttachmentTooLarge = 340009;

    /// <summary>
    /// 用户反馈附件无效
    /// </summary>
    public const int UserFeedbackAttachmentInvalid = 340010;

    /// <summary>
    /// 用户反馈导出数据过多
    /// </summary>
    public const int UserFeedbackExportTooManyRows = 340011;

    /// <summary>
    /// 无权访问用户反馈
    /// </summary>
    public const int UserFeedbackForbidden = 340012;

    #endregion
}
