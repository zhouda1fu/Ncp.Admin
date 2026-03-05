import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { IncomeExpenseTypeOptionApi } from '#/api/system/income-expense-type';

import { z } from '#/adapter/form';
import { $t } from '#/locales';

export function useSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'name',
      label: $t('contract.incomeExpenseTypeName'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('contract.incomeExpenseTypeName')])),
    },
    {
      component: 'InputNumber',
      componentProps: { class: 'w-full', min: 0 },
      defaultValue: 0,
      fieldName: 'typeValue',
      label: $t('contract.incomeExpenseTypeValue'),
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
      label: $t('contract.incomeExpenseTypeName'),
    },
  ];
}

export function useColumns(
  onActionClick?: OnActionClickFn<IncomeExpenseTypeOptionApi.IncomeExpenseTypeOptionItem>,
): VxeTableGridOptions<IncomeExpenseTypeOptionApi.IncomeExpenseTypeOptionItem>['columns'] {
  return [
    { field: 'name', title: $t('contract.incomeExpenseTypeName'), minWidth: 140 },
    { field: 'typeValue', title: $t('contract.incomeExpenseTypeValue'), width: 100 },
    { field: 'sortOrder', title: $t('contract.sortOrder'), width: 100 },
    {
      align: 'right',
      cellRender: {
        attrs: {
          nameField: 'name',
          nameTitle: $t('contract.incomeExpenseTypeName'),
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
