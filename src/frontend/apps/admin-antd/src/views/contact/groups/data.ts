import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { ContactGroupApi } from '#/api/system/contact-group';

import { z } from '#/adapter/form';
import { $t } from '#/locales';

export function useSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'name',
      label: $t('contact.group.groupName'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('contact.group.groupName')])),
    },
    {
      component: 'InputNumber',
      componentProps: { class: 'w-full', min: 0 },
      defaultValue: 0,
      fieldName: 'sortOrder',
      label: $t('contact.group.sortOrder'),
    },
  ];
}

export function useGridFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'name',
      label: $t('contact.group.groupName'),
    },
  ];
}

export function useColumns(
  onActionClick?: OnActionClickFn<ContactGroupApi.ContactGroupItem>,
): VxeTableGridOptions<ContactGroupApi.ContactGroupItem>['columns'] {
  return [
    { field: 'name', title: $t('contact.group.groupName'), width: 160 },
    { field: 'sortOrder', title: $t('contact.group.sortOrder'), width: 100 },
    { field: 'creatorId', title: $t('contact.group.creator'), width: 120 },
    {
      formatter: 'formatDateTime',
      field: 'createdAt',
      title: $t('contact.group.createdAt'),
      width: 170,
    },
    {
      align: 'right',
      cellRender: {
        attrs: {
          nameField: 'name',
          nameTitle: $t('contact.group.name'),
          onClick: onActionClick,
        },
        name: 'CellOperation',
        options: ['edit'],
      },
      field: 'operation',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('contact.group.operation'),
      width: 120,
    },
  ];
}
