import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { IndustryApi } from '#/api/system/industry';

import { z } from '#/adapter/form';
import { $t } from '#/locales';

/** 树节点（带 children），用于表格树形展示 */
export type IndustryTreeItem = IndustryApi.IndustryItem & {
  children?: IndustryTreeItem[];
};

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
  parentOptions: { label: string; value: string }[] = [],
): VbenFormSchema[] {
  const parentSelectOptions = [
    { label: $t('customer.topLevel'), value: '' },
    ...parentOptions,
  ];
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'name',
      label: $t('customer.industryName'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('customer.industryName')])),
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        class: 'w-full',
        options: parentSelectOptions,
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
      component: 'Input',
      componentProps: { class: 'w-full' },
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
