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

/** 合同类型/收支类型选项（来自 ContractTypeOption、IncomeExpenseTypeOption 聚合） */
export type ContractTypeOptionItem = { label: string; value: number };
export type IncomeExpenseTypeOptionItem = { label: string; value: number };

function statusLabel(value: number) {
  const map: Record<number, string> = {
    0: 'contract.statusDraft',
    1: 'contract.statusPendingApproval',
    2: 'contract.statusApproved',
    3: 'contract.statusArchived',
  };
  return $t(map[value] ?? '');
}

function optionLabel(value: number, options: ContractTypeOptionItem[]): string {
  return options.find((o) => o.value === value)?.label ?? String(value);
}

export function useSchema(
  contractTypeOptions: ContractTypeOptionItem[] = [],
  incomeExpenseTypeOptions: IncomeExpenseTypeOptionItem[] = [],
): VbenFormSchema[] {
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
      component: 'Select',
      componentProps: { class: 'w-full', options: contractTypeOptions },
      fieldName: 'contractType',
      label: $t('contract.contractType'),
    },
    {
      component: 'Select',
      componentProps: { class: 'w-full', options: incomeExpenseTypeOptions },
      fieldName: 'incomeExpenseType',
      label: $t('contract.incomeExpenseType'),
    },
    {
      component: 'DatePicker',
      componentProps: { class: 'w-full', valueFormat: 'YYYY-MM-DD' },
      fieldName: 'signDate',
      label: $t('contract.signDate'),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full', type: 'textarea', rows: 2 },
      fieldName: 'note',
      label: $t('contract.note'),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full', type: 'textarea', rows: 4 },
      fieldName: 'description',
      label: $t('contract.description'),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full', readonly: true },
      fieldName: 'orderId',
      label: $t('contract.orderId'),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full', readonly: true },
      fieldName: 'customerId',
      label: $t('contract.customer'),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'fileStorageKey',
      label: $t('contract.fileStorageKey'),
    },
  ];
}

/** 新建/编辑页全屏表单 schema（排版参考图示：合同签订公司、状态、单双盈、收支类型等） */
export function useFormPageSchema(
  contractTypeOptions: ContractTypeOptionItem[] = [],
  incomeExpenseTypeOptions: IncomeExpenseTypeOptionItem[] = [],
): VbenFormSchema[] {
  return [
    { component: 'Select', componentProps: { class: 'w-full', options: contractTypeOptions, allowClear: true }, fieldName: 'contractType', label: $t('contract.signingCompany') },
    { component: 'Select', componentProps: { class: 'w-full', options: STATUS_OPTIONS }, fieldName: 'status', label: $t('contract.status') },
    { component: 'Select', componentProps: { class: 'w-full', options: [{ label: '单盈', value: 0 }, { label: '双盈', value: 1 }], allowClear: true }, fieldName: 'singleDoubleProfit', label: $t('contract.singleDoubleProfit') },
    { component: 'Select', componentProps: { class: 'w-full', options: incomeExpenseTypeOptions, allowClear: true }, fieldName: 'incomeExpenseType', label: $t('contract.incomeExpenseType') },
    { component: 'Input', componentProps: { class: 'w-full' }, fieldName: 'title', label: $t('contract.contractName'), rules: z.string().min(1, $t('ui.formRules.required', [$t('contract.contractName')])) },
    { component: 'Input', componentProps: { class: 'w-full' }, fieldName: 'code', label: $t('contract.contractNumber'), rules: z.string().min(1, $t('ui.formRules.required', [$t('contract.contractNumber')])) },
    { component: 'Input', componentProps: { class: 'w-full' }, fieldName: 'orderId', label: $t('contract.selectOrder') },
    { component: 'Input', componentProps: { class: 'w-full' }, fieldName: 'invoicingInformation', label: $t('contract.invoicingInformation') },
    { component: 'DatePicker', componentProps: { class: 'w-full', valueFormat: 'YYYY-MM-DD' }, fieldName: 'signDate', label: $t('contract.signingTime') },
    { component: 'Input', componentProps: { class: 'w-full', placeholder: $t('contract.selectDepartment') }, fieldName: 'departmentId', label: $t('contract.selectDepartment') },
    { component: 'Input', componentProps: { class: 'w-full' }, fieldName: 'businessManager', label: $t('contract.businessManager') },
    { component: 'Input', componentProps: { class: 'w-full' }, fieldName: 'customerId', label: $t('contract.responsibleCustomer') },
    { component: 'Input', componentProps: { class: 'w-full' }, fieldName: 'responsibleProject', label: $t('contract.responsibleProject') },
    { component: 'Input', componentProps: { class: 'w-full' }, fieldName: 'inputCustomer', label: $t('contract.inputCustomer') },
    { component: 'Input', componentProps: { class: 'w-full' }, fieldName: 'warrantyPeriod', label: $t('contract.warrantyPeriod') },
    { component: 'Select', componentProps: { class: 'w-full', options: [], allowClear: true, placeholder: '---请选择---' }, fieldName: 'paymentStatus', label: $t('contract.paymentStatus') },
    { component: 'Checkbox', componentProps: { class: 'w-full' }, fieldName: 'nextPaymentReminder', label: $t('contract.nextPaymentReminder') },
    { component: 'Checkbox', componentProps: { class: 'w-full' }, fieldName: 'contractExpiryReminder', label: $t('contract.contractExpiryReminder') },
    { component: 'Select', componentProps: { class: 'w-full', options: [{ label: '否', value: 0 }, { label: '是', value: 1 }] }, fieldName: 'isInstallmentPayment', label: $t('contract.isInstallmentPayment') },
    { component: 'InputNumber', componentProps: { min: 0, class: 'w-full' }, fieldName: 'amount', label: $t('contract.contractAmount'), rules: z.number().min(0) },
    { component: 'InputNumber', componentProps: { min: 0, class: 'w-full' }, fieldName: 'accumulatedAmount', label: $t('contract.accumulatedAmount') },
    { component: 'Input', componentProps: { class: 'w-full', type: 'textarea', rows: 4 }, fieldName: 'note', label: $t('contract.remarks') },
    { component: 'Input', componentProps: { class: 'w-full' }, fieldName: 'fileStorageKey', label: $t('contract.chooseFile') },
    { component: 'Input', componentProps: { class: 'w-full' }, fieldName: 'partyA', label: $t('contract.partyA'), rules: z.string().min(1) },
    { component: 'Input', componentProps: { class: 'w-full' }, fieldName: 'partyB', label: $t('contract.partyB'), rules: z.string().min(1) },
    { component: 'DatePicker', componentProps: { class: 'w-full', valueFormat: 'YYYY-MM-DD' }, fieldName: 'startDate', label: $t('contract.startDate'), rules: z.string().min(1) },
    { component: 'DatePicker', componentProps: { class: 'w-full', valueFormat: 'YYYY-MM-DD' }, fieldName: 'endDate', label: $t('contract.endDate'), rules: z.string().min(1) },
  ];
}

export function useGridFormSchema(
  contractTypeOptions: ContractTypeOptionItem[] = [],
  incomeExpenseTypeOptions: IncomeExpenseTypeOptionItem[] = [],
): VbenFormSchema[] {
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
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'orderId',
      label: $t('contract.orderId'),
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
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        options: contractTypeOptions,
        class: 'w-full',
      },
      fieldName: 'contractType',
      label: $t('contract.contractType'),
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        options: incomeExpenseTypeOptions,
        class: 'w-full',
      },
      fieldName: 'incomeExpenseType',
      label: $t('contract.incomeExpenseType'),
    },
    {
      component: 'DatePicker',
      componentProps: { class: 'w-full', valueFormat: 'YYYY-MM-DD' },
      fieldName: 'signDateFrom',
      label: $t('contract.signDateFrom'),
    },
    {
      component: 'DatePicker',
      componentProps: { class: 'w-full', valueFormat: 'YYYY-MM-DD' },
      fieldName: 'signDateTo',
      label: $t('contract.signDateTo'),
    },
  ];
}

export function useColumns(
  onActionClick?: OnActionClickFn<ContractApi.ContractItem>,
  contractTypeOptions: ContractTypeOptionItem[] = [],
  incomeExpenseTypeOptions: IncomeExpenseTypeOptionItem[] = [],
): VxeTableGridOptions<ContractApi.ContractItem>['columns'] {
  return [
    { field: 'code', title: $t('contract.code'), width: 120 },
    { field: 'title', title: $t('contract.titleLabel'), minWidth: 140 },
    {
      formatter: ({ cellValue }) =>
        optionLabel((cellValue as number) ?? 0, contractTypeOptions),
      field: 'contractType',
      title: $t('contract.contractType'),
      width: 90,
    },
    { field: 'customerName', title: $t('contract.customer'), width: 120 },
    {
      field: 'amount',
      title: $t('contract.amount'),
      width: 110,
      formatter: ({ cellValue }) =>
        cellValue != null ? Number(cellValue).toLocaleString() : '-',
    },
    {
      formatter: ({ cellValue }) =>
        optionLabel((cellValue as number) ?? 0, incomeExpenseTypeOptions),
      field: 'incomeExpenseType',
      title: $t('contract.incomeExpenseType'),
      width: 90,
    },
    {
      formatter: ({ cellValue }) =>
        cellValue ? (typeof cellValue === 'string' ? cellValue.slice(0, 10) : '') : '-',
      field: 'signDate',
      title: $t('contract.signDate'),
      width: 110,
    },
    {
      formatter: ({ cellValue }) => statusLabel((cellValue as number) ?? 0),
      field: 'status',
      title: $t('contract.status'),
      width: 90,
    },
    {
      formatter: ({ cellValue }) => (cellValue ? $t('common.yes') : $t('common.no')),
      field: 'hasAttachment',
      title: $t('contract.attachment'),
      width: 80,
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
          {
            code: 'delete',
            text: $t('common.delete'),
            show: (row: ContractApi.ContractItem) => row.status === 0,
          },
        ],
      },
      field: 'operation',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('contract.operation'),
      width: 260,
    },
  ];
}

export { statusLabel, STATUS_OPTIONS };
