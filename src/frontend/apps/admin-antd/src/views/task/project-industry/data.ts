import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { ProjectIndustryApi } from '#/api/system/project-industry';

import { z } from '#/adapter/form';
import { $t } from '#/locales';

export function useSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'name',
      label: $t('task.projectIndustry.name'),
      rules: z
        .string()
        .min(1, $t('ui.formRules.required', [$t('task.projectIndustry.name')])),
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
      label: $t('task.projectIndustry.name'),
    },
  ];
}

export function useColumns(
  onActionClick?: OnActionClickFn<ProjectIndustryApi.ProjectIndustryItem>,
): VxeTableGridOptions<ProjectIndustryApi.ProjectIndustryItem>['columns'] {
  return [
    { field: 'name', title: $t('task.projectIndustry.name'), minWidth: 160 },
    { field: 'sortOrder', title: $t('task.sortOrder'), width: 100 },
    {
      align: 'right',
      cellRender: {
        attrs: {
          nameField: 'name',
          nameTitle: $t('task.projectIndustry.name'),
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
