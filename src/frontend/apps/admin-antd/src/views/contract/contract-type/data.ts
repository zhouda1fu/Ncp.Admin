import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { ContractTypeOptionApi } from '#/api/system/contract-type';

import { z } from '#/adapter/form';
import { $t } from '#/locales';

export function useSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'name',
      label: $t('contract.contractTypeName'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('contract.contractTypeName')])),
    },
    {
      component: 'InputNumber',
      componentProps: { class: 'w-full', min: 0 },
      defaultValue: 0,
      fieldName: 'typeValue',
      label: $t('contract.contractTypeValue'),
    },
    {
      component: 'Checkbox',
      componentProps: { class: 'w-full' },
      defaultValue: false,
      fieldName: 'orderSigningCompanyOptionDisplay',
      label: $t('contract.orderSigningCompanyOptionDisplay'),
    },
    {
      component: 'InputNumber',
      componentProps: { class: 'w-full', min: 0 },
      defaultValue: 0,
      fieldName: 'sortOrder',
      label: $t('contract.sortOrder'),
    },
  ];
}

export function useGridFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'name',
      label: $t('contract.contractTypeName'),
    },
  ];
}

export function useColumns(
  onActionClick?: OnActionClickFn<ContractTypeOptionApi.ContractTypeOptionItem>,
): VxeTableGridOptions<ContractTypeOptionApi.ContractTypeOptionItem>['columns'] {
  return [
    { field: 'name', title: $t('contract.contractTypeName'), minWidth: 140 },
    { field: 'typeValue', title: $t('contract.contractTypeValue'), width: 100 },
    {
      field: 'orderSigningCompanyOptionDisplay',
      title: $t('contract.orderSigningCompanyOptionDisplay'),
      width: 160,
      cellRender: {
        name: 'CellTag',
        options: [
          { color: 'success', label: $t('common.yes'), value: true },
          { color: 'default', label: $t('common.no'), value: false },
        ],
      },
    },
    { field: 'sortOrder', title: $t('contract.sortOrder'), width: 100 },
    {
      align: 'right',
      cellRender: {
        attrs: {
          nameField: 'name',
          nameTitle: $t('contract.contractTypeName'),
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
      title: $t('contract.operation'),
      width: 160,
    },
  ];
}
