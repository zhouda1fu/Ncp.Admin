import type { VbenFormSchema } from '#/adapter/form';

import { z } from '#/adapter/form';
import { $t } from '#/locales';

export function useProductTypeFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      fieldName: 'name',
      label: $t('product.productTypeName'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('product.productTypeName')])).max(100),
    },
    {
      component: 'InputNumber',
      componentProps: { min: 0, class: 'w-full' },
      fieldName: 'sortOrder',
      label: $t('product.productTypeSortOrder'),
      rules: z.number().int().min(0).optional().default(0),
    },
    {
      component: 'Switch',
      fieldName: 'visible',
      label: $t('product.productTypeVisible'),
      rules: z.boolean().optional().default(true),
    },
  ];
}
