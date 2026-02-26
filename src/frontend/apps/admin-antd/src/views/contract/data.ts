import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { ContractApi } from '#/api/system/contract';

import { z } from '#/adapter/form';
import { $t } from '#/locales';

const STATUS_OPTIONS = [
  { label: () => $t('contract.statusDraft'), value: 0 },
  { label: () => $t('contract.statusPendingApproval'), value: 1 },
  { label: () => $t('contract.statusApproved'), value: 2 },
  { label: () => $t('contract.statusArchived'), value: 3 },
];

function statusLabel(value: number) {
  const map: Record<number, string> = {
    0: 'contract.statusDraft',
    1: 'contract.statusPendingApproval',
    2: 'contract.statusApproved',
    3: 'contract.statusArchived',
  };
  return $t(map[value] ?? '');
}

export function useSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'code',
      label: $t('contract.code'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('contract.code')])),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'title',
      label: $t('contract.titleLabel'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('contract.titleLabel')])),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'partyA',
      label: $t('contract.partyA'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('contract.partyA')])),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'partyB',
      label: $t('contract.partyB'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('contract.partyB')])),
    },
    {
      component: 'InputNumber',
      componentProps: { min: 0, class: 'w-full' },
      fieldName: 'amount',
      label: $t('contract.amount'),
      rules: z.number().min(0),
    },
    {
      component: 'DatePicker',
      componentProps: { class: 'w-full', valueFormat: 'YYYY-MM-DD' },
      fieldName: 'startDate',
      label: $t('contract.startDate'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('contract.startDate')])),
    },
    {
      component: 'DatePicker',
      componentProps: { class: 'w-full', valueFormat: 'YYYY-MM-DD' },
      fieldName: 'endDate',
      label: $t('contract.endDate'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('contract.endDate')])),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'fileStorageKey',
      label: $t('contract.fileStorageKey'),
    },
  ];
}

export function useGridFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'code',
      label: $t('contract.code'),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'title',
      label: $t('contract.titleLabel'),
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        options: STATUS_OPTIONS,
        class: 'w-full',
      },
      fieldName: 'status',
      label: $t('contract.status'),
    },
  ];
}

export function useColumns(
  onActionClick?: OnActionClickFn<ContractApi.ContractItem>,
): VxeTableGridOptions<ContractApi.ContractItem>['columns'] {
  return [
    { field: 'code', title: $t('contract.code'), width: 120 },
    { field: 'title', title: $t('contract.titleLabel'), minWidth: 160 },
    { field: 'partyA', title: $t('contract.partyA'), width: 120 },
    { field: 'partyB', title: $t('contract.partyB'), width: 120 },
    {
      field: 'amount',
      title: $t('contract.amount'),
      width: 110,
      formatter: ({ cellValue }) =>
        cellValue != null ? Number(cellValue).toLocaleString() : '-',
    },
    {
      formatter: ({ cellValue }) => statusLabel(cellValue as number),
      field: 'status',
      title: $t('contract.status'),
      width: 90,
    },
    {
      formatter: 'formatDateTime',
      field: 'createdAt',
      title: $t('contract.createdAt'),
      width: 170,
    },
    {
      align: 'right',
      cellRender: {
        attrs: {
          nameField: 'title',
          nameTitle: $t('contract.titleLabel'),
          onClick: onActionClick,
        },
        name: 'CellOperation',
        options: [
          {
            code: 'edit',
            text: $t('contract.edit'),
            show: (row: ContractApi.ContractItem) => row.status === 0,
          },
          {
            code: 'submit',
            text: $t('contract.submit'),
            show: (row: ContractApi.ContractItem) => row.status === 0,
          },
          {
            code: 'approve',
            text: $t('contract.approve'),
            show: (row: ContractApi.ContractItem) => row.status === 1,
          },
          {
            code: 'archive',
            text: $t('contract.archive'),
            show: (row: ContractApi.ContractItem) => row.status === 2,
          },
        ],
      },
      field: 'operation',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('contract.operation'),
      width: 220,
    },
  ];
}

export { statusLabel, STATUS_OPTIONS };
