import type { VbenFormSchema } from '#/adapter/form';
import type { VxeTableGridOptions } from '#/adapter/vxe-table';
import type { MeetingApi } from '#/api/system/meeting';

import { z } from '#/adapter/form';
import { getMeetingRoomList } from '#/api/system/meeting';
import { $t } from '#/locales';

async function getRoomOptions() {
  const res = await getMeetingRoomList({ pageIndex: 1, pageSize: 500 });
  const items = (res.items ?? []).map((r) => ({
    label: r.name,
    value: r.id,
  }));
  return { items, total: res.total ?? 0 };
}

export function useSchema(): VbenFormSchema[] {
  return [
    {
      component: 'ApiSelect',
      componentProps: {
        api: getRoomOptions,
        class: 'w-full',
        labelField: 'label',
        valueField: 'value',
      },
      fieldName: 'meetingRoomId',
      label: $t('meeting.booking.meetingRoom'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('meeting.booking.meetingRoom')])),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'title',
      label: $t('meeting.booking.titleField'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('meeting.booking.titleField')])),
    },
    {
      component: 'DatePicker',
      componentProps: {
        showTime: true,
        format: 'YYYY-MM-DD HH:mm',
        valueFormat: 'YYYY-MM-DDTHH:mm:ss.SSSZ',
        class: 'w-full',
      },
      fieldName: 'startAt',
      label: $t('meeting.booking.startAt'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('meeting.booking.startAt')])),
    },
    {
      component: 'DatePicker',
      componentProps: {
        showTime: true,
        format: 'YYYY-MM-DD HH:mm',
        valueFormat: 'YYYY-MM-DDTHH:mm:ss.SSSZ',
        class: 'w-full',
      },
      fieldName: 'endAt',
      label: $t('meeting.booking.endAt'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('meeting.booking.endAt')])),
    },
  ];
}

export function useGridFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'meetingRoomId',
      label: $t('meeting.booking.meetingRoom'),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'bookerId',
      label: $t('meeting.booking.booker'),
    },
  ];
}

export function useColumns(): VxeTableGridOptions<MeetingApi.MeetingBookingItem>['columns'] {
  return [
    { field: 'meetingRoomName', title: $t('meeting.booking.meetingRoom'), width: 140 },
    { field: 'title', title: $t('meeting.booking.titleField'), minWidth: 160 },
    {
      formatter: 'formatDateTime',
      field: 'startAt',
      title: $t('meeting.booking.startAt'),
      width: 170,
    },
    {
      formatter: 'formatDateTime',
      field: 'endAt',
      title: $t('meeting.booking.endAt'),
      width: 170,
    },
    {
      formatter: 'formatDateTime',
      field: 'createdAt',
      title: $t('meeting.booking.createdAt'),
      width: 170,
    },
  ];
}
