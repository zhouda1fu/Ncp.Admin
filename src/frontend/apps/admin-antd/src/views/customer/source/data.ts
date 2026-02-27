import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { CustomerSourceApi } from '#/api/system/customerSource';

import { z } from '#/adapter/form';
import { $t } from '#/locales';

export function useSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'name',
      label: $t('customer.sourceName'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('customer.sourceName')])),
    },
    {
      component: 'InputNumber',
      componentProps: { class: 'w-full', min: 0 },
      defaultValue: 0,
      fieldName: 'sortOrder',
      label: $t('customer.sortOrder'),
    },
  ];
}

export function useGridFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'name',
      label: $t('customer.sourceName'),
    },
  ];
}

export function useColumns(
  onActionClick?: OnActionClickFn<CustomerSourceApi.CustomerSourceItem>,
): VxeTableGridOptions<CustomerSourceApi.CustomerSourceItem>['columns'] {
  return [
    { field: 'name', title: $t('customer.sourceName'), minWidth: 160 },
    { field: 'sortOrder', title: $t('customer.sortOrder'), width: 100 },
    {
      align: 'right',
      cellRender: {
        attrs: {
          nameField: 'name',
          nameTitle: $t('customer.sourceName'),
          onClick: onActionClick,
        },
        name: 'CellOperation',
        options: [{ code: 'edit', text: $t('customer.edit') }],
      },
      field: 'operation',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('customer.operation'),
      width: 120,
    },
  ];
}
