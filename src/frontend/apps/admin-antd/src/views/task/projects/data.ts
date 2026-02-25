import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { ProjectApi } from '#/api/system/project';

import { z } from '#/adapter/form';
import { $t } from '#/locales';

const STATUS_OPTIONS = [
  { label: () => $t('task.project.statusActive'), value: 0 },
  { label: () => $t('task.project.statusArchived'), value: 1 },
];

function statusLabel(value: number) {
  const map: Record<number, string> = {
    0: 'task.project.statusActive',
    1: 'task.project.statusArchived',
  };
  return $t(map[value] ?? '');
}

export function useSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'name',
      label: $t('task.project.projectName'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('task.project.projectName')])),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'description',
      label: $t('task.project.description'),
    },
  ];
}

export function useGridFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'name',
      label: $t('task.project.projectName'),
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        options: STATUS_OPTIONS,
        class: 'w-full',
      },
      fieldName: 'status',
      label: $t('task.project.status'),
    },
  ];
}

export function useColumns(
  onActionClick?: OnActionClickFn<ProjectApi.ProjectItem>,
): VxeTableGridOptions<ProjectApi.ProjectItem>['columns'] {
  return [
    { field: 'name', title: $t('task.project.projectName'), width: 160 },
    { field: 'description', title: $t('task.project.description'), minWidth: 160 },
    { field: 'creatorId', title: $t('task.project.creator'), width: 120 },
    {
      formatter: ({ cellValue }) => statusLabel(cellValue as number),
      field: 'status',
      title: $t('task.project.status'),
      width: 90,
    },
    {
      formatter: 'formatDateTime',
      field: 'createdAt',
      title: $t('task.project.createdAt'),
      width: 170,
    },
    {
      align: 'right',
      cellRender: {
        attrs: {
          nameField: 'name',
          nameTitle: $t('task.project.name'),
          onClick: onActionClick,
        },
        name: 'CellOperation',
        options: ['edit'],
      },
      field: 'operation',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('task.project.operation'),
      width: 120,
    },
  ];
}

export { statusLabel };
