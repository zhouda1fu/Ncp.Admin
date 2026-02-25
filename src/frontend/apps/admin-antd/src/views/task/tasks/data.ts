import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { TaskApi } from '#/api/system/task';

import { z } from '#/adapter/form';
import { getProjectList } from '#/api/system/project';
import { $t } from '#/locales';

async function getProjectOptions() {
  const res = await getProjectList({ pageIndex: 1, pageSize: 500 });
  return (res.items ?? []).map((p) => ({ label: p.name, value: p.id }));
}

export function useSchema(): VbenFormSchema[] {
  return [
    {
      component: 'ApiSelect',
      componentProps: {
        allowClear: false,
        api: getProjectOptions,
        class: 'w-full',
        labelField: 'label',
        valueField: 'value',
      },
      fieldName: 'projectId',
      label: $t('task.task.project'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('task.task.project')])),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'title',
      label: $t('task.task.titleField'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('task.task.titleField')])),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full', type: 'textarea' },
      fieldName: 'description',
      label: $t('task.task.description'),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'assigneeId',
      label: $t('task.task.assignee'),
    },
    {
      component: 'DatePicker',
      componentProps: {
        class: 'w-full',
        format: 'YYYY-MM-DD',
        valueFormat: 'YYYY-MM-DD',
      },
      fieldName: 'dueDate',
      label: $t('task.task.dueDate'),
    },
    {
      component: 'InputNumber',
      componentProps: { class: 'w-full', min: 0 },
      defaultValue: 0,
      fieldName: 'sortOrder',
      label: $t('task.task.sortOrder'),
    },
  ];
}

const STATUS_OPTIONS = [
  { label: () => $t('task.task.statusTodo'), value: 0 },
  { label: () => $t('task.task.statusInProgress'), value: 1 },
  { label: () => $t('task.task.statusDone'), value: 2 },
  { label: () => $t('task.task.statusCancelled'), value: 3 },
];

function statusLabel(value: number) {
  const map: Record<number, string> = {
    0: 'task.task.statusTodo',
    1: 'task.task.statusInProgress',
    2: 'task.task.statusDone',
    3: 'task.task.statusCancelled',
  };
  return $t(map[value] ?? '');
}

export function useGridFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'ApiSelect',
      componentProps: {
        allowClear: true,
        api: getProjectOptions,
        class: 'w-full',
        labelField: 'label',
        valueField: 'value',
      },
      fieldName: 'projectId',
      label: $t('task.task.project'),
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        options: STATUS_OPTIONS,
        class: 'w-full',
      },
      fieldName: 'status',
      label: $t('task.task.status'),
    },
  ];
}

export function useColumns(
  onActionClick?: OnActionClickFn<TaskApi.TaskItem>,
): VxeTableGridOptions<TaskApi.TaskItem>['columns'] {
  return [
    { field: 'projectId', title: $t('task.task.project'), width: 120 },
    { field: 'title', title: $t('task.task.titleField'), minWidth: 160 },
    {
      formatter: ({ cellValue }) => statusLabel(cellValue as number),
      field: 'status',
      title: $t('task.task.status'),
      width: 90,
    },
    { field: 'dueDate', title: $t('task.task.dueDate'), width: 120 },
    {
      formatter: 'formatDateTime',
      field: 'createdAt',
      title: $t('task.task.createdAt'),
      width: 170,
    },
    {
      align: 'right',
      cellRender: {
        attrs: {
          nameField: 'title',
          nameTitle: $t('task.task.name'),
          onClick: onActionClick,
        },
        name: 'CellOperation',
        options: ['edit'],
      },
      field: 'operation',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('task.task.operation'),
      width: 120,
    },
  ];
}

export { STATUS_OPTIONS, statusLabel };
