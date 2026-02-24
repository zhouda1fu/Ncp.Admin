import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { LeaveApi } from '#/api/system/leave';

import { z } from '#/adapter/form';
import { getUserList } from '#/api/system/user';
import { $t } from '#/locales';

const LEAVE_TYPE_OPTIONS = [
  { label: () => $t('leave.request.leaveTypeAnnual'), value: 0 },
  { label: () => $t('leave.request.leaveTypePersonal'), value: 1 },
  { label: () => $t('leave.request.leaveTypeSick'), value: 2 },
  { label: () => $t('leave.request.leaveTypeCompensatory'), value: 3 },
];

function leaveTypeLabel(value: number) {
  const map: Record<number, string> = {
    0: 'leave.request.leaveTypeAnnual',
    1: 'leave.request.leaveTypePersonal',
    2: 'leave.request.leaveTypeSick',
    3: 'leave.request.leaveTypeCompensatory',
  };
  return $t(map[value] ?? '');
}

async function getUsersForSelect() {
  const res = await getUserList({ pageIndex: 1, pageSize: 500 });
  const items = (res.items ?? []).map((u: { userId: string; realName?: string; name: string }) => ({
    label: u.realName || u.name || u.userId,
    value: u.userId,
  }));
  return { items, total: res.total ?? 0 };
}

export function useSchema(): VbenFormSchema[] {
  return [
    {
      component: 'ApiSelect',
      componentProps: {
        allowClear: false,
        api: getUsersForSelect,
        class: 'w-full',
        labelField: 'label',
        valueField: 'value',
      },
      fieldName: 'userId',
      label: $t('leave.balance.userId'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('leave.balance.userId')])),
    },
    {
      component: 'InputNumber',
      componentProps: {
        min: 2020,
        max: 2100,
        class: 'w-full',
      },
      fieldName: 'year',
      label: $t('leave.balance.year'),
      rules: z.number().min(2020, $t('ui.formRules.required', [$t('leave.balance.year')])),
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: false,
        options: LEAVE_TYPE_OPTIONS,
        class: 'w-full',
      },
      fieldName: 'leaveType',
      label: $t('leave.balance.leaveType'),
      rules: z.any(),
    },
    {
      component: 'InputNumber',
      componentProps: {
        min: 0,
        step: 0.5,
        class: 'w-full',
      },
      fieldName: 'totalDays',
      label: $t('leave.balance.totalDays'),
      rules: z.number().min(0, $t('ui.formRules.required', [$t('leave.balance.totalDays')])),
    },
  ];
}

export function useGridFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'userId',
      label: $t('leave.balance.userId'),
    },
    {
      component: 'InputNumber',
      componentProps: { min: 2020, max: 2100, class: 'w-full' },
      fieldName: 'year',
      label: $t('leave.balance.year'),
    },
    {
      component: 'Select',
      componentProps: { allowClear: true, options: LEAVE_TYPE_OPTIONS, class: 'w-full' },
      fieldName: 'leaveType',
      label: $t('leave.balance.leaveType'),
    },
  ];
}

export function useColumns(
  onActionClick?: OnActionClickFn<LeaveApi.LeaveBalanceItem>,
): VxeTableGridOptions<LeaveApi.LeaveBalanceItem>['columns'] {
  return [
    {
      align: 'left',
      field: 'userId',
      title: $t('leave.balance.userId'),
      width: 180,
    },
    {
      field: 'year',
      title: $t('leave.balance.year'),
      width: 180,
    },
    {
      formatter: ({ cellValue }) => leaveTypeLabel(cellValue as number),
      field: 'leaveType',
      title: $t('leave.balance.leaveType'),
      width: 180,
    },
    {
      field: 'totalDays',
      title: $t('leave.balance.totalDays'),
      width: 180,
    },
    {
      field: 'usedDays',
      title: $t('leave.balance.usedDays'),
      width: 180,
    },
    {
      field: 'remainingDays',
      title: $t('leave.balance.remainingDays'),
      width: 180,
    },
    {
      formatter: 'formatDateTime',
      field: 'createdAt',
      title: $t('leave.balance.createTime'),
      width: 180,
    },
    {
      align: 'right',
      cellRender: {
        attrs: {
          nameField: 'userId',
          nameTitle: $t('leave.balance.userId'),
          onClick: onActionClick,
        },
        name: 'CellOperation',
        options: [{ code: 'edit', text: $t('leave.balance.setBalance') }],
      },
      field: 'operation',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('leave.balance.operation'),
    
    },
  ];
}
