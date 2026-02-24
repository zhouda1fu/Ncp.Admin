import type { VbenFormSchema } from '#/adapter/form';
import type { VxeTableGridOptions } from '#/adapter/vxe-table';
import type { AttendanceApi } from '#/api/system/attendance';

import { $t } from '#/locales';

function sourceLabel(value: number) {
  const map: Record<number, string> = {
    0: 'attendance.record.sourceGps',
    1: 'attendance.record.sourceWifi',
    2: 'attendance.record.sourceManual',
  };
  return $t(map[value] ?? '');
}

export function useGridFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'userId',
      label: $t('attendance.record.userId'),
    },
    {
      component: 'DatePicker',
      componentProps: { class: 'w-full', format: 'YYYY-MM-DD', valueFormat: 'YYYY-MM-DD' },
      fieldName: 'dateFrom',
      label: $t('attendance.schedule.workDate') + '起',
    },
    {
      component: 'DatePicker',
      componentProps: { class: 'w-full', format: 'YYYY-MM-DD', valueFormat: 'YYYY-MM-DD' },
      fieldName: 'dateTo',
      label: $t('attendance.schedule.workDate') + '止',
    },
  ];
}

export function useColumns(): VxeTableGridOptions<AttendanceApi.AttendanceRecordItem>['columns'] {
  return [
    { field: 'userId', title: $t('attendance.record.userId'), width: 120 },
    {
      formatter: 'formatDateTime',
      field: 'checkInAt',
      title: $t('attendance.record.checkInAt'),
      width: 170,
    },
    {
      formatter: ({ cellValue }) =>
        cellValue ? new Date(cellValue as string).toLocaleString() : '-',
      field: 'checkOutAt',
      title: $t('attendance.record.checkOutAt'),
      width: 170,
    },
    {
      formatter: ({ cellValue }) => sourceLabel(cellValue as number),
      field: 'source',
      title: $t('attendance.record.source'),
      width: 90,
    },
    { field: 'location', title: $t('attendance.record.location'), minWidth: 120 },
    {
      formatter: 'formatDateTime',
      field: 'createdAt',
      title: $t('attendance.record.createdAt'),
      width: 170,
    },
  ];
}
