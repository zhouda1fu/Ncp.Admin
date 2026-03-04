import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { OrderApi } from '#/api/system/order';

import { $t } from '#/locales';

const orderTypeOptions = () => [
  { label: $t('order.typeSales'), value: 0 },
  { label: $t('order.typeAfterSales'), value: 1 },
  { label: $t('order.typeSample'), value: 2 },
  { label: $t('order.typeGeneralTest'), value: 3 },
];

const orderStatusOptions = () => [
  { label: $t('order.statusPendingAudit'), value: 1 },
  { label: $t('order.statusOrdered'), value: 2 },
  { label: $t('order.statusCompleted'), value: 3 },
  { label: $t('order.statusRejected'), value: 4 },
  { label: $t('order.statusUnpaid'), value: 5 },
];

export function useGridFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'orderNumber',
      label: $t('order.orderNumber'),
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        class: 'w-full',
        options: orderTypeOptions(),
      },
      fieldName: 'type',
      label: $t('order.type'),
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        class: 'w-full',
        options: orderStatusOptions(),
      },
      fieldName: 'status',
      label: $t('order.status'),
    },
  ];
}

export function useColumns(
  onActionClick?: OnActionClickFn<OrderApi.OrderItem>,
): VxeTableGridOptions<OrderApi.OrderItem>['columns'] {
  return [
    { field: 'orderNumber', title: $t('order.orderNumber'), minWidth: 140 },
    { field: 'customerName', title: $t('order.customerName'), minWidth: 140 },
    { field: 'projectId', title: $t('order.project'), minWidth: 100 },
    { field: 'contractId', title: $t('order.contract'), minWidth: 100 },
    {
      field: 'type',
      title: $t('order.type'),
      width: 90,
      formatter: ({ cellValue }: { cellValue?: number }) => {
        if (cellValue == null) return '';
        const map: Record<number, string> = {
          0: $t('order.typeSales'),
          1: $t('order.typeAfterSales'),
          2: $t('order.typeSample'),
          3: $t('order.typeGeneralTest'),
        };
        return map[cellValue] ?? '';
      },
    },
    {
      field: 'status',
      title: $t('order.status'),
      width: 100,
      formatter: ({ cellValue }: { cellValue?: number }) => {
        if (cellValue == null) return '';
        const map: Record<number, string> = {
          1: $t('order.statusPendingAudit'),
          2: $t('order.statusOrdered'),
          3: $t('order.statusCompleted'),
          4: $t('order.statusRejected'),
          5: $t('order.statusUnpaid'),
        };
        return map[cellValue] ?? '';
      },
    },
    { field: 'amount', title: $t('order.amount'), width: 110 },
    { field: 'ownerName', title: $t('order.ownerName'), width: 100 },
    {
      formatter: 'formatDateTime',
      field: 'createdAt',
      title: $t('order.createdAt'),
      width: 170,
    },
    {
      align: 'right',
      cellRender: {
        attrs: { nameField: 'orderNumber', nameTitle: $t('order.orderNumber'), onClick: onActionClick },
        name: 'CellOperation',
        options: [
          { code: 'view', text: $t('order.view') },
          { code: 'edit', text: $t('order.edit') },
          { code: 'delete', text: $t('order.delete') },
        ],
      },
      field: 'operation',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('order.operation'),
      width: 200,
    },
  ];
}
