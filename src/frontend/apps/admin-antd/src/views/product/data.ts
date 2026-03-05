import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { ProductApi } from '#/api/system/product';

import { z } from '#/adapter/form';
import { $t } from '#/locales';

export function useSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'name',
      label: $t('product.name'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('product.name')])).max(200),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'code',
      label: $t('product.code'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('product.code')])).max(100),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'model',
      label: $t('product.model'),
      rules: z.string().max(100),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'unit',
      label: $t('product.unit'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('product.unit')])).max(20),
    },
  ];
}

export function useGridFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'keyword',
      label: $t('product.keyword'),
    },
  ];
}

export function useColumns(
  onActionClick?: OnActionClickFn<ProductApi.ProductItem>,
): VxeTableGridOptions<ProductApi.ProductItem>['columns'] {
  return [
    { type: 'checkbox', width: 88 },
    { field: 'name', title: $t('product.name'), minWidth: 160 },
    { field: 'code', title: $t('product.code'), minWidth: 120 },
    { field: 'model', title: $t('product.model'), minWidth: 120 },
    { field: 'unit', title: $t('product.unit'), width: 80 },
    {
      align: 'right',
      cellRender: {
        attrs: {
          nameField: 'name',
          nameTitle: $t('product.name'),
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
      title: $t('common.operation'),
      width: 140,
    },
  ];
}
