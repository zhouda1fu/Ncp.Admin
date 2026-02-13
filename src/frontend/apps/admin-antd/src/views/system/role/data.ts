import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { SystemRoleApi } from '#/api/system/role';
import type { DataScope } from '#/api/system/role';

import { $t } from '#/locales';

function formatDataScope(value: DataScope | undefined) {
  const labels: Record<DataScope, string> = {
    0: $t('system.role.dataScopeAll'),
    1: $t('system.role.dataScopeDept'),
    2: $t('system.role.dataScopeDeptAndSub'),
    3: $t('system.role.dataScopeSelf'),
  };
  return value !== undefined && value in labels ? labels[value as DataScope] : '-';
}

export function useFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      fieldName: 'name',
      label: $t('system.role.roleName'),
      rules: 'required',
    },
    {
      component: 'RadioGroup',
      componentProps: {
        buttonStyle: 'solid',
        options: [
          { label: $t('common.enabled'), value: true },
          { label: $t('common.disabled'), value: false },
        ],
        optionType: 'button',
      },
      defaultValue: true,
      fieldName: 'isActive',
      label: $t('system.role.status'),
    },
    {
      component: 'Textarea',
      fieldName: 'description',
      label: $t('system.role.remark'),
    },
    {
      component: 'Select',
      componentProps: {
        class: 'w-full',
        options: [
          { label: $t('system.role.dataScopeAll'), value: 0 },
          { label: $t('system.role.dataScopeDept'), value: 1 },
          { label: $t('system.role.dataScopeDeptAndSub'), value: 2 },
          { label: $t('system.role.dataScopeSelf'), value: 3 },
        ],
      },
      defaultValue: 0,
      fieldName: 'dataScope',
      label: $t('system.role.dataScope'),
    },
    {
      component: 'Input',
      fieldName: 'permissionCodes',
      formItemClass: 'items-start',
      label: $t('system.role.setPermissions'),
      modelPropName: 'modelValue',
      slot: 'permissionCodes',
    },
  ];
}

export function useGridFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      fieldName: 'name',
      label: $t('system.role.roleName'),
    },
    {
      component: 'Input',
      fieldName: 'roleId',
      label: $t('system.role.id'),
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        options: [
          { label: $t('common.enabled'), value: true },
          { label: $t('common.disabled'), value: false },
        ],
      },
      fieldName: 'isActive',
      label: $t('system.role.status'),
    },
    {
      component: 'Input',
      fieldName: 'description',
      label: $t('system.role.remark'),
    },
    {
      component: 'RangePicker',
      fieldName: 'createdAt',
      label: $t('system.role.createTime'),
    },
  ];
}

export function useColumns<T = SystemRoleApi.SystemRole>(
  onActionClick: OnActionClickFn<T>,
  onStatusChange?: (
    newStatus: any,
    row: T,
  ) => PromiseLike<boolean | undefined>,
): VxeTableGridOptions['columns'] {
  return [
    {
      field: 'name',
      title: $t('system.role.roleName'),
      width: 200,
    },
    {
      field: 'roleId',
      title: $t('system.role.id'),
      width: 200,
    },
    {
      cellRender: {
        attrs: { beforeChange: onStatusChange },
        name: onStatusChange ? 'CellSwitch' : 'CellTag',
      },
      field: 'isActive',
      title: $t('system.role.status'),
      width: 100,
    },
    {
      field: 'description',
      minWidth: 100,
      title: $t('system.role.remark'),
    },
    {
      field: 'dataScope',
      title: $t('system.role.dataScope'),
      width: 160,
      formatter: ({ row }: { row: T }) => formatDataScope(row.dataScope),
    },
    {
      field: 'createdAt',
      formatter: 'formatDateTime',
      title: $t('system.role.createTime'),
      width: 200,
    },
    {
      align: 'center',
      cellRender: {
        attrs: {
          nameField: 'name',
          nameTitle: $t('system.role.name'),
          onClick: onActionClick,
        },
        name: 'CellOperation',
      },
      field: 'operation',
      fixed: 'right',
      title: $t('system.role.operation'),
      width: 130,
    },
  ];
}
