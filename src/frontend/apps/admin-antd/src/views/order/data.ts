import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { OrderApi } from '#/api/system/order';

import { OrderStatusEnum } from '#/api/system/order';
import { $t } from '#/locales';

const orderTypeOptions = () => [
  { label: $t('order.typeSales'), value: 0 },
  { label: $t('order.typeAfterSales'), value: 1 },
  { label: $t('order.typeSample'), value: 2 },
  { label: $t('order.typeGeneralTest'), value: 3 },
];

const orderStatusOptions = () => [
  { label: $t('order.statusDraft'), value: 0 },
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
  onActionClick?: OnActionClickFn<OrderApi.OrderQueryDto>,
  getCanSubmit?: () => boolean,
  hasPermission?: (code: string) => boolean,
): VxeTableGridOptions<OrderApi.OrderQueryDto>['columns'] {
  const columns: VxeTableGridOptions<OrderApi.OrderQueryDto>['columns'] = [
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
    { field: 'ownerName', title: $t('order.businessManager'), minWidth: 120 },
    { field: 'customerName', title: $t('order.customerName'), minWidth: 140 },
    {
      field: 'projectName',
      title: $t('order.projectName'),
      minWidth: 140,
      formatter: ({ cellValue }: { cellValue?: string }) =>
        (cellValue && String(cellValue)) || '',
    },
    { field: 'contractSigningCompany', title: $t('order.contractSigningCompany'), minWidth: 160 },
    {
      field: 'task',
      title: $t('order.task'),
      width: 100,
      formatter: () => '',
    },
  ];

  if (hasPermission?.('OrderContractSelect')) {
    columns.push({
      field: 'selectedContractFileId',
      title: $t('order.contract'),
      width: 90,
      formatter: ({ cellValue }: { cellValue?: number }) =>
        (cellValue ?? 0) > 0 ? $t('order.yes') : $t('order.no'),
    });
  }

  if (hasPermission?.('OrderNeedInvoice')) {
    columns.push({
      field: 'needInvoice',
      title: $t('order.needInvoice'),
      width: 90,
      formatter: ({ cellValue }: { cellValue?: boolean }) =>
        cellValue ? $t('order.yes') : $t('order.no'),
    });
  }

  columns.push({
    field: 'invoiceStatus',
    title: $t('order.invoiceStatus'),
    width: 100,
    formatter: () => '',
  });

  columns.push({
    formatter: 'formatDateTime',
    field: 'createdAt',
    title: $t('order.createdAt'),
    width: 170,
  });

  columns.push({
    field: 'paymentStatus',
    title: $t('order.paymentStatus'),
    width: 140,
    formatter: ({ cellValue }: { cellValue?: number }) => {
      if (cellValue == null) return '';
      const map: Record<number, string> = {
        0: $t('order.paymentStatusFullPayment'),
        1: $t('order.paymentStatusPartialPayment'),
        2: $t('order.paymentStatusInstallmentUrgent'),
        3: $t('order.paymentStatusPendingConfirmation'),
      };
      return map[cellValue] ?? '';
    },
  });

  columns.push({
    field: 'isShipped',
    title: $t('order.isShipped'),
    width: 90,
    formatter: ({ cellValue }: { cellValue?: boolean }) =>
      cellValue ? $t('order.yes') : $t('order.no'),
  });

  columns.push({
    field: 'status',
    title: $t('order.status'),
    width: 100,
    slots: { default: 'status' },
  });

  if (hasPermission?.('OrderContractAmount')) {
    columns.push({
      field: 'contractAmount',
      title: $t('order.contractAmount'),
      width: 110,
    });
  }

  columns.push({
    field: 'warehouseStatus',
    title: $t('order.warehouseStatus'),
    width: 110,
    formatter: ({ cellValue }: { cellValue?: number }) => {
      if (cellValue == null) return '';
      const map: Record<number, string> = {
        0: $t('order.warehouseStatusNotPushed'),
        1: $t('order.warehouseStatusUnseen'),
        2: $t('order.warehouseStatusSeen'),
        3: $t('order.warehouseStatusAssigned'),
        4: $t('order.warehouseStatusShipped'),
      };
      return map[cellValue] ?? '';
    },
  });

  if (hasPermission?.('OrderTechnicalStatus')) {
    columns.push({
      field: 'techStatus',
      title: $t('order.techStatus'),
      width: 100,
      formatter: () => $t('order.techStatusNotCompleted'),
    });
  }

  columns.push({
    align: 'right',
    field: 'operation',
    fixed: 'right',
    headerAlign: 'center',
    showOverflow: false,
    slots: { default: 'operation' },
    title: $t('order.operation'),
    width: 240,
  });

  return columns;
}
