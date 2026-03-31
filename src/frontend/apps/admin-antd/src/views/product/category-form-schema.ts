import type { ProductCategoryTreeItem } from '#/api/system/product';
import type { VbenFormSchema } from '#/adapter/form';

import { z } from '#/adapter/form';
import { $t } from '#/locales';

export function useCategoryFormSchema(treeData: ProductCategoryTreeItem[]): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'name',
      label: $t('product.categoryName'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('product.categoryName')])),
    },
    {
      component: 'Textarea',
      componentProps: { class: 'w-full', rows: 3 },
      fieldName: 'remark',
      label: $t('product.categoryRemark'),
    },
    {
      component: 'TreeSelect',
      componentProps: {
        allowClear: true,
        class: 'w-full',
        fieldNames: { label: 'name', value: 'id', children: 'children' },
        placeholder: $t('product.categoryParent'),
        treeData,
        treeDefaultExpandAll: true,
      },
      fieldName: 'parentId',
      label: $t('product.categoryParent'),
    },
    {
      component: 'InputNumber',
      componentProps: { class: 'w-full', min: 0 },
      defaultValue: 0,
      fieldName: 'sortOrder',
      label: $t('product.categorySortOrder'),
    },
    {
      component: 'Switch',
      componentProps: {},
      defaultValue: true,
      fieldName: 'visible',
      label: $t('product.categoryVisible'),
    },
    {
      component: 'Switch',
      componentProps: {},
      defaultValue: false,
      fieldName: 'isDiscount',
      label: $t('product.categoryIsDiscount'),
    },
  ];
}
