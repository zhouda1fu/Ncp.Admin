import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { MeetingApi } from '#/api/system/meeting';

import { z } from '#/adapter/form';
import { $t } from '#/locales';

const STATUS_OPTIONS = [
  { label: () => $t('meeting.room.statusDisabled'), value: 0 },
  { label: () => $t('meeting.room.statusAvailable'), value: 1 },
];

function statusLabel(value: number) {
  const map: Record<number, string> = {
    0: 'meeting.room.statusDisabled',
    1: 'meeting.room.statusAvailable',
  };
  return $t(map[value] ?? '');
}

export function useSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'name',
      label: $t('meeting.room.roomName'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('meeting.room.roomName')])),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'location',
      label: $t('meeting.room.location'),
    },
    {
      component: 'InputNumber',
      componentProps: { min: 1, class: 'w-full' },
      fieldName: 'capacity',
      label: $t('meeting.room.capacity'),
      rules: z.number().min(1, $t('ui.formRules.required', [$t('meeting.room.capacity')])),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'equipment',
      label: $t('meeting.room.equipment'),
    },
  ];
}

export function useGridFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'name',
      label: $t('meeting.room.roomName'),
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        options: STATUS_OPTIONS,
        class: 'w-full',
      },
      fieldName: 'status',
      label: $t('meeting.room.status'),
    },
  ];
}

export function useColumns(): VxeTableGridOptions<MeetingApi.MeetingRoomItem>['columns'] {
  return [
    { field: 'name', title: $t('meeting.room.roomName'), width: 140 },
    { field: 'location', title: $t('meeting.room.location'), minWidth: 120 },
    { field: 'capacity', title: $t('meeting.room.capacity'), width: 100 },
    { field: 'equipment', title: $t('meeting.room.equipment'), minWidth: 120 },
    {
      formatter: ({ cellValue }) => statusLabel(cellValue as number),
      field: 'status',
      title: $t('meeting.room.status'),
      width: 90,
    },
    {
      formatter: 'formatDateTime',
      field: 'createdAt',
      title: $t('meeting.room.createdAt'),
      width: 170,
    },
  ];
}
