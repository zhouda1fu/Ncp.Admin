import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { OrderLogisticsCompanyApi } from '#/api/system/order-logistics-company';

import { z } from '#/adapter/form';
import { $t } from '#/locales';

export function useSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'name',
      label: $t('order.logisticsCompanyName'),
      rules: z
        .string()
        .min(1, $t('ui.formRules.required', [$t('order.logisticsCompanyName')])),
    },
    {
      component: 'InputNumber',
      componentProps: { class: 'w-full', min: 0 },
      defaultValue: 0,
      fieldName: 'typeValue',
      label: $t('order.logisticsCompanyTypeValue'),
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
      label: $t('order.logisticsCompanyName'),
    },
  ];
}

export function useColumns(
  onActionClick?: OnActionClickFn<OrderLogisticsCompanyApi.OrderLogisticsCompanyItem>,
): VxeTableGridOptions<OrderLogisticsCompanyApi.OrderLogisticsCompanyItem>['columns'] {
  return [
    { field: 'name', title: $t('order.logisticsCompanyName'), minWidth: 160 },
    { field: 'typeValue', title: $t('order.logisticsCompanyTypeValue'), width: 120 },
    { field: 'sort', title: $t('order.sortOrder'), width: 100 },
    {
      align: 'right',
      cellRender: {
        attrs: {
          nameField: 'name',
          nameTitle: $t('order.logisticsCompanyName'),
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
