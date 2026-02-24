import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { LeaveApi } from '#/api/system/leave';

import { z } from '#/adapter/form';
import { $t } from '#/locales';

const LEAVE_TYPE_OPTIONS = [
  { label: () => $t('leave.request.leaveTypeAnnual'), value: 0 },
  { label: () => $t('leave.request.leaveTypePersonal'), value: 1 },
  { label: () => $t('leave.request.leaveTypeSick'), value: 2 },
  { label: () => $t('leave.request.leaveTypeCompensatory'), value: 3 },
];

const STATUS_OPTIONS = [
  { label: () => $t('leave.request.statusDraft'), value: 0 },
  { label: () => $t('leave.request.statusPending'), value: 1 },
  { label: () => $t('leave.request.statusApproved'), value: 2 },
  { label: () => $t('leave.request.statusRejected'), value: 3 },
  { label: () => $t('leave.request.statusCancelled'), value: 4 },
];

export function useSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Select',
      componentProps: {
        allowClear: false,
        options: LEAVE_TYPE_OPTIONS,
        class: 'w-full',
      },
      fieldName: 'leaveType',
      label: $t('leave.request.leaveType'),
      rules: z.any(),
    },
    {
      component: 'DatePicker',
      componentProps: {
        class: 'w-full',
        valueFormat: 'YYYY-MM-DD',
      },
      fieldName: 'startDate',
      label: $t('leave.request.startDate'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('leave.request.startDate')])),
    },
    {
      component: 'DatePicker',
      componentProps: {
        class: 'w-full',
        valueFormat: 'YYYY-MM-DD',
      },
      fieldName: 'endDate',
      label: $t('leave.request.endDate'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('leave.request.endDate')])),
    },
    {
      component: 'InputNumber',
      componentProps: {
        min: 0.5,
        max: 365,
        step: 0.5,
        class: 'w-full',
      },
      fieldName: 'days',
      label: $t('leave.request.days'),
      rules: z
        .number()
        .min(0.5, $t('ui.formRules.required', [$t('leave.request.days')])),
    },
    {
      component: 'Textarea',
      componentProps: {
        maxLength: 500,
        rows: 3,
        showCount: true,
      },
      fieldName: 'reason',
      label: $t('leave.request.reason'),
      rules: z.string().max(500).optional(),
    },
  ];
}

export function useGridFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      fieldName: 'applicantName',
      label: $t('leave.request.applicant'),
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        options: LEAVE_TYPE_OPTIONS,
      },
      fieldName: 'leaveType',
      label: $t('leave.request.leaveType'),
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        options: STATUS_OPTIONS,
      },
      fieldName: 'status',
      label: $t('leave.request.status'),
    },
    {
      component: 'DatePicker',
      componentProps: {
        valueFormat: 'YYYY-MM-DD',
        class: 'w-full',
      },
      fieldName: 'startDateFrom',
      label: $t('leave.request.startDate') + '（起）',
    },
    {
      component: 'DatePicker',
      componentProps: {
        valueFormat: 'YYYY-MM-DD',
        class: 'w-full',
      },
      fieldName: 'startDateTo',
      label: $t('leave.request.startDate') + '（止）',
    },
  ];
}

function leaveTypeLabel(value: number) {
  const map: Record<number, string> = {
    0: 'leave.request.leaveTypeAnnual',
    1: 'leave.request.leaveTypePersonal',
    2: 'leave.request.leaveTypeSick',
    3: 'leave.request.leaveTypeCompensatory',
  };
  return $t(map[value] ?? '');
}

function statusLabel(value: number) {
  const map: Record<number, string> = {
    0: 'leave.request.statusDraft',
    1: 'leave.request.statusPending',
    2: 'leave.request.statusApproved',
    3: 'leave.request.statusRejected',
    4: 'leave.request.statusCancelled',
  };
  return $t(map[value] ?? '');
}

export function useColumns(
  onActionClick?: OnActionClickFn<LeaveApi.LeaveRequestItem>,
): VxeTableGridOptions<LeaveApi.LeaveRequestItem>['columns'] {
  return [
    {
      field: 'applicantName',
      title: $t('leave.request.applicant'),
      width: 100,
    },
    {
      formatter: ({ cellValue }) => leaveTypeLabel(cellValue as number),
      field: 'leaveType',
      title: $t('leave.request.leaveType'),
      width: 90,
    },
    {
      field: 'startDate',
      title: $t('leave.request.startDate'),
      width: 110,
    },
    {
      field: 'endDate',
      title: $t('leave.request.endDate'),
      width: 110,
    },
    {
      field: 'days',
      title: $t('leave.request.days'),
      width: 80,
    },
    {
      field: 'reason',
      title: $t('leave.request.reason'),
      minWidth: 140,
      showOverflow: 'tooltip',
    },
    {
      formatter: ({ cellValue }) => statusLabel(cellValue as number),
      field: 'status',
      title: $t('leave.request.status'),
      width: 90,
    },
    {
      formatter: 'formatDateTime',
      field: 'createdAt',
      title: $t('leave.request.createTime'),
      width: 170,
    },
    {
      align: 'right',
      cellRender: {
        attrs: {
          nameField: 'applicantName',
          nameTitle: $t('leave.request.name'),
          onClick: onActionClick,
        },
        name: 'CellOperation',
        options: [
          { code: 'edit', text: $t('common.edit'), show: (row: LeaveApi.LeaveRequestItem) => row.status === 0 },
          { code: 'submit', text: $t('leave.request.submit'), show: (row: LeaveApi.LeaveRequestItem) => row.status === 0 },
          { code: 'cancel', text: $t('leave.request.cancel'), show: (row: LeaveApi.LeaveRequestItem) => row.status === 1 },
        ],
      },
      field: 'operation',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('leave.request.operation'),
      width: 180,
    },
  ];
}

export { leaveTypeLabel, statusLabel };
