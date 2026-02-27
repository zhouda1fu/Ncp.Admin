import type { DataNode } from 'ant-design-vue/es/tree';

import { PermissionCodes } from '#/constants/permission-codes';
import { $t } from '#/locales';

/**
 * 权限树节点接口
 */
export interface PermissionTreeNode extends DataNode {
  value: string;
  label: string;
  icon?: string;
  children?: PermissionTreeNode[];
}

/**
 * 构建权限树结构
 * 基于 PermissionDefinitionContext 的层级结构
 */
export function buildPermissionTree(): PermissionTreeNode[] {
  return [
    {
      value: PermissionCodes.UserManagement,
      label: '用户管理',
      icon: 'mdi:account',
      children: [
        {
          value: PermissionCodes.UserView,
          label: '查看用户',
          icon: 'mdi:eye',
        },
        {
          value: PermissionCodes.UserCreate,
          label: '创建用户',
          icon: 'mdi:account-plus',
        },
        {
          value: PermissionCodes.UserEdit,
          label: '编辑用户',
          icon: 'mdi:account-edit',
        },
        {
          value: PermissionCodes.UserDelete,
          label: '删除用户',
          icon: 'mdi:account-remove',
        },
        {
          value: PermissionCodes.UserRoleAssign,
          label: '分配用户角色',
          icon: 'mdi:account-group',
        },
        {
          value: PermissionCodes.UserResetPassword,
          label: '重置用户密码',
          icon: 'mdi:lock-reset',
        },
      ],
    },
    {
      value: PermissionCodes.RoleManagement,
      label: '角色管理',
      icon: 'mdi:account-group',
      children: [
        {
          value: PermissionCodes.RoleView,
          label: '查看角色',
          icon: 'mdi:eye',
        },
        {
          value: PermissionCodes.RoleCreate,
          label: '创建角色',
          icon: 'mdi:account-plus',
        },
        {
          value: PermissionCodes.RoleEdit,
          label: '编辑角色',
          icon: 'mdi:account-edit',
        },
        {
          value: PermissionCodes.RoleDelete,
          label: '删除角色',
          icon: 'mdi:account-remove',
        },
        {
          value: PermissionCodes.RoleUpdatePermissions,
          label: '更新角色权限',
          icon: 'mdi:shield-edit',
        },
      ],
    },
    {
      value: PermissionCodes.DeptManagement,
      label: '部门管理',
      icon: 'charm:organisation',
      children: [
        {
          value: PermissionCodes.DeptView,
          label: '查看部门',
          icon: 'mdi:eye',
        },
        {
          value: PermissionCodes.DeptCreate,
          label: '创建部门',
          icon: 'mdi:account-plus',
        },
        {
          value: PermissionCodes.DeptEdit,
          label: '编辑部门',
          icon: 'mdi:account-edit',
        },
        {
          value: PermissionCodes.DeptDelete,
          label: '删除部门',
          icon: 'mdi:account-remove',
        },
      ],
    },
    {
      value: PermissionCodes.PositionManagement,
      label: '岗位管理',
      icon: 'mdi:briefcase',
      children: [
        {
          value: PermissionCodes.PositionView,
          label: '查看岗位',
          icon: 'mdi:eye',
        },
        {
          value: PermissionCodes.PositionCreate,
          label: '创建岗位',
          icon: 'mdi:plus',
        },
        {
          value: PermissionCodes.PositionEdit,
          label: '编辑岗位',
          icon: 'mdi:pencil',
        },
        {
          value: PermissionCodes.PositionDelete,
          label: '删除岗位',
          icon: 'mdi:delete',
        },
      ],
    },
    {
      value: PermissionCodes.WorkflowManagement,
      label: '工作流管理',
      icon: 'mdi:workflow',
      children: [
        {
          value: PermissionCodes.WorkflowDefinitionView,
          label: '查看流程定义',
          icon: 'mdi:eye',
        },
        {
          value: PermissionCodes.WorkflowDefinitionCreate,
          label: '创建流程定义',
          icon: 'mdi:plus',
        },
        {
          value: PermissionCodes.WorkflowDefinitionEdit,
          label: '编辑流程定义',
          icon: 'mdi:pencil',
        },
        {
          value: PermissionCodes.WorkflowDefinitionDelete,
          label: '删除流程定义',
          icon: 'mdi:delete',
        },
        {
          value: PermissionCodes.WorkflowDefinitionPublish,
          label: '发布流程定义',
          icon: 'mdi:publish',
        },
        {
          value: PermissionCodes.WorkflowStart,
          label: '发起流程',
          icon: 'mdi:play',
        },
        {
          value: PermissionCodes.WorkflowCancel,
          label: '撤销流程',
          icon: 'mdi:stop',
        },
        {
          value: PermissionCodes.WorkflowTaskApprove,
          label: '审批任务',
          icon: 'mdi:check-decagram',
        },
        {
          value: PermissionCodes.WorkflowInstanceView,
          label: '查看流程实例',
          icon: 'mdi:eye',
        },
        {
          value: PermissionCodes.WorkflowMonitor,
          label: '流程监控',
          icon: 'mdi:monitor',
        },
      ],
    },
    {
      value: PermissionCodes.NotificationManagement,
      label: '通知管理',
      icon: 'mdi:bell',
      children: [
        {
          value: PermissionCodes.NotificationView,
          label: '查看通知',
          icon: 'mdi:eye',
        },
        {
          value: PermissionCodes.NotificationSend,
          label: '发送通知',
          icon: 'mdi:send',
        },
      ],
    },
    {
      value: PermissionCodes.ExpenseManagement,
      label: '报销管理',
      icon: 'mdi:receipt',
      children: [
        { value: PermissionCodes.ExpenseClaimView, label: '查看报销单', icon: 'mdi:eye' },
        { value: PermissionCodes.ExpenseClaimCreate, label: '创建报销单', icon: 'mdi:plus' },
        { value: PermissionCodes.ExpenseClaimSubmit, label: '提交报销单', icon: 'mdi:send' },
      ],
    },
    {
      value: PermissionCodes.MeetingManagement,
      label: '会议管理',
      icon: 'mdi:calendar-month',
      children: [
        { value: PermissionCodes.MeetingRoomView, label: '查看会议室', icon: 'mdi:eye' },
        { value: PermissionCodes.MeetingRoomEdit, label: '管理会议室', icon: 'mdi:cog' },
        { value: PermissionCodes.MeetingBookingView, label: '查看预订', icon: 'mdi:calendar' },
        { value: PermissionCodes.MeetingBookingCreate, label: '预订会议室', icon: 'mdi:plus' },
      ],
    },
    {
      value: PermissionCodes.AssetManagement,
      label: '资产管理',
      icon: 'mdi:package-variant',
      children: [
        { value: PermissionCodes.AssetView, label: '查看资产', icon: 'mdi:eye' },
        { value: PermissionCodes.AssetCreate, label: '创建资产', icon: 'mdi:plus' },
        { value: PermissionCodes.AssetEdit, label: '编辑资产', icon: 'mdi:pencil' },
        { value: PermissionCodes.AssetAllocate, label: '领用资产', icon: 'mdi:hand-extended' },
        { value: PermissionCodes.AssetReturn, label: '归还资产', icon: 'mdi:key-return' },
        { value: PermissionCodes.AssetScrap, label: '报废资产', icon: 'mdi:delete' },
        { value: PermissionCodes.AssetAllocationView, label: '查看领用记录', icon: 'mdi:format-list-bulleted' },
      ],
    },
    {
      value: PermissionCodes.VehicleManagement,
      label: '车辆管理',
      icon: 'mdi:car',
      children: [
        { value: PermissionCodes.VehicleView, label: '查看车辆', icon: 'mdi:eye' },
        { value: PermissionCodes.VehicleEdit, label: '管理车辆', icon: 'mdi:cog' },
        { value: PermissionCodes.VehicleBookingView, label: '查看预订', icon: 'mdi:calendar' },
        { value: PermissionCodes.VehicleBookingCreate, label: '预订用车', icon: 'mdi:plus' },
        { value: PermissionCodes.VehicleBookingCancel, label: '取消预订', icon: 'mdi:close' },
        { value: PermissionCodes.VehicleBookingComplete, label: '完成预订', icon: 'mdi:check' },
      ],
    },
    {
      value: PermissionCodes.ContractManagement,
      label: '合同管理',
      icon: 'mdi:file-document-edit',
      children: [
        { value: PermissionCodes.ContractView, label: '查看合同', icon: 'mdi:eye' },
        { value: PermissionCodes.ContractCreate, label: '创建合同', icon: 'mdi:plus' },
        { value: PermissionCodes.ContractEdit, label: '编辑合同', icon: 'mdi:pencil' },
        { value: PermissionCodes.ContractSubmit, label: '提交审批', icon: 'mdi:send' },
        { value: PermissionCodes.ContractApprove, label: '审批合同', icon: 'mdi:check' },
        { value: PermissionCodes.ContractArchive, label: '归档合同', icon: 'mdi:archive' },
      ],
    },
    {
      value: PermissionCodes.CustomerManagement,
      label: '客户管理',
      icon: 'mdi:account-group',
      children: [
        { value: PermissionCodes.CustomerView, label: '查看客户', icon: 'mdi:eye' },
        { value: PermissionCodes.CustomerCreate, label: '创建客户', icon: 'mdi:plus' },
        { value: PermissionCodes.CustomerEdit, label: '编辑客户', icon: 'mdi:pencil' },
        { value: PermissionCodes.CustomerDelete, label: '删除客户', icon: 'mdi:delete' },
        { value: PermissionCodes.CustomerContactEdit, label: '编辑客户联系人', icon: 'mdi:account-edit' },
        { value: PermissionCodes.CustomerReleaseToSea, label: '释放到公海', icon: 'mdi:share' },
        { value: PermissionCodes.CustomerClaimFromSea, label: '公海领用', icon: 'mdi:hand-extended' },
        { value: PermissionCodes.IndustryView, label: '查看行业', icon: 'mdi:eye' },
        { value: PermissionCodes.CustomerSourceView, label: '查看客户来源', icon: 'mdi:eye' },
        { value: PermissionCodes.CustomerSourceCreate, label: '创建客户来源', icon: 'mdi:plus' },
        { value: PermissionCodes.CustomerSourceEdit, label: '编辑客户来源', icon: 'mdi:pencil' },
      ],
    },
    {
      value: PermissionCodes.AttendanceManagement,
      label: '考勤管理',
      icon: 'mdi:clock-check',
      children: [
        { value: PermissionCodes.AttendanceRecordView, label: '查看考勤记录', icon: 'mdi:eye' },
        { value: PermissionCodes.AttendanceCheckIn, label: '打卡/签退', icon: 'mdi:clock-outline' },
        { value: PermissionCodes.ScheduleView, label: '查看排班', icon: 'mdi:calendar' },
        { value: PermissionCodes.ScheduleEdit, label: '编辑排班', icon: 'mdi:pencil' },
      ],
    },
    {
      value: PermissionCodes.AnnouncementManagement,
      label: '公告管理',
      icon: 'mdi:bullhorn',
      children: [
        { value: PermissionCodes.AnnouncementView, label: '查看公告', icon: 'mdi:eye' },
        { value: PermissionCodes.AnnouncementCreate, label: '创建公告', icon: 'mdi:plus' },
        { value: PermissionCodes.AnnouncementEdit, label: '编辑公告', icon: 'mdi:pencil' },
        { value: PermissionCodes.AnnouncementPublish, label: '发布公告', icon: 'mdi:send' },
      ],
    },
    {
      value: PermissionCodes.TaskManagement,
      label: '任务管理',
      icon: 'mdi:clipboard-list',
      children: [
        { value: PermissionCodes.ProjectView, label: '查看项目', icon: 'mdi:eye' },
        { value: PermissionCodes.ProjectCreate, label: '创建项目', icon: 'mdi:plus' },
        { value: PermissionCodes.ProjectEdit, label: '编辑项目', icon: 'mdi:pencil' },
        { value: PermissionCodes.TaskView, label: '查看任务', icon: 'mdi:eye' },
        { value: PermissionCodes.TaskCreate, label: '创建任务', icon: 'mdi:plus' },
        { value: PermissionCodes.TaskEdit, label: '编辑任务', icon: 'mdi:pencil' },
      ],
    },
    {
      value: PermissionCodes.ChatManagement,
      label: '即时通讯',
      icon: 'mdi:message-text',
      children: [
        { value: PermissionCodes.ChatView, label: '查看会话与消息', icon: 'mdi:eye' },
        { value: PermissionCodes.ChatCreate, label: '创建会话', icon: 'mdi:plus' },
      ],
    },
    {
      value: PermissionCodes.DocumentManagement,
      label: '文档管理',
      icon: 'mdi:file-document-multiple',
      children: [
        { value: PermissionCodes.DocumentView, label: '查看文档', icon: 'mdi:eye' },
        { value: PermissionCodes.DocumentCreate, label: '上传文档', icon: 'mdi:upload' },
        { value: PermissionCodes.DocumentEdit, label: '编辑文档', icon: 'mdi:pencil' },
        { value: PermissionCodes.DocumentShare, label: '创建共享链接', icon: 'mdi:link' },
      ],
    },
    {
      value: PermissionCodes.ContactManagement,
      label: '通讯录管理',
      icon: 'mdi:card-account-details',
      children: [
        { value: PermissionCodes.ContactGroupView, label: '查看联系组', icon: 'mdi:eye' },
        { value: PermissionCodes.ContactGroupCreate, label: '创建联系组', icon: 'mdi:plus' },
        { value: PermissionCodes.ContactGroupEdit, label: '编辑联系组', icon: 'mdi:pencil' },
        { value: PermissionCodes.ContactView, label: '查看联系人', icon: 'mdi:eye' },
        { value: PermissionCodes.ContactCreate, label: '创建联系人', icon: 'mdi:plus' },
        { value: PermissionCodes.ContactEdit, label: '编辑联系人', icon: 'mdi:pencil' },
      ],
    },
    {
      value: PermissionCodes.LeaveManagement,
      label: '请假管理',
      icon: 'mdi:calendar-account',
      children: [
        {
          value: PermissionCodes.LeaveRequestView,
          label: '查看请假申请',
          icon: 'mdi:eye',
        },
        {
          value: PermissionCodes.LeaveRequestCreate,
          label: '创建请假申请',
          icon: 'mdi:plus',
        },
        {
          value: PermissionCodes.LeaveRequestEdit,
          label: '编辑请假申请',
          icon: 'mdi:pencil',
        },
        {
          value: PermissionCodes.LeaveRequestSubmit,
          label: '提交请假申请',
          icon: 'mdi:send',
        },
        {
          value: PermissionCodes.LeaveRequestCancel,
          label: '撤销请假申请',
          icon: 'mdi:close',
        },
        {
          value: PermissionCodes.LeaveBalanceView,
          label: '查看请假余额',
          icon: 'mdi:eye',
        },
        {
          value: PermissionCodes.LeaveBalanceEdit,
          label: '设置请假余额',
          icon: 'mdi:counter',
        },
      ],
    },
    {
      value: PermissionCodes.AllApiAccess,
      label: '所有接口访问权限',
      icon: 'mdi:shield-check',
    },
  ];
}

/**
 * 获取所有权限码（扁平列表）
 */
export function getAllPermissionCodes(): string[] {
  const tree = buildPermissionTree();
  const codes: string[] = [];

  function traverse(nodes: PermissionTreeNode[]) {
    for (const node of nodes) {
      codes.push(node.value);
      if (node.children) {
        traverse(node.children);
      }
    }
  }

  traverse(tree);
  return codes;
}
