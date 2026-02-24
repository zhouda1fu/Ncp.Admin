import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { AnnouncementApi } from '#/api/system/announcement';

import { z } from '#/adapter/form';
import { $t } from '#/locales';

const STATUS_OPTIONS = [
  { label: () => $t('announcement.statusDraft'), value: 0 },
  { label: () => $t('announcement.statusPublished'), value: 1 },
];

export function useSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { maxLength: 200, class: 'w-full' },
      fieldName: 'title',
      label: $t('announcement.titleField'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('announcement.titleField')])).max(200),
    },
    {
      component: 'Textarea',
      componentProps: { rows: 8, showCount: true, class: 'w-full' },
      fieldName: 'content',
      label: $t('announcement.content'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('announcement.content')])),
    },
  ];
}

export function useGridFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'title',
      label: $t('announcement.titleField'),
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        options: STATUS_OPTIONS,
        class: 'w-full',
      },
      fieldName: 'status',
      label: $t('announcement.status'),
    },
  ];
}

function statusLabel(value: number) {
  const map: Record<number, string> = {
    0: 'announcement.statusDraft',
    1: 'announcement.statusPublished',
  };
  return $t(map[value] ?? '');
}

export function useColumns(
  onActionClick?: OnActionClickFn<AnnouncementApi.AnnouncementItem>,
): VxeTableGridOptions<AnnouncementApi.AnnouncementItem>['columns'] {
  return [
    {
      field: 'title',
      title: $t('announcement.titleField'),
      minWidth: 160,
      showOverflow: 'tooltip',
    },
    {
      field: 'publisherName',
      title: $t('announcement.publisher'),
      width: 100,
    },
    {
      formatter: ({ cellValue }) => statusLabel(cellValue as number),
      field: 'status',
      title: $t('announcement.status'),
      width: 90,
    },
    {
      field: 'isRead',
      title: $t('announcement.isRead'),
      width: 80,
      formatter: ({ cellValue }) => (cellValue === true ? $t('common.yes') : $t('common.no')),
    },
    {
      formatter: 'formatDateTime',
      field: 'publishAt',
      title: $t('announcement.publishAt'),
      width: 170,
    },
    {
      formatter: 'formatDateTime',
      field: 'createdAt',
      title: $t('announcement.createdAt'),
      width: 170,
    },
    {
      align: 'right',
      cellRender: {
        attrs: {
          nameField: 'title',
          nameTitle: $t('announcement.name'),
          onClick: onActionClick,
        },
        name: 'CellOperation',
        options: [
          {
            code: 'view',
            text: $t('common.view'),
            show: () => true,
          },
          {
            code: 'edit',
            text: $t('common.edit'),
            show: (row: AnnouncementApi.AnnouncementItem) => row.status === 0,
          },
          {
            code: 'publish',
            text: $t('announcement.publish'),
            show: (row: AnnouncementApi.AnnouncementItem) => row.status === 0,
          },
          {
            code: 'read',
            text: $t('announcement.markRead'),
            show: (row: AnnouncementApi.AnnouncementItem) => row.status === 1 && row.isRead !== true,
          },
        ],
      },
      field: 'operation',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('common.operation'),
      width: 220,
    },
  ];
}

export { statusLabel };
