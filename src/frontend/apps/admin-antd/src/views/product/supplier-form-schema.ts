import type { VbenFormSchema } from '#/adapter/form';

import { z } from '#/adapter/form';
import { $t } from '#/locales';

export function useSupplierFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      fieldName: 'fullName',
      label: $t('product.supplierFullName'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('product.supplierFullName')])),
    },
    {
      component: 'Input',
      fieldName: 'shortName',
      label: $t('product.supplierShortName'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('product.supplierShortName')])),
    },
    {
      component: 'Input',
      fieldName: 'contact',
      label: $t('product.supplierContact'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('product.supplierContact')])),
    },
    {
      component: 'Input',
      fieldName: 'phone',
      label: $t('product.supplierPhone'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('product.supplierPhone')])),
    },
    {
      component: 'Input',
      fieldName: 'email',
      label: $t('product.supplierEmail'),
    },
    {
      component: 'Input',
      fieldName: 'address',
      label: $t('product.supplierAddress'),
    },
    {
      component: 'Textarea',
      componentProps: { rows: 3 ,class: 'w-full'},
      fieldName: 'remark',
      label: $t('product.supplierRemark'),
    },
  ];
}
