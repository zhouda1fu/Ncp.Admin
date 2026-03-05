import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { IndustryApi } from '#/api/system/industry';

import { z } from '#/adapter/form';
import { $t } from '#/locales';

/** 树节点（带 children），用于表格树形展示 */
export type IndustryTreeItem = IndustryApi.IndustryItem & {
  children?: IndustryTreeItem[];
};

/** TreeSelect 树节点（label/value/children），用于父级行业下拉等 */
export type IndustryTreeSelectOption = {
  label: string;
  value: string;
  children?: IndustryTreeSelectOption[];
};

/** 将扁平行业列表转为 TreeSelect 树（根节点带 ▸ 前缀、按 sortOrder 排序） */
export function buildIndustryTreeForSelect(
  list: IndustryApi.IndustryItem[],
): IndustryTreeSelectOption[] {
  const sorted = [...list].sort((a, b) => a.sortOrder - b.sortOrder);
  interface Node extends IndustryTreeSelectOption {
    sortOrder: number;
    children: Node[];
  }
  const map = new Map<string, Node>();
  for (const x of sorted) {
    map.set(x.id, { label: x.name, value: x.id, sortOrder: x.sortOrder, children: [] });
  }
  const roots: Node[] = [];
  for (const x of sorted) {
    const node = map.get(x.id)!;
    const parentId = x.parentId && String(x.parentId).trim() ? x.parentId : undefined;
    if (!parentId || !map.has(parentId)) {
      roots.push(node);
    } else {
      map.get(parentId)!.children.push(node);
    }
  }
  function toOption(n: Node, isRoot: boolean): IndustryTreeSelectOption {
    const children =
      n.children.length > 0
        ? n.children
            .sort((a, b) => a.sortOrder - b.sortOrder)
            .map((c) => toOption(c, false))
        : undefined;
    const label = isRoot ? `▸ ${n.label}` : n.label;
    return { label, value: n.value, children };
  }
  return roots.sort((a, b) => a.sortOrder - b.sortOrder).map((r) => toOption(r, true));
}

/** 将平铺行业列表转为树结构（按 sortOrder 排序） */
export function flatToTree(
  flat: IndustryApi.IndustryItem[],
): IndustryTreeItem[] {
  const list = flat.map((item) => ({ ...item, children: [] as IndustryTreeItem[] }));
  const byId = new Map<string, IndustryTreeItem>();
  list.forEach((node) => byId.set(node.id, node));
  const roots: IndustryTreeItem[] = [];
  list.forEach((node) => {
    if (!node.parentId) {
      roots.push(node);
    } else {
      const parent = byId.get(node.parentId);
      if (parent?.children) parent.children.push(node);
      else roots.push(node);
    }
  });
  const sortByOrder = (nodes: IndustryTreeItem[]) =>
    nodes.sort((a, b) => a.sortOrder - b.sortOrder);
  sortByOrder(roots);
  roots.forEach((r) => r.children?.length && sortByOrder(r.children));
  return roots;
}

export function useSchema(
  industryTreeOptions: IndustryTreeSelectOption[] = [],
): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'name',
      label: $t('customer.industryName'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('customer.industryName')])),
    },
    {
      component: 'TreeSelect',
      componentProps: {
        allowClear: true,
        class: 'w-full customer-industry-parent-treeselect',
        fieldNames: { label: 'label', value: 'value', children: 'children' },
        placeholder: $t('customer.parentIndustry'),
        showSearch: true,
        treeData: industryTreeOptions,
        treeLine: true,
        treeNodeFilterProp: 'label',
      },
      fieldName: 'parentId',
      label: $t('customer.parentIndustry'),
    },
    {
      component: 'InputNumber',
      componentProps: { class: 'w-full', min: 0 },
      defaultValue: 0,
      fieldName: 'sortOrder',
      label: $t('customer.sortOrder'),
    },
    {
      component: 'Textarea',
      componentProps: {
        class: 'w-full',
        rows: 4,
        autoSize: { minRows: 4, maxRows: 8 },
      },
      fieldName: 'remark',
      label: $t('customer.remark'),
    },
  ];
}

export function useGridFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'name',
      label: $t('customer.industryName'),
    },
  ];
}

export function useColumns(
  onActionClick?: OnActionClickFn<IndustryApi.IndustryItem>,
  industryList: IndustryApi.IndustryItem[] = [],
): VxeTableGridOptions<IndustryApi.IndustryItem>['columns'] {
  return [
    { type: 'checkbox', width: 88, title: $t('customer.select') },
    {
      field: 'name',
      title: $t('customer.industryName'),
      minWidth: 200,
      treeNode: true,
    },
    {
      field: 'parentId',
      title: $t('customer.industry'),
      minWidth: 140,
      formatter: ({ row }) => {
        if (!row.parentId) return '';
        const parent = industryList.find((x) => x.id === row.parentId);
        return parent?.name ?? row.parentId;
      },
    },
    { field: 'remark', title: $t('customer.remark'), minWidth: 120 },
    {
      align: 'right',
      cellRender: {
        attrs: {
          nameField: 'name',
          nameTitle: $t('customer.industryName'),
          onClick: onActionClick,
        },
        name: 'CellOperation',
        options: [
          { code: 'edit', text: $t('customer.edit') },
          { code: 'delete', text: $t('customer.delete') },
        ],
      },
      field: 'operation',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('customer.operation'),
      width: 120,
    },
  ];
}
