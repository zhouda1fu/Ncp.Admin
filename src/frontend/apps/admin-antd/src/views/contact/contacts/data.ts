import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { ContactApi } from '#/api/system/contact';

import { z } from '#/adapter/form';
import { getContactGroupList } from '#/api/system/contact-group';
import { $t } from '#/locales';

async function getGroupOptions() {
  const res = await getContactGroupList({ pageIndex: 1, pageSize: 500 });
  return (res.items ?? []).map((g) => ({ label: g.name, value: g.id }));
}

export function useSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'name',
      label: $t('contact.contact.nameField'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('contact.contact.nameField')])),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'phone',
      label: $t('contact.contact.phone'),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'email',
      label: $t('contact.contact.email'),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'company',
      label: $t('contact.contact.company'),
    },
    {
      component: 'ApiSelect',
      componentProps: {
        allowClear: true,
        api: getGroupOptions,
        class: 'w-full',
        labelField: 'label',
        valueField: 'value',
      },
      fieldName: 'groupId',
      label: $t('contact.contact.group'),
    },
  ];
}

export function useGridFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'keyword',
      label: $t('contact.contact.nameField'),
    },
    {
      component: 'ApiSelect',
      componentProps: {
        allowClear: true,
        api: getGroupOptions,
        class: 'w-full',
        labelField: 'label',
        valueField: 'value',
      },
      fieldName: 'groupId',
      label: $t('contact.contact.group'),
    },
  ];
}

export function useColumns(
  onActionClick?: OnActionClickFn<ContactApi.ContactItem>,
): VxeTableGridOptions<ContactApi.ContactItem>['columns'] {
  return [
    { field: 'name', title: $t('contact.contact.nameField'), width: 120 },
    { field: 'phone', title: $t('contact.contact.phone'), width: 130 },
    { field: 'email', title: $t('contact.contact.email'), minWidth: 160 },
    { field: 'company', title: $t('contact.contact.company'), width: 120 },
    { field: 'groupId', title: $t('contact.contact.group'), width: 120 },
    {
      formatter: 'formatDateTime',
      field: 'createdAt',
      title: $t('contact.contact.createdAt'),
      width: 170,
    },
    {
      align: 'right',
      cellRender: {
        attrs: {
          nameField: 'name',
          nameTitle: $t('contact.contact.name'),
          onClick: onActionClick,
        },
        name: 'CellOperation',
        options: ['edit'],
      },
      field: 'operation',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('contact.contact.operation'),
      width: 120,
    },
  ];
}
