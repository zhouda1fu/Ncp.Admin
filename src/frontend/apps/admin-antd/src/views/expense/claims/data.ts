import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { ExpenseApi } from '#/api/system/expense';

import { $t } from '#/locales';

const STATUS_OPTIONS = [
  { label: () => $t('expense.claim.statusDraft'), value: 0 },
  { label: () => $t('expense.claim.statusSubmitted'), value: 1 },
  { label: () => $t('expense.claim.statusApproved'), value: 2 },
  { label: () => $t('expense.claim.statusRejected'), value: 3 },
];

const TYPE_OPTIONS = [
  { label: () => $t('expense.claim.typeTravel'), value: 0 },
  { label: () => $t('expense.claim.typeMeals'), value: 1 },
  { label: () => $t('expense.claim.typeAccommodation'), value: 2 },
  { label: () => $t('expense.claim.typeOffice'), value: 3 },
  { label: () => $t('expense.claim.typeOther'), value: 4 },
];

function statusLabel(value: number) {
  const map: Record<number, string> = {
    0: 'expense.claim.statusDraft',
    1: 'expense.claim.statusSubmitted',
    2: 'expense.claim.statusApproved',
    3: 'expense.claim.statusRejected',
  };
  return $t(map[value] ?? '');
}

function typeLabel(value: number) {
  const map: Record<number, string> = {
    0: 'expense.claim.typeTravel',
    1: 'expense.claim.typeMeals',
    2: 'expense.claim.typeAccommodation',
    3: 'expense.claim.typeOffice',
    4: 'expense.claim.typeOther',
  };
  return $t(map[value] ?? '');
}

export function useGridFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'applicantId',
      label: $t('expense.claim.applicant'),
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        options: STATUS_OPTIONS,
        class: 'w-full',
      },
      fieldName: 'status',
      label: $t('expense.claim.status'),
    },
  ];
}

export function useColumns(
  onActionClick?: OnActionClickFn<ExpenseApi.ExpenseClaimItem>,
): VxeTableGridOptions<ExpenseApi.ExpenseClaimItem>['columns'] {
  return [
    { field: 'applicantName', title: $t('expense.claim.applicant'), width: 100 },
    {
      field: 'totalAmount',
      title: $t('expense.claim.totalAmount'),
      width: 120,
      formatter: ({ cellValue }) => (cellValue != null ? Number(cellValue).toFixed(2) : '-'),
    },
    {
      formatter: ({ cellValue }) => statusLabel(cellValue as number),
      field: 'status',
      title: $t('expense.claim.status'),
      width: 90,
    },
    {
      formatter: 'formatDateTime',
      field: 'createdAt',
      title: $t('expense.claim.createdAt'),
      width: 170,
    },
    {
      align: 'right',
      cellRender: {
        attrs: {
          nameField: 'applicantName',
          nameTitle: $t('expense.claim.name'),
          onClick: onActionClick,
        },
        name: 'CellOperation',
        options: [
          {
            code: 'submit',
            text: $t('expense.claim.submit'),
            show: (row: ExpenseApi.ExpenseClaimItem) => row.status === 0,
          },
        ],
      },
      field: 'operation',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('expense.claim.operation'),
      width: 120,
    },
  ];
}

export { STATUS_OPTIONS, TYPE_OPTIONS, statusLabel, typeLabel };
