import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { OrderInvoiceTypeOptionApi } from '#/api/system/order-invoice-type';

import { z } from '#/adapter/form';
import { $t } from '#/locales';

export function useSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'name',
      label: $t('order.invoiceTypeName'),
      rules: z
        .string()
        .min(1, $t('ui.formRules.required', [$t('order.invoiceTypeName')])),
    },
    {
      component: 'InputNumber',
      componentProps: { class: 'w-full', min: 0 },
      defaultValue: 0,
      fieldName: 'typeValue',
      label: $t('order.invoiceTypeValue'),
    },
    {
      component: 'InputNumber',
      componentProps: { class: 'w-full', min: 0 },
      defaultValue: 0,
      fieldName: 'sortOrder',
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
      label: $t('order.invoiceTypeName'),
    },
  ];
}

export function useColumns(
  onActionClick?: OnActionClickFn<OrderInvoiceTypeOptionApi.OrderInvoiceTypeOptionItem>,
): VxeTableGridOptions<OrderInvoiceTypeOptionApi.OrderInvoiceTypeOptionItem>['columns'] {
  return [
    { field: 'name', title: $t('order.invoiceTypeName'), minWidth: 160 },
    { field: 'typeValue', title: $t('order.invoiceTypeValue'), width: 120 },
    { field: 'sortOrder', title: $t('order.sortOrder'), width: 100 },
    {
      align: 'right',
      cellRender: {
        attrs: {
          nameField: 'name',
          nameTitle: $t('order.invoiceTypeName'),
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

