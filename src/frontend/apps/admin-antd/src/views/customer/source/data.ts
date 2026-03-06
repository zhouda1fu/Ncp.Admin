import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { CustomerSourceApi } from '#/api/system/customerSource';

import { z } from '#/adapter/form';
import { $t } from '#/locales';

const usageSceneOptions = [
  { label: '公海', value: 0 },
  { label: '客户列表', value: 1 },
  { label: '通用', value: 2 },
];

/** 列表筛选项：全部 / 公海 / 客户列表（对应接口 scene 参数） */
export const sceneFilterOptions = [
  { label: '全部', value: '' },
  { label: '公海', value: 'sea' },
  { label: '客户列表', value: 'list' },
];

export function useSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'name',
      label: $t('customer.sourceName'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('customer.sourceName')])),
    },
    {
      component: 'Select',
      componentProps: { class: 'w-full', options: usageSceneOptions },
      defaultValue: 2,
      fieldName: 'usageScene',
      label: '使用场景',
    },
    {
      component: 'InputNumber',
      componentProps: { class: 'w-full', min: 0 },
      defaultValue: 0,
      fieldName: 'sortOrder',
      label: $t('customer.sortOrder'),
    },
  ];
}

export function useGridFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'name',
      label: $t('customer.sourceName'),
    },
    {
      component: 'Select',
      componentProps: { class: 'w-full', options: sceneFilterOptions, allowClear: true },
      fieldName: 'scene',
      label: '使用场景',
    },
  ];
}

export function useColumns(
  onActionClick?: OnActionClickFn<CustomerSourceApi.CustomerSourceItem>,
): VxeTableGridOptions<CustomerSourceApi.CustomerSourceItem>['columns'] {
  return [
    { field: 'name', title: $t('customer.sourceName'), minWidth: 160 },
    {
      field: 'usageScene',
      title: '使用场景',
      width: 100,
      formatter: ({ cellValue, row }) => {
        const r = row as unknown as Record<string, unknown>;
        const v = (cellValue ?? r?.usageScene ?? r?.UsageScene) as number | undefined;
        const o = usageSceneOptions.find((x) => x.value === v);
        return o?.label ?? '';
      },
    },
    { field: 'sortOrder', title: $t('customer.sortOrder'), width: 100 },
    {
      align: 'right',
      cellRender: {
        attrs: {
          nameField: 'name',
          nameTitle: $t('customer.sourceName'),
          onClick: onActionClick,
        },
        name: 'CellOperation',
        options: [{ code: 'edit', text: $t('customer.edit') }],
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
