import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { SystemPositionApi } from '#/api/system/position';

import { z } from '#/adapter/form';
import { getDeptTree } from '#/api/system/dept';
import { $t } from '#/locales';

export function useSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      fieldName: 'name',
      label: $t('system.position.positionName'),
      rules: z
        .string()
        .min(1, $t('ui.formRules.required', [$t('system.position.positionName')]))
        .max(50, $t('ui.formRules.maxLength', [$t('system.position.positionName'), 50])),
    },
    {
      component: 'Input',
      fieldName: 'code',
      label: $t('system.position.code'),
      rules: z
        .string()
        .min(1, $t('ui.formRules.required', [$t('system.position.code')]))
        .max(32, $t('ui.formRules.maxLength', [$t('system.position.code'), 32])),
    },
    {
      component: 'ApiTreeSelect',
      componentProps: {
        allowClear: false,
        api: getDeptTree,
        class: 'w-full',
        labelField: 'name',
        valueField: 'id',
        childrenField: 'children',
      },
      fieldName: 'deptId',
      label: $t('system.position.dept'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('system.position.dept')])),
    },
    {
      component: 'InputNumber',
      componentProps: {
        min: 0,
        class: 'w-full',
      },
      defaultValue: 0,
      fieldName: 'sortOrder',
      label: $t('system.position.sortOrder'),
    },
    {
      component: 'RadioGroup',
      componentProps: {
        buttonStyle: 'solid',
        options: [
          { label: $t('common.enabled'), value: 1 },
          { label: $t('common.disabled'), value: 0 },
        ],
        optionType: 'button',
      },
      defaultValue: 1,
      fieldName: 'status',
      label: $t('system.position.status'),
    },
    {
      component: 'Textarea',
      componentProps: {
        maxLength: 200,
        rows: 3,
        showCount: true,
      },
      fieldName: 'description',
      label: $t('system.position.description'),
      rules: z.string().max(200, $t('ui.formRules.maxLength', [$t('system.position.description'), 200])).optional(),
    },
  ];
}

export function useGridFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      fieldName: 'name',
      label: $t('system.position.positionName'),
    },
    {
      component: 'Input',
      fieldName: 'code',
      label: $t('system.position.code'),
    },
    {
      component: 'ApiTreeSelect',
      componentProps: {
        allowClear: true,
        api: getDeptTree,
        class: 'w-full',
        labelField: 'name',
        valueField: 'id',
        childrenField: 'children',
      },
      fieldName: 'deptId',
      label: $t('system.position.dept'),
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        options: [
          { label: $t('common.enabled'), value: 1 },
          { label: $t('common.disabled'), value: 0 },
        ],
      },
      fieldName: 'status',
      label: $t('system.position.status'),
    },
  ];
}

export function useColumns(
  onActionClick?: OnActionClickFn<SystemPositionApi.PositionListItem>,
): VxeTableGridOptions<SystemPositionApi.PositionListItem>['columns'] {
  return [
    {
      field: 'name',
      title: $t('system.position.positionName'),
      width: 140,
    },
    {
      field: 'code',
      title: $t('system.position.code'),
      width: 120,
    },
    {
      field: 'deptName',
      title: $t('system.position.dept'),
      width: 140,
    },
    {
      field: 'sortOrder',
      title: $t('system.position.sortOrder'),
      width: 90,
    },
    {
      cellRender: { name: 'CellTag' },
      field: 'status',
      title: $t('system.position.status'),
      width: 100,
    },
    {
      formatter: 'formatDateTime',
      field: 'createdAt',
      title: $t('system.position.createTime'),
      width: 180,
    },
    {
      align: 'right',
      cellRender: {
        attrs: {
          nameField: 'name',
          nameTitle: $t('system.position.name'),
          onClick: onActionClick,
        },
        name: 'CellOperation',
        options: ['edit', 'delete'],
      },
      field: 'operation',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('system.position.operation'),
      width: 140,
    },
  ];
}
