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

/** 发票类型选项（与后端 InvoiceType 枚举一致） */
export const INVOICE_TYPE_OPTIONS = [
  { label: () => $t('contract.invoiceTypeVatSpecial'), value: 0 },
  { label: () => $t('contract.invoiceTypeVatGeneral'), value: 1 },
  { label: () => $t('contract.invoiceTypeReceipt'), value: 2 },
  { label: () => $t('contract.invoiceTypeAirEticket'), value: 3 },
  { label: () => $t('contract.invoiceTypeRailwayEticket'), value: 4 },
];

/** 发票状态选项：false=待确认，true=已确认 */
export const INVOICE_STATUS_OPTIONS = [
  { label: () => $t('contract.statusPendingConfirm'), value: false },
  { label: () => $t('contract.statusConfirmed'), value: true },
];

/** 发票抽屉表单 schema（与主题表单一致：useVbenForm + useVbenDrawer） */
export function useInvoiceFormSchema(
  invoiceTypeOptions: { label: string; value: number }[],
  invoiceStatusOptions: { label: string; value: number }[],
): VbenFormSchema[] {
  return [
    { component: 'Select', componentProps: { class: 'w-full', options: invoiceTypeOptions }, fieldName: 'type', label: $t('contract.invoiceType'), rules: z.any() },
    { component: 'Select', componentProps: { class: 'w-full', options: invoiceStatusOptions }, fieldName: 'status', label: $t('contract.invoiceStatus'), rules: z.any() },
    { component: 'Input', componentProps: { class: 'w-full' }, fieldName: 'invoiceNumber', label: $t('contract.invoiceNumber'), rules: z.string().min(1, $t('ui.formRules.required', [$t('contract.invoiceNumber')])) },
    { component: 'InputNumber', componentProps: { class: 'w-full', min: 0, max: 100, precision: 2, addonAfter: '%' }, fieldName: 'taxRate', label: $t('contract.invoiceTaxRate'), rules: z.any() },
    { component: 'InputNumber', componentProps: { class: 'w-full', min: 0, precision: 2 }, fieldName: 'amountExclTax', label: $t('contract.amountExclTax'), rules: z.any() },
    { component: 'Input', componentProps: { class: 'w-full' }, fieldName: 'source', label: $t('contract.source'), rules: z.string().min(1, $t('ui.formRules.required', [$t('contract.source')])) },
    { component: 'Input', componentProps: { class: 'w-full' }, fieldName: 'title', label: $t('contract.invoiceTitle'), formItemClass: 'col-span-2', rules: z.string().min(1, $t('ui.formRules.required', [$t('contract.invoiceTitle')])) },
    { component: 'InputNumber', componentProps: { class: 'w-full', min: 0, precision: 2 }, fieldName: 'taxAmount', label: $t('contract.taxAmount'), rules: z.any() },
    { component: 'InputNumber', componentProps: { class: 'w-full', min: 0, precision: 2 }, fieldName: 'invoicedAmount', label: $t('contract.invoicedAmount'), rules: z.any() },
    { component: 'Input', componentProps: { class: 'w-full' }, fieldName: 'handler', label: $t('contract.handler') },
    { component: 'DatePicker', componentProps: { class: 'w-full', valueFormat: 'YYYY-MM-DD' }, fieldName: 'billingDate', label: $t('contract.billingDate'), rules: z.any() },
    { component: 'Textarea', componentProps: { class: 'w-full', rows: 3 }, fieldName: 'remarks', label: $t('contract.remarksNote'), formItemClass: 'col-span-2' },
    { component: 'Input', componentProps: { class: 'w-full' }, fieldName: 'attachmentStorageKey', label: $t('contract.attachment'), formItemClass: 'col-span-2' },
  ];
}

/** 新建/编辑页全屏表单 schema（合同过期报警下方单独一行：是否分期、合同金额、累计金额三列） */
export function useFormPageSchema(
  contractTypeOptions: ContractTypeOptionItem[] = [],
  incomeExpenseTypeOptions: IncomeExpenseTypeOptionItem[] = [],
): VbenFormSchema[] {
  return [
    { component: 'Select', componentProps: { class: 'w-full', options: contractTypeOptions, allowClear: true }, fieldName: 'contractType', label: $t('contract.signingCompany') },
    { component: 'Select', componentProps: { class: 'w-full', options: STATUS_OPTIONS }, fieldName: 'status', label: $t('contract.status') },
    { component: 'Select', componentProps: { class: 'w-full', options: [{ label: '单章', value: 0 }, { label: '双章', value: 1 }], allowClear: true }, fieldName: 'singleDoubleSeal', label: $t('contract.singleDoubleSeal') },
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
    { component: 'Checkbox', componentProps: { class: 'w-full' }, fieldName: 'nextPaymentReminder', label: $t('contract.nextPaymentReminder'), formItemClass: 'col-span-2' },
    { component: 'Checkbox', componentProps: { class: 'w-full' }, fieldName: 'contractExpiryReminder', label: $t('contract.contractExpiryReminder'), formItemClass: 'col-span-2' },
    /** 占位：用于在“是否分期”一行上方插入新增发票按钮与发票列表（由 form-page 通过 slot 渲染） */
    { component: 'Input', fieldName: '_invoiceBlock', label: '', formItemClass: 'col-span-4', hideLabel: true },
    { component: 'Select', componentProps: { class: 'w-full', options: [{ label: '否', value: 0 }, { label: '是', value: 1 }] }, fieldName: 'isInstallmentPayment', label: $t('contract.isInstallmentPayment') },
    { component: 'InputNumber', componentProps: { min: 0, class: 'w-full' }, fieldName: 'amount', label: $t('contract.contractAmount'), rules: z.number().min(0) },
    { component: 'InputNumber', componentProps: { min: 0, class: 'w-full' }, fieldName: 'accumulatedAmount', label: $t('contract.accumulatedAmount') },
    { component: 'Textarea', componentProps: { class: 'w-full', rows: 4 }, fieldName: 'note', label: $t('contract.remarks'), formItemClass: 'col-span-4' },
    { component: 'Input', componentProps: { class: 'w-full' }, fieldName: 'partyA', label: $t('contract.partyA'), rules: z.string().min(1) },
    { component: 'Input', componentProps: { class: 'w-full' }, fieldName: 'partyB', label: $t('contract.partyB'), rules: z.string().min(1) },
    { component: 'DatePicker', componentProps: { class: 'w-full', valueFormat: 'YYYY-MM-DD' }, fieldName: 'startDate', label: $t('contract.startDate'), rules: z.string().min(1) },
    { component: 'DatePicker', componentProps: { class: 'w-full', valueFormat: 'YYYY-MM-DD' }, fieldName: 'endDate', label: $t('contract.endDate'), rules: z.string().min(1) },
    /** 附件上传（最后一行单独显示，由 form-page 通过 slot 渲染上传按钮与表格，不显示重复标签） */
    { component: 'Input', fieldName: 'fileStorageKey', label: '', formItemClass: 'col-span-4', hideLabel: true },
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
