import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { OrderLogisticsMethodApi } from '#/api/system/order-logistics-method';

import { z } from '#/adapter/form';
import { $t } from '#/locales';

export function useSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'name',
      label: $t('order.logisticsMethodName'),
      rules: z
        .string()
        .min(1, $t('ui.formRules.required', [$t('order.logisticsMethodName')])),
    },
    {
      component: 'InputNumber',
      componentProps: { class: 'w-full', min: 0 },
      defaultValue: 0,
      fieldName: 'typeValue',
      label: $t('order.logisticsMethodTypeValue'),
    },
    {
      component: 'InputNumber',
      componentProps: { class: 'w-full', min: 0 },
      defaultValue: 0,
      fieldName: 'sort',
      label: $t('order.sortOrder'),
    },
  ];
}

export function useGridFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'name',
      label: $t('order.logisticsMethodName'),
    },
  ];
}

export function useColumns(
  onActionClick?: OnActionClickFn<OrderLogisticsMethodApi.OrderLogisticsMethodItem>,
): VxeTableGridOptions<OrderLogisticsMethodApi.OrderLogisticsMethodItem>['columns'] {
  return [
    { field: 'name', title: $t('order.logisticsMethodName'), minWidth: 160 },
    { field: 'typeValue', title: $t('order.logisticsMethodTypeValue'), width: 120 },
    { field: 'sort', title: $t('order.sortOrder'), width: 100 },
    {
      align: 'right',
      cellRender: {
        attrs: {
          nameField: 'name',
          nameTitle: $t('order.logisticsMethodName'),
          onClick: onActionClick,
        },
        name: 'CellOperation',
        options: [
          { code: 'edit', text: $t('common.edit') },
          { code: 'delete', text: $t('common.delete') },
        ],
      },
      field: 'operation',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('order.operation'),
      width: 160,
    },
  ];
}
