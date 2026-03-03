import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { ProjectTypeApi } from '#/api/system/project-type';

import { z } from '#/adapter/form';
import { $t } from '#/locales';

export function useSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'name',
      label: $t('task.projectType.name'),
      rules: z
        .string()
        .min(1, $t('ui.formRules.required', [$t('task.projectType.name')])),
    },
    {
      component: 'InputNumber',
      componentProps: { class: 'w-full', min: 0 },
      defaultValue: 0,
      fieldName: 'sortOrder',
      label: $t('task.sortOrder'),
    },
  ];
}

export function useGridFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'name',
      label: $t('task.projectType.name'),
    },
  ];
}

export function useColumns(
  onActionClick?: OnActionClickFn<ProjectTypeApi.ProjectTypeItem>,
): VxeTableGridOptions<ProjectTypeApi.ProjectTypeItem>['columns'] {
  return [
    { field: 'name', title: $t('task.projectType.name'), minWidth: 160 },
    { field: 'sortOrder', title: $t('task.sortOrder'), width: 100 },
    {
      align: 'right',
      cellRender: {
        attrs: {
          nameField: 'name',
          nameTitle: $t('task.projectType.name'),
          onClick: onActionClick,
        },
        name: 'CellOperation',
        options: [
          { code: 'edit', text: $t('common.edit') },
          { code: 'delete', text: $t('common.delete') },
        ],
      },
      field: 'operation',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('task.project.operation'),
      width: 160,
    },
  ];
}
