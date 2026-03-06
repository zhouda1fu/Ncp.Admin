<script lang="ts" setup>
import type { ContractApi } from '#/api/system/contract';
import type { ContractTypeOptionItem, IncomeExpenseTypeOptionItem } from './data';

import { computed, h, nextTick, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { Page, useVbenDrawer } from '@vben/common-ui';
import { ArrowLeft } from '@vben/icons';

import { Button, Card, Input, message, Popconfirm, Table } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import {
  createContract,
  updateContract,
  getContract,
  createContractInvoice,
  updateContractInvoice,
  removeContractInvoice,
} from '#/api/system/contract';
import type { ContractTypeOptionApi } from '#/api/system/contract-type';
import type { IncomeExpenseTypeOptionApi } from '#/api/system/income-expense-type';
import { getContractTypeOptionList } from '#/api/system/contract-type';
import { getIncomeExpenseTypeOptionList } from '#/api/system/income-expense-type';
import { uploadFile, fetchFileBlob } from '#/api/system/file';
import { $t } from '#/locales';
import FilePreviewModal from '#/components/file-preview/FilePreviewModal.vue';

import { useFormPageSchema, useInvoiceFormSchema, INVOICE_TYPE_OPTIONS } from './data';

const route = useRoute();
const router = useRouter();
const id = computed(() => route.params.id as string | undefined);
const isEdit = computed(() => !!id.value);

const contractTypeOptions = ref<ContractTypeOptionItem[]>([]);
const incomeExpenseTypeOptions = ref<IncomeExpenseTypeOptionItem[]>([]);
const invoicesRef = ref<ContractApi.ContractInvoiceItem[]>([]);
/** 新建合同时的待提交发票（仅前端维护，提交合同时一并创建） */
type PendingInvoiceItem = ContractApi.CreateContractInvoicePayload & { _tempId: string };
const pendingInvoicesRef = ref<PendingInvoiceItem[]>([]);
const editingPendingTempId = ref<string | null>(null);

const invoiceDrawerMode = ref<'add' | 'edit'>('add');
const editingInvoiceId = ref<string | null>(null);
const invoiceDrawerTitle = ref($t('contract.addInvoice'));
interface ContractAttachmentItem {
  path: string;
  fileName: string;
  size: number;
  format: string;
  updatedAt: string;
}
const attachmentFileRef = ref<ContractAttachmentItem | null>(null);
const attachmentInputRef = ref<HTMLInputElement | null>(null);
const attachmentUploading = ref(false);
const attachmentSearchKeyword = ref('');
const previewModalVisible = ref(false);
const previewFilePath = ref('');
const previewFileName = ref('');
const invoiceAttachmentInputRef = ref<HTMLInputElement | null>(null);
const invoiceAttachmentUploading = ref(false);
const invoiceAttachmentRef = ref<{ path: string; fileName: string } | null>(null);

const invoiceTypeOptions = computed(() =>
  INVOICE_TYPE_OPTIONS.map((o) => ({ label: typeof o.label === 'function' ? (o.label as () => string)() : o.label, value: o.value })),
);
const invoiceStatusOptions = computed(() =>
  [
    { label: $t('contract.statusPendingConfirm'), value: 0 },
    { label: $t('contract.statusConfirmed'), value: 1 },
  ],
);

const invoiceFormSchema = computed(() =>
  useInvoiceFormSchema(invoiceTypeOptions.value, invoiceStatusOptions.value),
);
const [InvoiceForm, invoiceFormApi] = useVbenForm({
  layout: 'vertical',
  schema: invoiceFormSchema as unknown as ReturnType<typeof useInvoiceFormSchema>,
  showDefaultActions: false,
  wrapperClass: 'grid-cols-2 gap-y-4',
});

/** 表格展示的发票列表：编辑态用接口数据，新建态用待提交列表 */
const displayInvoices = computed(() =>
  isEdit.value ? invoicesRef.value : (pendingInvoicesRef.value as (ContractApi.ContractInvoiceItem | PendingInvoiceItem)[]),
);
function getInvoiceRowKey(record: ContractApi.ContractInvoiceItem | PendingInvoiceItem) {
  return 'id' in record && record.id ? record.id : (record as PendingInvoiceItem)._tempId;
}

async function doSaveInvoiceFromForm() {
  const data = await invoiceFormApi.getValues();
  const payload = {
    type: Number(data.type) ?? 0,
    invoiceNumber: String(data.invoiceNumber ?? '').trim(),
    taxRate: Number(data.taxRate) || 0,
    amountExclTax: Number(data.amountExclTax) || 0,
    source: String(data.source ?? '').trim(),
    status: data.status === 1,
    title: String(data.title ?? '').trim(),
    taxAmount: Number(data.taxAmount) || 0,
    invoicedAmount: Number(data.invoicedAmount) || 0,
    handler: String(data.handler ?? '').trim(),
    billingDate: data.billingDate ? new Date(String(data.billingDate)).toISOString() : new Date().toISOString(),
    remarks: String(data.remarks ?? ''),
    attachmentStorageKey: String(data.attachmentStorageKey ?? ''),
  };
  if (isEdit.value && id.value) {
    if (invoiceDrawerMode.value === 'edit' && editingInvoiceId.value) {
      await updateContractInvoice(id.value, editingInvoiceId.value, payload);
    } else {
      await createContractInvoice(id.value, payload);
    }
    const detail = await getContract(id.value);
    invoicesRef.value = detail.invoices ?? [];
  } else {
    if (invoiceDrawerMode.value === 'edit' && editingPendingTempId.value) {
      const idx = pendingInvoicesRef.value.findIndex((x) => x._tempId === editingPendingTempId.value);
      if (idx !== -1) pendingInvoicesRef.value[idx] = { ...payload, _tempId: editingPendingTempId.value };
    } else {
      pendingInvoicesRef.value.push({ ...payload, _tempId: crypto.randomUUID() });
    }
  }
  message.success($t('common.success'));
  invoiceDrawerApi.close();
}

const [InvoiceDrawer, invoiceDrawerApi] = useVbenDrawer({
  async onConfirm() {
    const { valid } = await invoiceFormApi.validate();
    if (!valid) return;
    invoiceDrawerApi.lock();
    try {
      await doSaveInvoiceFromForm();
    } catch (e) {
      message.error((e as Error)?.message ?? $t('common.error'));
    } finally {
      invoiceDrawerApi.lock(false);
    }
  },
  onOpenChange(isOpen) {
    if (isOpen) {
      const data = invoiceDrawerApi.getData<{ mode: 'add' | 'edit'; row?: ContractApi.ContractInvoiceItem | PendingInvoiceItem }>();
      const mode = data?.mode ?? 'add';
      invoiceDrawerMode.value = mode;
      invoiceDrawerTitle.value = mode === 'edit' ? $t('contract.edit') : $t('contract.addInvoice');
      if (mode === 'edit' && data?.row) {
        const r = data.row as ContractApi.ContractInvoiceItem;
        const attPath = r.attachmentStorageKey ?? '';
        invoiceAttachmentRef.value = attPath ? { path: attPath, fileName: attPath.split('/').pop() || attPath } : null;
        invoiceFormApi.setValues({
          type: r.type,
          status: r.status ? 1 : 0,
          invoiceNumber: r.invoiceNumber,
          taxRate: r.taxRate,
          amountExclTax: r.amountExclTax,
          source: r.source,
          title: r.title,
          taxAmount: r.taxAmount,
          invoicedAmount: r.invoicedAmount,
          handler: r.handler ?? '',
          billingDate: r.billingDate ? String(r.billingDate).slice(0, 10) : new Date().toISOString().slice(0, 10),
          remarks: r.remarks ?? '',
          attachmentStorageKey: attPath,
        });
      } else {
        invoiceAttachmentRef.value = null;
        invoiceFormApi.setValues({
          type: 0,
          status: 0,
          invoiceNumber: '',
          taxRate: 0,
          amountExclTax: 0,
          source: '',
          title: '',
          taxAmount: 0,
          invoicedAmount: 0,
          handler: '',
          billingDate: new Date().toISOString().slice(0, 10),
          remarks: '',
          attachmentStorageKey: '',
        });
      }
    }
  },
});

function triggerInvoiceAttachmentUpload() {
  invoiceAttachmentInputRef.value?.click();
}

async function onInvoiceAttachmentFileSelected(e: Event) {
  const target = e.target as HTMLInputElement;
  const file = target.files?.[0];
  target.value = '';
  if (!file) return;
  invoiceAttachmentUploading.value = true;
  try {
    const res = await uploadFile(file);
    invoiceAttachmentRef.value = { path: res.path, fileName: file.name };
    invoiceFormApi.setValues({ attachmentStorageKey: res.path });
    message.success($t('common.success'));
  } catch (err) {
    message.error((err as Error)?.message ?? $t('common.error'));
  } finally {
    invoiceAttachmentUploading.value = false;
  }
}

function clearInvoiceAttachment() {
  invoiceAttachmentRef.value = null;
  invoiceFormApi.setValues({ attachmentStorageKey: '' });
}

function previewInvoiceAttachment() {
  if (!invoiceAttachmentRef.value) return;
  previewFilePath.value = invoiceAttachmentRef.value.path;
  previewFileName.value = invoiceAttachmentRef.value.fileName;
  previewModalVisible.value = true;
}

onMounted(async () => {
  const [ctList, ieList] = await Promise.all([
    getContractTypeOptionList(),
    getIncomeExpenseTypeOptionList(),
  ]) as unknown as [ContractTypeOptionApi.ContractTypeOptionItem[], IncomeExpenseTypeOptionApi.IncomeExpenseTypeOptionItem[]];
  contractTypeOptions.value = ctList.map((x) => ({ label: x.name, value: x.typeValue }));
  incomeExpenseTypeOptions.value = ieList.map((x) => ({ label: x.name, value: x.typeValue }));
  if (id.value) {
    const detail = await getContract(id.value);
    await nextTick();
    setFormValues(detail);
    invoicesRef.value = detail.invoices ?? [];
  } else {
    attachmentFileRef.value = null;
    const today = new Date().toISOString().slice(0, 10);
    const orderIdFromQuery = route.query.orderId as string | undefined;
    formApi.setValues({
      status: 0,
      startDate: today,
      endDate: today,
      nextPaymentReminder: false,
      contractExpiryReminder: false,
      isInstallmentPayment: false,
      ...(orderIdFromQuery && { orderId: orderIdFromQuery }),
    });
  }
});

const formSchema = computed(() => useFormPageSchema(contractTypeOptions.value, incomeExpenseTypeOptions.value));

const [Form, formApi] = useVbenForm({
  layout: 'vertical',
  schema: formSchema as unknown as ReturnType<typeof useFormPageSchema>,
  showDefaultActions: false,
  wrapperClass: 'grid-cols-4',
});

function setFormValues(d: ContractApi.ContractItem) {
  const start = d.startDate ? String(d.startDate).slice(0, 10) : '';
  const end = d.endDate ? String(d.endDate).slice(0, 10) : '';
  const sign = d.signDate ? String(d.signDate).slice(0, 10) : '';
  formApi.setValues({
    code: d.code,
    title: d.title,
    partyA: d.partyA,
    partyB: d.partyB,
    amount: d.amount,
    startDate: start,
    endDate: end,
    status: d.status,
    contractType: d.contractType,
    incomeExpenseType: d.incomeExpenseType,
    signDate: sign,
    orderId: d.orderId ?? '',
    customerId: d.customerId ?? '',
    note: d.note ?? '',
    fileStorageKey: d.fileStorageKey ?? '',
    departmentId: d.departmentId ?? '',
    businessManager: d.businessManager ?? '',
    responsibleProject: d.responsibleProject ?? '',
    inputCustomer: d.inputCustomer ?? '',
    nextPaymentReminder: d.nextPaymentReminder ?? false,
    contractExpiryReminder: d.contractExpiryReminder ?? false,
    singleDoubleSeal: d.singleDoubleSeal,
    invoicingInformation: d.invoicingInformation ?? '',
    paymentStatus: d.paymentStatus,
    warrantyPeriod: d.warrantyPeriod ?? '',
    isInstallmentPayment: d.isInstallmentPayment ? 1 : 0,
    accumulatedAmount: d.accumulatedAmount,
  });
  if (d.fileStorageKey) {
    const path = d.fileStorageKey;
    attachmentFileRef.value = {
      path,
      fileName: path.split('/').pop() || path,
      size: 0,
      format: path.split('.').pop() || '',
      updatedAt: '',
    };
  } else {
    attachmentFileRef.value = null;
  }
}

function formatFileSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(0)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(2)} MB`;
}

function getFileIcon(format: string): string {
  const f = (format || '').toLowerCase();
  if (f === 'doc' || f === 'docx') return 'W';
  if (f === 'pdf') return 'PDF';
  if (f === 'xls' || f === 'xlsx') return 'X';
  if (['jpg', 'jpeg', 'png', 'gif', 'bmp', 'webp'].includes(f)) return '图';
  if (f === 'txt') return 'TXT';
  return f || '?';
}

const BLOCKED_EXTS = ['doc', 'xls'];

function triggerAttachmentUpload() {
  attachmentInputRef.value?.click();
}

async function onAttachmentFileSelected(e: Event) {
  const target = e.target as HTMLInputElement;
  const file = target.files?.[0];
  target.value = '';
  if (!file) return;
  const ext = file.name.split('.').pop()?.toLowerCase() ?? '';
  if (BLOCKED_EXTS.includes(ext)) {
    message.warning('不支持 .doc/.xls 旧格式文件，请转换为 .docx/.xlsx 后重新上传');
    return;
  }
  attachmentUploading.value = true;
  try {
    const res = await uploadFile(file);
    attachmentFileRef.value = {
      path: res.path,
      fileName: file.name,
      size: file.size,
      format: ext,
      updatedAt: new Date().toISOString(),
    };
    formApi.setValues({ fileStorageKey: res.path });
    message.success($t('common.success'));
  } catch (err) {
    message.error((err as Error)?.message ?? $t('common.error'));
  } finally {
    attachmentUploading.value = false;
  }
}

function refreshAttachmentList() {
  attachmentSearchKeyword.value = '';
}

const filteredAttachmentList = computed(() => {
  const item = attachmentFileRef.value;
  if (!item) return [];
  const kw = attachmentSearchKeyword.value.trim().toLowerCase();
  if (!kw) return [item];
  return item.fileName.toLowerCase().includes(kw) || (item.format && item.format.toLowerCase().includes(kw)) ? [item] : [];
});

function previewAttachmentFile(record: ContractAttachmentItem) {
  previewFilePath.value = record.path;
  previewFileName.value = record.fileName;
  previewModalVisible.value = true;
}

async function downloadAttachmentFile(record: ContractAttachmentItem) {
  try {
    const blob = await fetchFileBlob(record.path);
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = record.fileName;
    a.click();
    URL.revokeObjectURL(url);
  } catch (err) {
    message.error((err as Error)?.message ?? $t('common.error'));
  }
}

async function printAttachmentFile(record: ContractAttachmentItem) {
  try {
    const blob = await fetchFileBlob(record.path);
    const url = URL.createObjectURL(blob);
    const w = window.open(url, '_blank');
    if (w) w.addEventListener('load', () => { w.print(); });
    setTimeout(() => URL.revokeObjectURL(url), 10000);
  } catch (err) {
    message.error((err as Error)?.message ?? $t('common.error'));
  }
}

function removeAttachmentFile(record: ContractAttachmentItem) {
  if (attachmentFileRef.value?.path === record.path) {
    attachmentFileRef.value = null;
    formApi.setValues({ fileStorageKey: '' });
  }
}

const attachmentFileColumns = [
  { title: () => $t('order.contractFileType'), dataIndex: 'format', key: 'type', width: 70 },
  { title: () => $t('order.contractFileTitle'), dataIndex: 'fileName', key: 'fileName', ellipsis: true },
  { title: () => $t('order.contractFileFormat'), dataIndex: 'format', key: 'format', width: 80 },
  { title: () => $t('order.contractFileSize'), dataIndex: 'size', key: 'size', width: 90 },
  { title: () => $t('order.contractFileUpdated'), dataIndex: 'updatedAt', key: 'updatedAt', width: 165 },
  { title: () => $t('order.contractFileStatus'), key: 'status', width: 80 },
  { title: () => $t('order.operation'), key: 'action', width: 220, fixed: 'right' as const, ellipsis: false },
];

function openAddInvoiceDrawer() {
  editingInvoiceId.value = null;
  editingPendingTempId.value = null;
  invoiceDrawerApi.setData({ mode: 'add' }).open();
}

function openEditInvoiceDrawer(row: ContractApi.ContractInvoiceItem | PendingInvoiceItem) {
  const isPending = '_tempId' in row;
  if (isPending) {
    editingInvoiceId.value = null;
    editingPendingTempId.value = (row as PendingInvoiceItem)._tempId;
  } else {
    editingInvoiceId.value = (row as ContractApi.ContractInvoiceItem).id;
    editingPendingTempId.value = null;
  }
  invoiceDrawerApi.setData({ mode: 'edit', row }).open();
}

async function doRemoveInvoice(record: ContractApi.ContractInvoiceItem | PendingInvoiceItem) {
  const isPending = '_tempId' in record;
  if (isPending) {
    pendingInvoicesRef.value = pendingInvoicesRef.value.filter((x) => x._tempId !== (record as PendingInvoiceItem)._tempId);
    message.success($t('common.success'));
    return;
  }
  if (!id.value) return;
  try {
    await removeContractInvoice(id.value, (record as ContractApi.ContractInvoiceItem).id);
    message.success($t('common.success'));
    const detail = await getContract(id.value);
    invoicesRef.value = detail.invoices ?? [];
  } catch (e) {
    message.error((e as Error)?.message ?? $t('common.error'));
  }
}

const invoiceColumns = computed(() => [
  { title: () => $t('contract.invoiceType'), dataIndex: 'type', key: 'type', width: 140, customRender: ({ text }: { text: number }) => invoiceTypeOptions.value.find((o) => o.value === text)?.label ?? text },
  { title: () => $t('contract.invoiceNumber'), dataIndex: 'invoiceNumber', key: 'invoiceNumber', width: 140 },
  { title: () => $t('contract.invoiceTitle'), dataIndex: 'title', key: 'title', ellipsis: true },
  { title: () => $t('contract.invoicedAmount'), dataIndex: 'invoicedAmount', key: 'invoicedAmount', width: 110 },
  { title: () => $t('contract.invoiceStatus'), dataIndex: 'status', key: 'status', width: 90, customRender: ({ text }: { text: boolean }) => (text ? $t('contract.statusConfirmed') : $t('contract.statusPendingConfirm')) },
  {
    title: () => $t('contract.operation'),
    key: 'action',
    width: 140,
    fixed: 'right' as const,
    customRender: ({ record }: { record: ContractApi.ContractInvoiceItem | PendingInvoiceItem }) => [
      h(Button, { type: 'link', size: 'small', onClick: () => openEditInvoiceDrawer(record) }, () => $t('contract.edit')),
      h(
        Popconfirm,
        { title: $t('common.confirmDelete'), onConfirm: () => doRemoveInvoice(record) },
        { default: () => h(Button, { type: 'link', size: 'small', danger: true }, () => $t('common.delete')) },
      ),
    ],
  },
]);

async function onSubmit() {
  const { valid } = await formApi.validate();
  if (!valid) return;
  const data = await formApi.getValues();
  const payload = {
    code: String(data.code ?? ''),
    title: String(data.title ?? ''),
    partyA: String(data.partyA ?? ''),
    partyB: String(data.partyB ?? ''),
    amount: Number(data.amount) ?? 0,
    startDate: data.startDate ? new Date(data.startDate).toISOString() : new Date().toISOString(),
    endDate: data.endDate ? new Date(data.endDate).toISOString() : new Date().toISOString(),
    fileStorageKey: data.fileStorageKey ? String(data.fileStorageKey) : undefined,
    contractType: data.contractType != null ? Number(data.contractType) : 0,
    incomeExpenseType: data.incomeExpenseType != null ? Number(data.incomeExpenseType) : 0,
    signDate: data.signDate ? new Date(data.signDate).toISOString() : undefined,
    note: data.note ? String(data.note) : undefined,
    description: data.description ? String(data.description) : undefined,
    orderId: data.orderId ? String(data.orderId) : undefined,
    customerId: data.customerId ? String(data.customerId) : undefined,
    departmentId: data.departmentId ? String(data.departmentId) : undefined,
    businessManager: data.businessManager ? String(data.businessManager) : undefined,
    responsibleProject: data.responsibleProject ? String(data.responsibleProject) : undefined,
    inputCustomer: data.inputCustomer ? String(data.inputCustomer) : undefined,
    nextPaymentReminder: Boolean(data.nextPaymentReminder),
    contractExpiryReminder: Boolean(data.contractExpiryReminder),
    singleDoubleSeal: data.singleDoubleSeal != null ? Number(data.singleDoubleSeal) : undefined,
    invoicingInformation: data.invoicingInformation ? String(data.invoicingInformation) : undefined,
    paymentStatus: data.paymentStatus != null ? Number(data.paymentStatus) : undefined,
    warrantyPeriod: data.warrantyPeriod ? String(data.warrantyPeriod) : undefined,
    isInstallmentPayment: data.isInstallmentPayment === 1 || data.isInstallmentPayment === true,
    accumulatedAmount: data.accumulatedAmount != null ? Number(data.accumulatedAmount) : undefined,
  };
  try {
    if (isEdit.value && id.value) {
      await updateContract(id.value, payload);
      message.success($t('common.success'));
      router.push('/contract/list');
      return;
    }
    const res = await createContract(payload);
    const newId = (res as { id?: string })?.id;
    if (newId && pendingInvoicesRef.value.length > 0) {
      for (const inv of pendingInvoicesRef.value) {
        const { _tempId: _, ...invoicePayload } = inv;
        await createContractInvoice(newId, invoicePayload);
      }
    }
    message.success($t('common.success'));
    router.push('/contract/list');
  } catch (e) {
    message.error((e as Error)?.message ?? $t('common.error'));
  }
}

function goBack() {
  router.push('/contract/list');
}
</script>

<template>
  <Page auto-content-height>
    <div class="px-4 py-4">
      <div class="mb-4 flex items-center gap-2">
        <Button type="text" class="inline-flex items-center gap-1" @click="goBack">
          <ArrowLeft class="size-5" />
          {{ $t('common.back') }}
        </Button>
      </div>

      <Card :title="isEdit ? $t('contract.edit') : $t('contract.create')" class="mb-4">
        <Form>
          <template #_invoiceBlock>
            <!-- 新增发票按钮与发票列表（放在“是否分期”一行上方，由 schema 占位 + slot 插入） -->
            <div class="mb-4">
              <div class="mb-2 flex items-center gap-2">
                <Button type="primary" @click="openAddInvoiceDrawer">
                  {{ $t('contract.addInvoice') }}
                </Button>
              </div>
              <Table
                v-if="displayInvoices.length > 0"
                :columns="invoiceColumns as any"
                :data-source="displayInvoices"
                :pagination="false"
                size="small"
                :row-key="getInvoiceRowKey"
              />
            </div>
          </template>
          <template #fileStorageKey>
            <div>
              <div class="mb-2 flex flex-wrap items-center gap-2">
                <Button type="primary" danger size="small" :loading="attachmentUploading" @click="triggerAttachmentUpload">
                  + {{ $t('contract.attachmentUpload') }}
                </Button>
                <input
                  ref="attachmentInputRef"
                  type="file"
                  class="hidden"
                  accept=".docx,.pdf,.xlsx,.jpg,.jpeg,.png,.gif,.bmp,.txt"
                  @change="onAttachmentFileSelected"
                />
                <Button size="small" @click="refreshAttachmentList">{{ $t('order.refresh') }}</Button>
                <Input
                  v-model:value="attachmentSearchKeyword"
                  size="small"
                  class="ml-auto w-40"
                  :placeholder="$t('order.searchPlaceholder')"
                  allow-clear
                />
              </div>
              <Table
                :columns="attachmentFileColumns as any"
                :data-source="filteredAttachmentList"
                :pagination="false"
                size="small"
                bordered
                row-key="path"
              >
                <template #bodyCell="{ column, record }">
                  <template v-if="column.key === 'type'">
                    <span class="inline-flex h-6 w-6 items-center justify-center rounded bg-blue-100 text-xs font-bold text-blue-700">
                      {{ getFileIcon((record as ContractAttachmentItem).format) }}
                    </span>
                  </template>
                  <template v-else-if="column.key === 'size'">
                    {{ (record as ContractAttachmentItem).size ? formatFileSize((record as ContractAttachmentItem).size) : '—' }}
                  </template>
                  <template v-else-if="column.key === 'updatedAt'">
                    {{ (record as ContractAttachmentItem).updatedAt ? new Date((record as ContractAttachmentItem).updatedAt).toLocaleString() : '—' }}
                  </template>
                  <template v-else-if="column.key === 'status'">
                    <span class="rounded bg-blue-100 px-1.5 py-0.5 text-xs text-blue-700">{{ $t('order.statusGeneral') }}</span>
                  </template>
                  <template v-else-if="column.key === 'action'">
                    <div class="flex flex-wrap gap-x-1 gap-y-0.5">
                      <Button type="link" size="small" class="p-0 min-w-0 h-auto leading-normal" @click="previewAttachmentFile(record as ContractAttachmentItem)">{{ $t('order.preview') }}</Button>
                      <Button type="link" size="small" class="p-0 min-w-0 h-auto leading-normal" @click="downloadAttachmentFile(record as ContractAttachmentItem)">{{ $t('order.download') }}</Button>
                      <Button type="link" size="small" class="p-0 min-w-0 h-auto leading-normal" @click="printAttachmentFile(record as ContractAttachmentItem)">{{ $t('order.print') }}</Button>
                      <Button type="link" size="small" danger class="p-0 min-w-0 h-auto leading-normal" @click="removeAttachmentFile(record as ContractAttachmentItem)">{{ $t('order.delete') }}</Button>
                    </div>
                  </template>
                </template>
              </Table>
            </div>
          </template>
        </Form>
        <div class="mt-4 flex gap-2">
          <Button type="primary" @click="onSubmit">
            {{ $t('common.confirm') }}
          </Button>
          <Button @click="goBack">
            {{ $t('common.cancel') }}
          </Button>
        </div>
      </Card>
    </div>

    <!-- 发票抽屉：与主题一致（useVbenDrawer + useVbenForm） -->
    <InvoiceDrawer :title="invoiceDrawerTitle" class="!w-[720px]">
      <InvoiceForm class="mx-4">
        <template #attachmentStorageKey>
          <div class="flex flex-wrap items-center gap-2">
            <Button type="primary" size="small" :loading="invoiceAttachmentUploading" @click="triggerInvoiceAttachmentUpload">
              + {{ $t('contract.attachmentUpload') }}
            </Button>
            <input
              ref="invoiceAttachmentInputRef"
              type="file"
              class="hidden"
              accept=".docx,.pdf,.xlsx,.jpg,.jpeg,.png,.gif,.bmp,.txt"
              @change="onInvoiceAttachmentFileSelected"
            />
            <template v-if="invoiceAttachmentRef">
              <span class="text-muted-foreground text-sm">{{ invoiceAttachmentRef.fileName }}</span>
              <Button type="link" size="small" class="p-0 min-w-0 h-auto leading-normal" @click="previewInvoiceAttachment">{{ $t('order.preview') }}</Button>
              <Button type="link" size="small" danger class="p-0 min-w-0 h-auto leading-normal" @click="clearInvoiceAttachment">{{ $t('order.delete') }}</Button>
            </template>
          </div>
        </template>
      </InvoiceForm>
    </InvoiceDrawer>
    <FilePreviewModal
      v-model:open="previewModalVisible"
      :file-path="previewFilePath"
      :file-name="previewFileName"
    />
  </Page>
</template>
