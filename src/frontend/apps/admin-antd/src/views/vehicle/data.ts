import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { VehicleApi } from '#/api/system/vehicle';

import { z } from '#/adapter/form';
import { $t } from '#/locales';

const STATUS_OPTIONS = [
  { label: () => $t('vehicle.statusAvailable'), value: 0 },
  { label: () => $t('vehicle.statusDisabled'), value: 1 },
];

function statusLabel(value: number) {
  const map: Record<number, string> = {
    0: 'vehicle.statusAvailable',
    1: 'vehicle.statusDisabled',
  };
  return $t(map[value] ?? '');
}

export function useSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'plateNumber',
      label: $t('vehicle.plateNumber'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('vehicle.plateNumber')])),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'model',
      label: $t('vehicle.model'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('vehicle.model')])),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'remark',
      label: $t('vehicle.remark'),
    },
  ];
}

export function useGridFormSchema(): VbenFormSchema[] {
  return [
    { component: 'Input', componentProps: { class: 'w-full' }, fieldName: 'plateNumber', label: $t('vehicle.plateNumber') },
    {
      component: 'Select',
      componentProps: { allowClear: true, options: STATUS_OPTIONS, class: 'w-full' },
      fieldName: 'status',
      label: $t('vehicle.status'),
    },
  ];
}

export function useColumns(
  onActionClick?: OnActionClickFn<VehicleApi.VehicleItem>,
): VxeTableGridOptions<VehicleApi.VehicleItem>['columns'] {
  return [
    { field: 'plateNumber', title: $t('vehicle.plateNumber'), width: 120 },
    { field: 'model', title: $t('vehicle.model'), minWidth: 120 },
    {
      formatter: ({ cellValue }) => statusLabel(cellValue as number),
      field: 'status',
      title: $t('vehicle.status'),
      width: 90,
    },
    {
      formatter: 'formatDateTime',
      field: 'createdAt',
      title: $t('vehicle.createdAt'),
      width: 170,
    },
    {
      align: 'right',
      cellRender: {
        attrs: { nameField: 'plateNumber', nameTitle: $t('vehicle.plateNumber'), onClick: onActionClick },
        name: 'CellOperation',
        options: [
          { code: 'edit', text: $t('vehicle.edit'), show: () => true },
        ],
      },
      field: 'operation',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('vehicle.operation'),
      width: 100,
    },
  ];
}

export { statusLabel, STATUS_OPTIONS };
