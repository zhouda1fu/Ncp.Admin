namespace Ncp.Admin.Web.AppPermissions
{
    /// <summary>
    /// 权限映射辅助类，用于通过权限代码自动获取权限名称和描述
    /// </summary>
    public static class PermissionMapper
    {
        /// <summary>
        /// 权限代码到权限描述的映射字典
        /// 从 SeedDatabaseExtension.cs 中提取
        /// </summary>
        private static readonly Dictionary<string, string> _permissionDescriptionMap = new()
    {
        // 父权限码（用于菜单和路由权限控制）
        { PermissionCodes.UserManagement, "用户管理" },
        { PermissionCodes.RoleManagement, "角色管理" },
        { PermissionCodes.DeptManagement, "部门管理" },

        // 用户管理权限
        { PermissionCodes.UserCreate, "创建新用户" },
        { PermissionCodes.UserView, "查看用户信息" },
        { PermissionCodes.UserEdit, "更新用户信息" },
        { PermissionCodes.UserDelete, "删除用户" },
        { PermissionCodes.UserRoleAssign, "分配用户角色权限" },
        { PermissionCodes.UserResetPassword, "重置用户密码" },

        // 角色管理权限
        { PermissionCodes.RoleCreate, "创建新角色" },
        { PermissionCodes.RoleView, "查看角色信息" },
        { PermissionCodes.RoleEdit, "更新角色信息" },
        { PermissionCodes.RoleDelete, "删除角色" },
        { PermissionCodes.RoleUpdatePermissions, "更新角色的权限" },

        // 部门管理权限
        { PermissionCodes.DeptCreate, "创建部门" },
        { PermissionCodes.DeptView, "查看部门信息" },
        { PermissionCodes.DeptEdit, "更新部门信息" },
        { PermissionCodes.DeptDelete, "删除部门" },

        // 工作流管理权限
        { PermissionCodes.WorkflowManagement, "工作流管理" },
        { PermissionCodes.WorkflowDefinitionView, "查看流程定义" },
        { PermissionCodes.WorkflowDefinitionCreate, "创建流程定义" },
        { PermissionCodes.WorkflowDefinitionEdit, "编辑流程定义" },
        { PermissionCodes.WorkflowDefinitionDelete, "删除流程定义" },
        { PermissionCodes.WorkflowDefinitionPublish, "发布流程定义" },
        { PermissionCodes.WorkflowStart, "发起流程" },
        { PermissionCodes.WorkflowCancel, "撤销流程" },
        { PermissionCodes.WorkflowTaskApprove, "审批任务" },
        { PermissionCodes.WorkflowInstanceView, "查看流程实例" },
        { PermissionCodes.WorkflowMonitor, "流程监控" },

        // 岗位管理权限
        { PermissionCodes.PositionManagement, "岗位管理" },
        { PermissionCodes.PositionCreate, "创建岗位" },
        { PermissionCodes.PositionView, "查看岗位信息" },
        { PermissionCodes.PositionEdit, "更新岗位信息" },
        { PermissionCodes.PositionDelete, "删除岗位" },

        // 通知管理权限
        { PermissionCodes.NotificationManagement, "通知管理" },
        { PermissionCodes.NotificationView, "查看通知" },
        { PermissionCodes.NotificationSend, "发送通知" },

        // 公告管理权限
        { PermissionCodes.AnnouncementManagement, "公告管理" },
        { PermissionCodes.AnnouncementView, "查看公告" },
        { PermissionCodes.AnnouncementCreate, "创建公告" },
        { PermissionCodes.AnnouncementEdit, "编辑公告" },
        { PermissionCodes.AnnouncementPublish, "发布公告" },

        // 会议/预订管理权限
        { PermissionCodes.MeetingManagement, "会议管理" },
        { PermissionCodes.MeetingRoomView, "查看会议室" },
        { PermissionCodes.MeetingRoomEdit, "管理会议室" },
        { PermissionCodes.MeetingBookingView, "查看预订" },
        { PermissionCodes.MeetingBookingCreate, "预订会议室" },

        // 报销管理权限
        { PermissionCodes.ExpenseManagement, "报销管理" },
        { PermissionCodes.ExpenseClaimView, "查看报销单" },
        { PermissionCodes.ExpenseClaimCreate, "创建报销单" },
        { PermissionCodes.ExpenseClaimSubmit, "提交报销单" },

        // 考勤管理权限
        { PermissionCodes.AttendanceManagement, "考勤管理" },
        { PermissionCodes.AttendanceRecordView, "查看考勤记录" },
        { PermissionCodes.AttendanceCheckIn, "打卡/签退" },
        { PermissionCodes.ScheduleView, "查看排班" },
        { PermissionCodes.ScheduleEdit, "编辑排班" },

        // 请假管理权限
        { PermissionCodes.LeaveManagement, "请假管理" },
        { PermissionCodes.LeaveRequestView, "查看请假申请" },
        { PermissionCodes.LeaveRequestCreate, "创建请假申请" },
        { PermissionCodes.LeaveRequestEdit, "编辑请假申请" },
        { PermissionCodes.LeaveRequestSubmit, "提交请假申请" },
        { PermissionCodes.LeaveRequestCancel, "撤销请假申请" },
        { PermissionCodes.LeaveBalanceView, "查看请假余额" },
        { PermissionCodes.LeaveBalanceEdit, "设置请假余额" },

        // 任务/项目管理权限
        { PermissionCodes.TaskManagement, "任务管理" },
        { PermissionCodes.ProjectView, "查看项目" },
        { PermissionCodes.ProjectCreate, "创建项目" },
        { PermissionCodes.ProjectEdit, "编辑项目" },
        { PermissionCodes.TaskView, "查看任务" },
        { PermissionCodes.TaskCreate, "创建任务" },
        { PermissionCodes.TaskEdit, "编辑任务" },

        // 文档管理权限
        { PermissionCodes.DocumentManagement, "文档管理" },
        { PermissionCodes.DocumentView, "查看文档" },
        { PermissionCodes.DocumentCreate, "上传文档" },
        { PermissionCodes.DocumentEdit, "编辑文档" },
        { PermissionCodes.DocumentShare, "创建共享链接" },

        // 即时通讯权限
        { PermissionCodes.ChatManagement, "即时通讯" },
        { PermissionCodes.ChatView, "查看会话与消息" },
        { PermissionCodes.ChatCreate, "创建会话" },

        // 通讯录管理权限
        { PermissionCodes.ContactManagement, "通讯录管理" },
        { PermissionCodes.ContactGroupView, "查看联系组" },
        { PermissionCodes.ContactGroupCreate, "创建联系组" },
        { PermissionCodes.ContactGroupEdit, "编辑联系组" },
        { PermissionCodes.ContactView, "查看联系人" },
        { PermissionCodes.ContactCreate, "创建联系人" },
        { PermissionCodes.ContactEdit, "编辑联系人" },

        // 资产管理权限
        { PermissionCodes.AssetManagement, "资产管理" },
        { PermissionCodes.AssetView, "查看资产" },
        { PermissionCodes.AssetCreate, "创建资产" },
        { PermissionCodes.AssetEdit, "编辑资产" },
        { PermissionCodes.AssetAllocate, "领用资产" },
        { PermissionCodes.AssetReturn, "归还资产" },
        { PermissionCodes.AssetScrap, "报废资产" },
        { PermissionCodes.AssetAllocationView, "查看领用记录" },

        // 车辆管理权限
        { PermissionCodes.VehicleManagement, "车辆管理" },
        { PermissionCodes.VehicleView, "查看车辆" },
        { PermissionCodes.VehicleEdit, "管理车辆" },
        { PermissionCodes.VehicleBookingView, "查看预订" },
        { PermissionCodes.VehicleBookingCreate, "预订用车" },
        { PermissionCodes.VehicleBookingCancel, "取消预订" },
        { PermissionCodes.VehicleBookingComplete, "完成预订" },

        // 合同管理权限
        { PermissionCodes.ContractManagement, "合同管理" },
        { PermissionCodes.ContractView, "查看合同" },
        { PermissionCodes.ContractCreate, "创建合同" },
        { PermissionCodes.ContractEdit, "编辑合同" },
        { PermissionCodes.ContractSubmit, "提交审批" },
        { PermissionCodes.ContractApprove, "审批合同" },
        { PermissionCodes.ContractArchive, "归档合同" },

        // 所有接口访问权限
        { PermissionCodes.AllApiAccess, "所有接口访问权限" },
    };

        /// <summary>
        /// 通过权限代码获取权限信息（名称和描述）
        /// </summary>
        /// <param name="permissionCode">权限代码</param>
        /// <returns>返回 (权限名称, 权限描述) 元组</returns>
        public static (string Name, string Description) GetPermissionInfo(string permissionCode)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(permissionCode);

            // 优先从 PermissionDefinitionContext 获取权限名称（DisplayName）
            var permissionName = GetPermissionNameFromContext(permissionCode);

            // 如果 PermissionDefinitionContext 中没有找到，使用权限代码作为名称
            if (string.IsNullOrEmpty(permissionName))
            {
                permissionName = permissionCode;
            }

            // 从映射字典获取权限描述
            var permissionDescription = _permissionDescriptionMap.TryGetValue(permissionCode, out var description)
                ? description
                : string.Empty;

            return (permissionName, permissionDescription);
        }

        /// <summary>
        /// 从 PermissionDefinitionContext 中查找权限的 DisplayName
        /// </summary>
        /// <param name="permissionCode">权限代码</param>
        /// <returns>权限的显示名称，如果未找到则返回 null</returns>
        private static string? GetPermissionNameFromContext(string permissionCode)
        {
            foreach (var group in PermissionDefinitionContext.PermissionGroups)
            {
                // 在权限组的所有权限及其子权限中查找
                foreach (var permission in group.Permissions)
                {
                    var found = FindPermissionRecursive(permission, permissionCode);
                    if (found != null)
                    {
                        return found.DisplayName;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 递归查找权限
        /// </summary>
        /// <param name="permission">当前权限</param>
        /// <param name="permissionCode">要查找的权限代码</param>
        /// <returns>找到的权限对象，如果未找到则返回 null</returns>
        private static AppPermission? FindPermissionRecursive(AppPermission permission, string permissionCode)
        {
            // 检查当前权限是否匹配
            if (permission.Code == permissionCode)
            {
                return permission;
            }

            // 递归查找子权限
            foreach (var child in permission.Children)
            {
                var found = FindPermissionRecursive(child, permissionCode);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
