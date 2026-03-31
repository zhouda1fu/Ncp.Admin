import type { VbenFormSchema } from '#/adapter/form';
import type { VxeTableGridOptions } from '#/adapter/vxe-table';
import type { OperationLogApi } from '#/api/system/operation-log';
import type { OperationLogType } from '#/api/system/operation-log';

import { $t } from '#/locales';

function formatOperationType(value: OperationLogType | undefined) {
  const labels: Record<OperationLogType, string> = {
    0: $t('system.operationLog.typeCreate'),
    1: $t('system.operationLog.typeUpdate'),
    2: $t('system.operationLog.typeDelete'),
    3: $t('system.operationLog.typeSubmit'),
    4: $t('system.operationLog.typeApprove'),
    5: $t('system.operationLog.typeOther'),
  };
  return value !== undefined && value in labels ? labels[value] : '-';
}

export function useGridFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'operatorUserId',
      label: $t('system.operationLog.operatorUserId'),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'module',
      label: $t('system.operationLog.module'),
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        class: 'w-full',
        options: [
          { label: $t('system.operationLog.typeCreate'), value: 0 },
          { label: $t('system.operationLog.typeUpdate'), value: 1 },
          { label: $t('system.operationLog.typeDelete'), value: 2 },
          { label: $t('system.operationLog.typeSubmit'), value: 3 },
          { label: $t('system.operationLog.typeApprove'), value: 4 },
          { label: $t('system.operationLog.typeOther'), value: 5 },
        ],
      },
      fieldName: 'operationType',
      label: $t('system.operationLog.operationType'),
    },
    {
      component: 'RangePicker',
      componentProps: { class: 'w-full' },
      fieldName: 'createdAt',
      label: $t('system.operationLog.operationTime'),
    },
  ];
}

export function useColumns(): VxeTableGridOptions<OperationLogApi.OperationLogItem>['columns'] {
  return [
    {
      field: 'operatorUserName',
      title: $t('system.operationLog.operatorUserName'),
      width: 120,
    },
    {
      field: 'module',
      title: $t('system.operationLog.module'),
      width: 100,
    },
    {
      field: 'operationType',
      formatter: ({ row }: { row: OperationLogApi.OperationLogItem }) =>
        formatOperationType(row.operationType),
      title: $t('system.operationLog.operationType'),
      width: 90,
    },
    {
      field: 'requestPath',
      title: $t('system.operationLog.requestPath'),
      minWidth: 180,
    },
    {
      field: 'requestMethod',
      title: $t('system.operationLog.requestMethod'),
      width: 90,
    },
    {
      field: 'httpStatusCode',
      title: $t('system.operationLog.httpStatusCode'),
      width: 100,
    },
    {
      cellRender: { name: 'CellTag' },
      field: 'isSuccess',
      title: $t('system.operationLog.isSuccess'),
      width: 90,
    },
    {
      field: 'ipAddress',
      title: $t('system.operationLog.ipAddress'),
      width: 130,
    },
    {
      field: 'durationMs',
      title: $t('system.operationLog.durationMs'),
      width: 100,
    },
    {
      field: 'createdAt',
      formatter: 'formatDateTime',
      title: $t('system.operationLog.operationTime'),
      width: 180,
    },
    {
      align: 'center',
      field: 'operation',
      fixed: 'right',
      title: $t('system.workflow.instance.operation'),
      width: 100,
      slots: { default: 'action' },
    },
    {
      field: '_flex',
      title: '',
    },
  ];
}
