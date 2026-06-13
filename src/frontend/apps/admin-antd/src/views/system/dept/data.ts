import type { VxeTableGridOptions } from '@vben/plugins/vxe-table';

import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn } from '#/adapter/vxe-table';
import type { SystemDeptApi } from '#/api/system/dept';

import { z } from '#/adapter/form';
import { getDeptTree } from '#/api/system/dept';
import { getUserList } from '#/api/system/user';
import { $t } from '#/locales';

/** 将部门 ID 规范为字符串（雪花 ID 禁止转 number，避免精度丢失） */
export function normalizeDeptId(id?: string | number | null) {
  if (id === undefined || id === null || id === '') {
    return '';
  }
  return String(id);
}

/** 将 parentId 规范为 vxe 树表 transform 模式可识别的值（顶级为 null） */
export function normalizeDeptParentId(parentId?: string | number | null) {
  if (
    parentId === undefined ||
    parentId === null ||
    parentId === '' ||
    parentId === '0' ||
    parentId === 0
  ) {
    return null;
  }
  return String(parentId);
}

/** 将部门树铺平为带 parentId 的列表，供 vxe 树表 transform 模式使用 */
export function flattenDeptTree(
  nodes: SystemDeptApi.SystemDept[],
  parentId: string | null = null,
): SystemDeptApi.SystemDept[] {
  const result: SystemDeptApi.SystemDept[] = [];
  for (const node of nodes) {
    const { children, ...rest } = node;
    const resolvedParentId =
      parentId ?? normalizeDeptParentId(node.parentId as string | undefined);
    result.push({
      ...rest,
      id: normalizeDeptId(node.id),
      parentId: resolvedParentId ?? undefined,
    });
    if (children?.length) {
      result.push(...flattenDeptTree(children, normalizeDeptId(node.id)));
    }
  }
  return result;
}

/** 从铺平后的表格数据中提取同级部门的当前顺序 */
export function getSiblingOrderedIds(
  rows: SystemDeptApi.SystemDept[],
  parentId: string | null,
) {
  const normalizedParentId = normalizeDeptParentId(parentId);
  return rows
    .map((row, index) => ({ row, index }))
    .filter(({ row }) => normalizeDeptParentId(row.parentId) === normalizedParentId)
    .sort((a, b) => a.index - b.index)
    .map(({ row }) => normalizeDeptId(row.id));
}

/** 根据拖拽目标位置计算同级部门的新顺序 */
export function buildSiblingOrderAfterDrag(
  before: string[],
  dragId: string,
  targetId: string,
  dragPos: 'bottom' | 'top' = 'top',
) {
  if (!before.length || dragId === targetId) {
    return null;
  }
  const next = before.filter((id) => id !== dragId);
  let targetIndex = next.indexOf(targetId);
  if (targetIndex < 0) {
    return null;
  }
  if (dragPos === 'bottom') {
    targetIndex += 1;
  }
  next.splice(targetIndex, 0, dragId);
  return next;
}

/**
 * 从部门树中排除指定节点及其所有子节点（编辑时禁止选自己或下级为上级）
 */
function filterDeptTreeExcluding(
  nodes: SystemDeptApi.SystemDept[],
  excludeId?: string,
): SystemDeptApi.SystemDept[] {
  if (!excludeId) return nodes;
  return nodes.reduce<SystemDeptApi.SystemDept[]>((result, node) => {
      if (String(node.id) === String(excludeId)) return result;
      result.push({
        ...node,
        children: node.children?.length
          ? filterDeptTreeExcluding(node.children, excludeId)
          : undefined,
      });
      return result;
    }, []);
}

/**
 * 获取编辑表单的字段配置。如果没有使用多语言，可以直接export一个数组常量
 * @param getExcludeDeptId 编辑时传入当前部门 id 的 getter，用于从上级部门选项中排除自身及其子部门
 */
export function useSchema(
  getExcludeDeptId?: () => string | undefined,
): VbenFormSchema[] {
  async function getUserOptions() {
    const res = await getUserList({ pageIndex: 1, pageSize: 1000, countTotal: false, isResigned: false });
    return (res.items ?? []).map((u) => ({
      label: u.realName || u.name,
      value: String(u.userId),
    }));
  }

  return [
    {
      component: 'Input',
      fieldName: 'name',
      label: $t('system.dept.deptName'),
      rules: z
        .string()
        .min(2, $t('ui.formRules.minLength', [$t('system.dept.deptName'), 2]))
        .max(
          20,
          $t('ui.formRules.maxLength', [$t('system.dept.deptName'), 20]),
        ),
    },
    {
      component: 'ApiTreeSelect',
      componentProps: {
        allowClear: true,
        api: async () => {
          const tree = await getDeptTree();
          return filterDeptTreeExcluding(tree, getExcludeDeptId?.());
        },
        class: 'w-full',
        labelField: 'name',
        valueField: 'id',
        childrenField: 'children',
      },
      fieldName: 'parentId',
      label: $t('system.dept.parentDept'),
    },
    {
      component: 'RadioGroup',
      componentProps: {
        buttonStyle: 'solid',
        options: [
          { label: $t('common.enabled'), value: 1 },
          { label: $t('common.disabled'), value: 0 },
        ],
        optionType: 'button',
      },
      defaultValue: 1,
      fieldName: 'status',
      label: $t('system.dept.status'),
    },
    {
      component: 'ApiSelect',
      componentProps: {
        allowClear: true,
        api: getUserOptions,
        class: 'w-full',
        labelField: 'label',
        mode: 'multiple',
        optionFilterProp: 'label',
        showSearch: true,
        valueField: 'value',
      },
      fieldName: 'responsibleUserIds',
      label: $t('system.dept.responsibleUsers'),
    },
    {
      component: 'ApiSelect',
      componentProps: {
        allowClear: true,
        api: getUserOptions,
        class: 'w-full',
        labelField: 'label',
        optionFilterProp: 'label',
        showSearch: true,
        valueField: 'value',
      },
      fieldName: 'defaultResponsibleUserId',
      label: $t('system.dept.defaultResponsibleUser'),
    },
    {
      component: 'Textarea',
      componentProps: {
        maxLength: 50,
        rows: 3,
        showCount: true,
      },
      fieldName: 'remark',
      label: $t('system.dept.remark'),
      rules: z
        .string()
        .max(50, $t('ui.formRules.maxLength', [$t('system.dept.remark'), 50]))
        .optional(),
    },
  ];
}

/**
 * 按名称/备注、状态筛选部门树；命中子节点时保留祖先路径
 */
export function filterDeptTree(
  nodes: SystemDeptApi.SystemDept[],
  filters?: { name?: string; status?: number | string | null },
): SystemDeptApi.SystemDept[] {
  const kw =
    typeof filters?.name === 'string' ? filters.name.trim().toLowerCase() : '';
  const statusRaw = filters?.status;
  const statusFilter =
    statusRaw === undefined || statusRaw === null || statusRaw === ''
      ? undefined
      : Number(statusRaw);

  function nodeMatches(node: SystemDeptApi.SystemDept): boolean {
    const nameOk =
      !kw ||
      node.name.toLowerCase().includes(kw) ||
      (node.remark?.toLowerCase().includes(kw) ?? false);
    const statusOk =
      statusFilter === undefined || node.status === statusFilter;
    return nameOk && statusOk;
  }

  function filterNode(
    node: SystemDeptApi.SystemDept,
  ): SystemDeptApi.SystemDept | null {
    const children = (node.children ?? [])
      .map(filterNode)
      .filter((n): n is SystemDeptApi.SystemDept => n != null);
    if (nodeMatches(node) || children.length > 0) {
      return {
        ...node,
        children: children.length > 0 ? children : undefined,
      };
    }
    return null;
  }

  return nodes.reduce<SystemDeptApi.SystemDept[]>((result, node) => {
    const filtered = filterNode(node);
    if (filtered) result.push(filtered);
    return result;
  }, []);
}

/**
 * 列表顶部搜索表单
 */
export function useGridFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: {
        allowClear: false,
        class: 'w-full',
      },
      fieldName: 'name',
      label: $t('system.dept.deptName'),
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        class: 'w-full',
        options: [
          { label: $t('common.enabled'), value: 1 },
          { label: $t('common.disabled'), value: 0 },
        ],
      },
      fieldName: 'status',
      label: $t('system.dept.status'),
    },
  ];
}

/**
 * 获取表格列配置
 * @description 使用函数的形式返回列数据而不是直接export一个Array常量，是为了响应语言切换时重新翻译表头
 * @param onActionClick 表格操作按钮点击事件
 */
export function useColumns(
  onActionClick?: OnActionClickFn<SystemDeptApi.SystemDept>,
  options?: { dragSort?: boolean },
): VxeTableGridOptions<SystemDeptApi.SystemDept>['columns'] {
  return [
    {
      align: 'left',
      dragSort: options?.dragSort,
      field: 'name',
      fixed: 'left',
      title: $t('system.dept.deptName'),
      treeNode: true,
      width: options?.dragSort ? 180 : 150,
    },
    {
      cellRender: { name: 'CellTag' },
      field: 'status',
      title: $t('system.dept.status'),
      width: 100,
    },
    {
      field: 'responsibleUsers',
      formatter: ({ row }) =>
        row.responsibleUsers?.map((x: SystemDeptApi.DeptResponsibleUser) => x.name).filter(Boolean).join('、') || '—',
      title: $t('system.dept.responsibleUsers'),
      width: 180,
    },
    {
      formatter: 'formatDateTime',
      field: 'createdAt',
      title: $t('system.dept.createTime'),
      width: 180,
    },
    {
      field: 'remark',
      title: $t('system.dept.remark'),
    },
    { field: '_flex', minWidth: 1, title: '' },
    {
      align: 'right',
      cellRender: {
        attrs: {
          nameField: 'name',
          nameTitle: $t('system.dept.name'),
          onClick: onActionClick,
        },
        name: 'CellOperation',
        options: [
          {
            code: 'append',
            text: '新增下级',
          },
          'edit', // 默认的编辑按钮
          {
            code: 'delete', // 默认的删除按钮
            disabled: (row: SystemDeptApi.SystemDept) => {
              return !!(row.children && row.children.length > 0);
            },
          },
        ],
      },
      field: 'operation',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('system.dept.operation'),
      width: 200,
    },
  ];
}
