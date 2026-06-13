import type { DataNode } from 'ant-design-vue/es/tree';

import { PermissionCodes } from '#/constants/permission-codes';
import {
  MODULE_CATEGORY_DEFINITIONS,
  MODULE_TOP_LEVEL_PERMISSION_ROOTS,
  PERMISSION_ROOT_CATEGORY_MAP,
  SYSTEM_GROUP_VALUE_PREFIX,
  isSyntheticPermissionTreeKey,
  type ModuleCategoryId,
} from '#/constants/module-menu-categories';

export interface PermissionTreeNode extends DataNode {
  value: string;
  label: string;
  icon?: string;
  hint?: string;
  nodeType?: 'group' | 'permission';
  children?: PermissionTreeNode[];
}

type PermissionTreeNodeInput = Omit<PermissionTreeNode, 'key' | 'children'> & {
  children?: PermissionTreeNodeInput[];
};

function ensureTreeKeys(
  nodes: PermissionTreeNodeInput[],
): PermissionTreeNode[] {
  return nodes.map(
    (node) =>
      ({
        ...node,
        key: node.value,
        children: node.children ? ensureTreeKeys(node.children) : undefined,
      }) as PermissionTreeNode,
  );
}

type PermissionPageGroupDefinition = {
  codes: string[];
  icon: string;
  hint?: string;
  key: string;
  label: string;
};

const PERMISSION_PAGE_GROUP_VALUE_PREFIX = `${SYSTEM_GROUP_VALUE_PREFIX}page:`;

const PERMISSION_PAGE_GROUPS: Record<string, PermissionPageGroupDefinition[]> = {
  [PermissionCodes.WorkflowManagement]: [
    {
      key: 'task',
      label: '任务中心',
      icon: 'mdi:clipboard-text-clock',
      hint: '我的待办 / 我的已办 / 我发起的',
      codes: [
        PermissionCodes.WorkflowInstanceView,
        PermissionCodes.WorkflowTaskApprove,
        PermissionCodes.WorkflowStart,
        PermissionCodes.WorkflowCancel,
      ],
    },
    {
      key: 'definition',
      label: '流程定义',
      icon: 'mdi:file-tree',
      hint: '流程定义列表',
      codes: [
        PermissionCodes.WorkflowDefinitionView,
        PermissionCodes.WorkflowDefinitionCreate,
        PermissionCodes.WorkflowDefinitionEdit,
        PermissionCodes.WorkflowDefinitionPublish,
        PermissionCodes.WorkflowDefinitionDelete,
        PermissionCodes.WorkflowDefinitionDeletePublished,
      ],
    },
    {
      key: 'monitor',
      label: '流程监控',
      icon: 'mdi:monitor-dashboard',
      hint: '流程实例监控',
      codes: [PermissionCodes.WorkflowMonitor],
    },
  ],
};

function groupPermissionTreePages(
  nodes: PermissionTreeNodeInput[],
): PermissionTreeNodeInput[] {
  return nodes.map((node) => {
    const children = node.children ? groupPermissionTreePages(node.children) : undefined;
    const pageGroups = PERMISSION_PAGE_GROUPS[node.value];
    if (!children || !pageGroups) {
      return { ...node, children };
    }

    const childMap = new Map(children.map((child) => [child.value, child]));
    const groupedCodes = new Set<string>();
    const groupedChildren: PermissionTreeNodeInput[] = [];

    for (const group of pageGroups) {
      const items = group.codes
        .map((code) => childMap.get(code))
        .filter((child): child is PermissionTreeNodeInput => !!child);
      if (items.length === 0) continue;
      group.codes.forEach((code) => groupedCodes.add(code));
      groupedChildren.push({
        value: `${PERMISSION_PAGE_GROUP_VALUE_PREFIX}${node.value}:${group.key}`,
        label: group.label,
        icon: group.icon,
        hint: group.hint,
        nodeType: 'group',
        children: items,
      });
    }

    const ungroupedChildren = children.filter((child) => !groupedCodes.has(child.value));
    return {
      ...node,
      children: [...groupedChildren, ...ungroupedChildren],
    };
  });
}

export function buildPermissionTree(): PermissionTreeNode[] {
  const tree: PermissionTreeNodeInput[] = [
    {
      value: PermissionCodes.UserManagement,
      label: '用户管理',
      icon: 'mdi:account',
      children: [
        { value: PermissionCodes.UserView, label: '查看用户', icon: 'mdi:eye' },
        { value: PermissionCodes.UserCreate, label: '创建用户', icon: 'mdi:account-plus' },
        { value: PermissionCodes.UserEdit, label: '编辑用户', icon: 'mdi:account-edit' },
        { value: PermissionCodes.UserDelete, label: '删除用户', icon: 'mdi:account-remove' },
        { value: PermissionCodes.UserRoleAssign, label: '分配用户角色', icon: 'mdi:account-group' },
        { value: PermissionCodes.UserResetPassword, label: '重置用户密码', icon: 'mdi:lock-reset' },
        { value: PermissionCodes.UserExport, label: '导出用户', icon: 'mdi:file-excel' },
        { value: PermissionCodes.UserImport, label: '导入用户', icon: 'mdi:upload' },
        { value: PermissionCodes.UserChangeHistoryView, label: '用户修改记录', icon: 'mdi:history' },
      ],
    },
    {
      value: PermissionCodes.RoleManagement,
      label: '角色管理',
      icon: 'mdi:account-group',
      children: [
        { value: PermissionCodes.RoleView, label: '查看角色', icon: 'mdi:eye' },
        { value: PermissionCodes.RoleCreate, label: '创建角色', icon: 'mdi:account-plus' },
        { value: PermissionCodes.RoleEdit, label: '编辑角色', icon: 'mdi:account-edit' },
        { value: PermissionCodes.RoleDelete, label: '删除角色', icon: 'mdi:account-remove' },
        { value: PermissionCodes.RoleUpdatePermissions, label: '更新角色权限', icon: 'mdi:shield-edit' },
      ],
    },
    {
      value: PermissionCodes.DeptManagement,
      label: '部门管理',
      icon: 'charm:organisation',
      children: [
        { value: PermissionCodes.DeptView, label: '查看部门', icon: 'mdi:eye' },
        { value: PermissionCodes.DeptCreate, label: '创建部门', icon: 'mdi:account-plus' },
        { value: PermissionCodes.DeptEdit, label: '编辑部门', icon: 'mdi:account-edit' },
        { value: PermissionCodes.DeptDelete, label: '删除部门', icon: 'mdi:account-remove' },
      ],
    },
    {
      value: PermissionCodes.PositionManagement,
      label: '岗位管理',
      icon: 'mdi:briefcase',
      children: [
        { value: PermissionCodes.PositionView, label: '查看岗位', icon: 'mdi:eye' },
        { value: PermissionCodes.PositionCreate, label: '创建岗位', icon: 'mdi:plus' },
        { value: PermissionCodes.PositionEdit, label: '编辑岗位', icon: 'mdi:pencil' },
        { value: PermissionCodes.PositionDelete, label: '删除岗位', icon: 'mdi:delete' },
      ],
    },
    {
      value: PermissionCodes.WorkflowManagement,
      label: '工作流管理',
      icon: 'mdi:workflow',
      children: [
        { value: PermissionCodes.WorkflowDefinitionView, label: '查看流程定义', icon: 'mdi:eye' },
        { value: PermissionCodes.WorkflowDefinitionCreate, label: '创建流程定义', icon: 'mdi:plus' },
        { value: PermissionCodes.WorkflowDefinitionEdit, label: '编辑流程定义', icon: 'mdi:pencil' },
        { value: PermissionCodes.WorkflowDefinitionDelete, label: '删除流程定义', icon: 'mdi:delete' },
        { value: PermissionCodes.WorkflowDefinitionDeletePublished, label: '删除已发布流程定义', icon: 'mdi:delete-alert' },
        { value: PermissionCodes.WorkflowDefinitionPublish, label: '发布流程定义', icon: 'mdi:publish' },
        { value: PermissionCodes.WorkflowStart, label: '发起流程', icon: 'mdi:play' },
        { value: PermissionCodes.WorkflowCancel, label: '撤销流程', icon: 'mdi:stop' },
        { value: PermissionCodes.WorkflowTaskApprove, label: '审批任务', icon: 'mdi:check-decagram' },
        { value: PermissionCodes.WorkflowInstanceView, label: '查看流程实例', icon: 'mdi:eye' },
        { value: PermissionCodes.WorkflowMonitor, label: '流程监控', icon: 'mdi:monitor' },
      ],
    },
    {
      value: PermissionCodes.NotificationManagement,
      label: '通知管理',
      icon: 'mdi:bell',
      children: [
        { value: PermissionCodes.NotificationView, label: '查看通知', icon: 'mdi:eye' },
        { value: PermissionCodes.NotificationSend, label: '发送通知', icon: 'mdi:send' },
      ],
    },
    {
      value: PermissionCodes.OperationLogManagement,
      label: '操作日志',
      icon: 'mdi:history',
      children: [
        { value: PermissionCodes.OperationLogView, label: '查看操作日志', icon: 'mdi:eye' },
      ],
    },
    {
      value: PermissionCodes.SystemLogManagement,
      label: '系统日志',
      icon: 'mdi:alert-circle-outline',
      children: [
        { value: PermissionCodes.SystemLogView, label: '查看系统日志', icon: 'mdi:eye' },
      ],
    },
    {
      value: PermissionCodes.BackgroundJobManagement,
      label: '后台任务',
      icon: 'mdi:timer-cog-outline',
      children: [
        { value: PermissionCodes.BackgroundJobView, label: '查看后台任务', icon: 'mdi:eye' },
        { value: PermissionCodes.BackgroundJobTrigger, label: '触发后台任务', icon: 'mdi:cog-play-outline' },
      ],
    },
    {
      value: PermissionCodes.HomeDashboard,
      label: '首页工作台',
      icon: 'mdi:view-dashboard-outline',
    },
    {
      value: PermissionCodes.CommonDataAccess,
      label: '公共基础数据',
      icon: 'mdi:database-search',
      hint: '业务页面下拉/附件等公共接口，与菜单权限解耦',
      children: [
        { value: PermissionCodes.RoleOptionView, label: '角色下拉查询', icon: 'mdi:account-group-outline' },
        { value: PermissionCodes.UserOptionView, label: '用户下拉查询', icon: 'mdi:account-outline' },
        { value: PermissionCodes.DeptOptionView, label: '部门树查询', icon: 'charm:organisation' },
        { value: PermissionCodes.PositionOptionView, label: '岗位下拉查询', icon: 'mdi:briefcase-outline' },
        { value: PermissionCodes.FileAccess, label: '通用文件访问', icon: 'mdi:file-multiple' },
      ],
    },
    {
      value: PermissionCodes.AllApiAccess,
      label: '所有接口访问权限',
      icon: 'mdi:shield-check',
      hint: '超级管理员全局兜底，勿授予普通业务角色',
    },
  ];
  return groupPermissionTreeByModuleCategory(ensureTreeKeys(groupPermissionTreePages(tree)));
}

function groupPermissionTreeByModuleCategory(
  nodes: PermissionTreeNode[],
): PermissionTreeNode[] {
  const globalNodes: PermissionTreeNode[] = [];
  const topLevelSiblingNodes: PermissionTreeNode[] = [];
  const systemNodes: PermissionTreeNode[] = [];

  for (const node of nodes) {
    if (
      node.value === PermissionCodes.AllApiAccess
      || node.value === PermissionCodes.CommonDataAccess
    ) {
      globalNodes.push(node);
      continue;
    }

    const category = PERMISSION_ROOT_CATEGORY_MAP[
      node.value as keyof typeof PERMISSION_ROOT_CATEGORY_MAP
    ] as ModuleCategoryId | undefined;

    if (
      MODULE_TOP_LEVEL_PERMISSION_ROOTS.includes(
        node.value as (typeof MODULE_TOP_LEVEL_PERMISSION_ROOTS)[number],
      )
    ) {
      topLevelSiblingNodes.push(node);
      continue;
    }

    if (!category) {
      globalNodes.push(node);
      continue;
    }

    if (category === 'system') {
      systemNodes.push(node);
    }
  }

  const groupedCategories: PermissionTreeNode[] = [];

  for (const definition of MODULE_CATEGORY_DEFINITIONS) {
    const children = definition.id === 'system' ? systemNodes : [];
    if (children.length === 0) continue;

    groupedCategories.push({
      key: definition.permissionCode,
      value: definition.permissionCode,
      label: definition.title,
      icon: definition.icon,
      children,
    });
  }

  return [...topLevelSiblingNodes, ...groupedCategories, ...globalNodes];
}

export function getAllPermissionCodes(): string[] {
  const tree = buildPermissionTree();
  const codes: string[] = [];

  function traverse(nodes: PermissionTreeNode[]) {
    for (const node of nodes) {
      if (!isSyntheticPermissionTreeKey(node.value)) {
        codes.push(node.value);
      }
      if (node.children) {
        traverse(node.children);
      }
    }
  }

  traverse(tree);
  return codes;
}

export function expandLegacyPermissionSelection(codes: readonly string[]): string[] {
  return [...new Set(codes)];
}

export function enrichPermissionTreeSelection(
  selectedCodes: readonly string[],
  tree: readonly PermissionTreeNode[],
): string[] {
  const selected = new Set(selectedCodes);
  let changed = true;

  while (changed) {
    changed = false;
    const visit = (nodes: readonly PermissionTreeNode[]) => {
      for (const node of nodes) {
        const children = node.children ?? [];
        if (children.length === 0) continue;
        visit(children);
        const allChildrenSelected = children.every((child) =>
          selected.has(child.value),
        );
        if (allChildrenSelected && !selected.has(node.value)) {
          selected.add(node.value);
          changed = true;
        }
      }
    };
    visit(tree);
  }

  return [...selected];
}
