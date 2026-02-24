import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { AttendanceApi } from '#/api/system/attendance';

import { z } from '#/adapter/form';
import { getUserList } from '#/api/system/user';
import { $t } from '#/locales';

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
      label: $t('attendance.schedule.userId'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('attendance.schedule.userId')])),
    },
    {
      component: 'DatePicker',
      componentProps: {
        format: 'YYYY-MM-DD',
        valueFormat: 'YYYY-MM-DD',
        class: 'w-full',
      },
      fieldName: 'workDate',
      label: $t('attendance.schedule.workDate'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('attendance.schedule.workDate')])),
    },
    {
      component: 'TimePicker',
      componentProps: { format: 'HH:mm', valueFormat: 'HH:mm', class: 'w-full' },
      fieldName: 'startTime',
      label: $t('attendance.schedule.startTime'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('attendance.schedule.startTime')])),
    },
    {
      component: 'TimePicker',
      componentProps: { format: 'HH:mm', valueFormat: 'HH:mm', class: 'w-full' },
      fieldName: 'endTime',
      label: $t('attendance.schedule.endTime'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('attendance.schedule.endTime')])),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'shiftName',
      label: $t('attendance.schedule.shiftName'),
    },
  ];
}

/** 编辑时仅时间与班次 */
export function useEditSchema(): VbenFormSchema[] {
  return [
    {
      component: 'TimePicker',
      componentProps: { format: 'HH:mm', valueFormat: 'HH:mm', class: 'w-full' },
      fieldName: 'startTime',
      label: $t('attendance.schedule.startTime'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('attendance.schedule.startTime')])),
    },
    {
      component: 'TimePicker',
      componentProps: { format: 'HH:mm', valueFormat: 'HH:mm', class: 'w-full' },
      fieldName: 'endTime',
      label: $t('attendance.schedule.endTime'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('attendance.schedule.endTime')])),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'shiftName',
      label: $t('attendance.schedule.shiftName'),
    },
  ];
}

export function useGridFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'userId',
      label: $t('attendance.schedule.userId'),
    },
    {
      component: 'DatePicker',
      componentProps: {
        format: 'YYYY-MM-DD',
        valueFormat: 'YYYY-MM-DD',
        class: 'w-full',
      },
      fieldName: 'workDateFrom',
      label: $t('attendance.schedule.workDate') + '起',
    },
    {
      component: 'DatePicker',
      componentProps: {
        format: 'YYYY-MM-DD',
        valueFormat: 'YYYY-MM-DD',
        class: 'w-full',
      },
      fieldName: 'workDateTo',
      label: $t('attendance.schedule.workDate') + '止',
    },
  ];
}

export function useColumns(
  onActionClick?: OnActionClickFn<AttendanceApi.ScheduleItem>,
): VxeTableGridOptions<AttendanceApi.ScheduleItem>['columns'] {
  return [
    { field: 'userId', title: $t('attendance.schedule.userId'), width: 120 },
    { field: 'workDate', title: $t('attendance.schedule.workDate'), width: 120 },
    { field: 'startTime', title: $t('attendance.schedule.startTime'), width: 100 },
    { field: 'endTime', title: $t('attendance.schedule.endTime'), width: 100 },
    { field: 'shiftName', title: $t('attendance.schedule.shiftName'), minWidth: 120 },
    {
      formatter: 'formatDateTime',
      field: 'createdAt',
      title: $t('attendance.schedule.createdAt'),
      width: 170,
    },
    {
      align: 'right',
      cellRender: {
        attrs: {
          nameField: 'shiftName',
          nameTitle: $t('attendance.schedule.name'),
          onClick: onActionClick,
        },
        name: 'CellOperation',
        options: ['edit'],
      },
      field: 'operation',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('attendance.schedule.operation'),
      width: 120,
    },
  ];
}
