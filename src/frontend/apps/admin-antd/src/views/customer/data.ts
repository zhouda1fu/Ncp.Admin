import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { CustomerApi } from '#/api/system/customer';

import { z } from '#/adapter/form';
import { $t } from '#/locales';

export function useSchema(
  industryOptions: { label: string; value: string }[],
  customerSourceOptions: { label: string; value: string }[],
): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'fullName',
      label: $t('customer.fullName'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('customer.fullName')])),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'shortName',
      label: $t('customer.shortName'),
    },
    {
      component: 'Select',
      componentProps: {
        class: 'w-full',
        options: customerSourceOptions,
        placeholder: $t('ui.formRules.required', [$t('customer.customerSource')]),
      },
      fieldName: 'customerSourceId',
      label: $t('customer.customerSource'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('customer.customerSource')])),
    },
    {
      component: 'InputNumber',
      componentProps: { min: 0, class: 'w-full' },
      fieldName: 'statusId',
      label: $t('customer.statusId'),
    },
    {
      component: 'InputNumber',
      componentProps: { class: 'w-full' },
      fieldName: 'ownerId',
      label: $t('customer.ownerId'),
    },
    {
      component: 'InputNumber',
      componentProps: { class: 'w-full' },
      fieldName: 'deptId',
      label: $t('customer.deptId'),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'mainContactName',
      label: $t('customer.mainContactName'),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'mainContactPhone',
      label: $t('customer.mainContactPhone'),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'wechatStatus',
      label: $t('customer.wechatStatus'),
    },
    {
      component: 'Select',
      componentProps: {
        class: 'w-full',
        mode: 'multiple',
        options: industryOptions,
      },
      fieldName: 'industryIds',
      label: $t('customer.industryIds'),
    },
    {
      component: 'Checkbox',
      componentProps: { class: 'w-full' },
      fieldName: 'isKeyAccount',
      label: $t('customer.isKeyAccount'),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full', type: 'textarea' },
      fieldName: 'remark',
      label: $t('customer.remark'),
    },
  ];
}

export function useGridFormSchema(
  customerSourceOptions: { label: string; value: string }[] = [],
): VbenFormSchema[] {
  return [
    { component: 'Input', componentProps: { class: 'w-full' }, fieldName: 'fullName', label: $t('customer.fullName') },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        class: 'w-full',
        options: customerSourceOptions,
      },
      fieldName: 'customerSourceId',
      label: $t('customer.customerSource'),
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        options: [
          { label: () => $t('customer.isInSeaYes'), value: true },
          { label: () => $t('customer.isInSeaNo'), value: false },
        ],
        class: 'w-full',
      },
      fieldName: 'isInSea',
      label: $t('customer.isInSea'),
    },
  ];
}

export function useColumns(
  onActionClick?: OnActionClickFn<CustomerApi.CustomerItem>,
): VxeTableGridOptions<CustomerApi.CustomerItem>['columns'] {
  return [
    { field: 'fullName', title: $t('customer.fullName'), minWidth: 140 },
    { field: 'shortName', title: $t('customer.shortName'), width: 120 },
    { field: 'customerSourceName', title: $t('customer.customerSource'), width: 100 },
    { field: 'mainContactName', title: $t('customer.mainContactName'), width: 100 },
    { field: 'mainContactPhone', title: $t('customer.mainContactPhone'), width: 120 },
    {
      field: 'contactCount',
      title: $t('customer.contactCount'),
      width: 90,
    },
    {
      field: 'isKeyAccount',
      title: $t('customer.isKeyAccount'),
      width: 90,
      formatter: ({ cellValue }) => (cellValue ? $t('common.yes') : $t('common.no')),
    },
    {
      field: 'isInSea',
      title: $t('customer.isInSea'),
      width: 80,
      formatter: ({ cellValue }) => (cellValue ? $t('customer.isInSeaYes') : $t('customer.isInSeaNo')),
    },
    {
      formatter: 'formatDateTime',
      field: 'createdAt',
      title: $t('customer.createdAt'),
      width: 170,
    },
    {
      align: 'right',
      cellRender: {
        attrs: { nameField: 'fullName', nameTitle: $t('customer.fullName'), onClick: onActionClick },
        name: 'CellOperation',
        options: [
          { code: 'edit', text: $t('customer.edit') },
          { code: 'releaseToSea', text: $t('customer.releaseToSea'), show: (row: CustomerApi.CustomerItem) => !row.isInSea },
        ],
      },
      field: 'operation',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('customer.operation'),
      width: 200,
    },
  ];
}
