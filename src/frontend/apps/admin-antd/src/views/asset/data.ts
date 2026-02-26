import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { AssetApi } from '#/api/system/asset';

import { z } from '#/adapter/form';
import { $t } from '#/locales';

const STATUS_OPTIONS = [
  { label: () => $t('asset.statusAvailable'), value: 0 },
  { label: () => $t('asset.statusAllocated'), value: 1 },
  { label: () => $t('asset.statusScrapped'), value: 2 },
];

function statusLabel(value: number) {
  const map: Record<number, string> = {
    0: 'asset.statusAvailable',
    1: 'asset.statusAllocated',
    2: 'asset.statusScrapped',
  };
  return $t(map[value] ?? '');
}

export function useSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'name',
      label: $t('asset.name'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('asset.name')])),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'category',
      label: $t('asset.category'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('asset.category')])),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'code',
      label: $t('asset.code'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('asset.code')])),
    },
    {
      component: 'DatePicker',
      componentProps: { class: 'w-full', valueFormat: 'YYYY-MM-DD' },
      fieldName: 'purchaseDate',
      label: $t('asset.purchaseDate'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('asset.purchaseDate')])),
    },
    {
      component: 'InputNumber',
      componentProps: { min: 0, class: 'w-full' },
      fieldName: 'value',
      label: $t('asset.value'),
      rules: z.number().min(0),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'remark',
      label: $t('asset.remark'),
    },
  ];
}

export function useGridFormSchema(): VbenFormSchema[] {
  return [
    { component: 'Input', componentProps: { class: 'w-full' }, fieldName: 'code', label: $t('asset.code') },
    { component: 'Input', componentProps: { class: 'w-full' }, fieldName: 'name', label: $t('asset.name') },
    {
      component: 'Select',
      componentProps: { allowClear: true, options: STATUS_OPTIONS, class: 'w-full' },
      fieldName: 'status',
      label: $t('asset.status'),
    },
  ];
}

export function useColumns(
  onActionClick?: OnActionClickFn<AssetApi.AssetItem>,
): VxeTableGridOptions<AssetApi.AssetItem>['columns'] {
  return [
    { field: 'code', title: $t('asset.code'), width: 120 },
    { field: 'name', title: $t('asset.name'), minWidth: 120 },
    { field: 'category', title: $t('asset.category'), width: 100 },
    {
      field: 'value',
      title: $t('asset.value'),
      width: 110,
      formatter: ({ cellValue }) => (cellValue != null ? Number(cellValue).toLocaleString() : '-'),
    },
    {
      formatter: ({ cellValue }) => statusLabel(cellValue as number),
      field: 'status',
      title: $t('asset.status'),
      width: 90,
    },
    {
      formatter: 'formatDateTime',
      field: 'createdAt',
      title: $t('asset.createdAt'),
      width: 170,
    },
    {
      align: 'right',
      cellRender: {
        attrs: { nameField: 'name', nameTitle: $t('asset.name'), onClick: onActionClick },
        name: 'CellOperation',
        options: [
          { code: 'edit', text: $t('asset.edit'), show: (row: AssetApi.AssetItem) => row.status === 0 },
          { code: 'allocate', text: $t('asset.allocate'), show: (row: AssetApi.AssetItem) => row.status === 0 },
          { code: 'scrap', text: $t('asset.scrap'), show: (row: AssetApi.AssetItem) => row.status === 0 || row.status === 1 },
        ],
      },
      field: 'operation',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('asset.operation'),
      width: 200,
    },
  ];
}

export { statusLabel, STATUS_OPTIONS };
