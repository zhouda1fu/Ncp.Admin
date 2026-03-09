<script lang="ts" setup>
import { computed, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';
import { ArrowLeft, Search } from '@vben/icons';

import {
  Button,
  Card,
  DatePicker,
  Input,
  InputNumber,
  message,
  Select,
  Table,
  Tooltip,
  TreeSelect,
} from 'ant-design-vue';

import type { OrderApi } from '#/api/system/order';
import { createOrder, getOrder, updateOrder } from '#/api/system/order';
import { getContractList } from '#/api/system/contract';
import type { ContractTypeOptionApi } from '#/api/system/contract-type';
import { getContractTypeOptionList } from '#/api/system/contract-type';
import type { CustomerApi } from '#/api/system/customer';
import { getCustomer } from '#/api/system/customer';
import { getDeptTree } from '#/api/system/dept';
import type { SystemDeptApi } from '#/api/system/dept';
import { fetchFileBlob, uploadFile } from '#/api/system/file';
import { getProductList } from '#/api/system/product';
import { getProjectList } from '#/api/system/project';
import { getUserList } from '#/api/system/user';
import { $t } from '#/locales';
import { OrderTypeEnum } from '#/api/system/order';
import FilePreviewModal from '#/components/file-preview/FilePreviewModal.vue';
import CustomerSelectModal from '#/views/task/projects/modules/customer-select-modal.vue';

const route = useRoute();
const router = useRouter();

const id = computed(() => route.params.id as string | undefined);
const isNew = computed(() => !id.value);

const loading = ref(false);
const submitting = ref(false);
const userOptions = ref<{ label: string; value: string }[]>([]);
const customerSelectModalRef = ref<InstanceType<typeof CustomerSelectModal> | null>(null);
const selectedCustomerName = ref('');
const contractOptions = ref<{ label: string; value: string }[]>([]);
/** 合同签订公司选项：来自合同类型中「订单签订公司选项展示」为是的项 */
const contractSigningCompanyOptions = ref<{ label: string; value: string }[]>([]);
const projectOptions = ref<{ label: string; value: string }[]>([]);
const productOptions = ref<{ label: string; value: string; name: string; model: string; unit: string }[]>([]);
/** 部门树（原始接口数据，用于按 id 查名称） */
const deptTreeRaw = ref<SystemDeptApi.SystemDept[]>([]);
/** 部门树选项（TreeSelect tree-data 格式） */
const deptTreeData = ref<{ title: string; value: string; key: string; children?: any[] }[]>([]);

const paymentStatusOptions = [
  { label: '已到款', value: '已到款' },
  { label: '未到款', value: '未到款' },
  { label: '部分到款', value: '部分到款' },
  { label: '待确认', value: '待确认' },
];

const form = ref({
  customerId: '',
  customerName: '',
  projectId: '' as string,
  contractId: '' as string,
  orderNumber: '',
  type: OrderTypeEnum.Sales as number,
  status: 1,
  amount: 0,
  remark: '',
  ownerId: '',
  ownerName: '',
  deptId: '' as string,
  deptName: '' as string,
  projectContactName: '',
  projectContactPhone: '',
  warranty: '' as string,
  contractSigningCompany: '' as string,
  contractTrustee: '' as string,
  needInvoice: false,
  installationFee: 0,
  estimatedFreight: 0,
  contractFiles: [] as OrderApi.OrderContractFileItem[],
  selectedContractFileId: '' as string,
  isShipped: false,
  paymentStatus: '' as string,
  contractNotCompanyTemplate: false,
  contractDiscount: 0,
  contractAmount: 0,
  receiverName: '',
  receiverPhone: '',
  receiverAddress: '',
  payDate: '' as string,
  deliveryDate: '' as string,
  items: [] as {
    productId: string;
    productName: string;
    model: string;
    number: string;
    qty: number;
    unit: string;
    price: number;
    amount: number;
    remark: string;
  }[],
});

const itemColumns = [
  { title: () => $t('order.product'), dataIndex: 'productId', width: 180, key: 'productId' },
  { title: () => $t('order.productName'), dataIndex: 'productName', width: 140, key: 'productName' },
  { title: () => $t('order.model'), dataIndex: 'model', width: 100, key: 'model' },
  { title: () => $t('order.number'), dataIndex: 'number', width: 90, key: 'number' },
  { title: () => $t('order.qty'), dataIndex: 'qty', width: 70, key: 'qty' },
  { title: () => $t('order.unit'), dataIndex: 'unit', width: 60, key: 'unit' },
  { title: () => $t('order.price'), dataIndex: 'price', width: 90, key: 'price' },
  { title: () => $t('order.itemAmount'), dataIndex: 'amount', width: 90, key: 'amount' },
  { title: () => $t('order.remark'), dataIndex: 'remark', key: 'remark', ellipsis: true },
  { title: () => $t('order.operation'), key: 'action', width: 80, fixed: 'right' as const },
];

function addItem() {
  form.value.items.push({
    productId: '',
    productName: '',
    model: '',
    number: '',
    qty: 1,
    unit: '台',
    price: 0,
    amount: 0,
    remark: '',
  });
}

function onProductSelect(val: string, index: number) {
  const opt = productOptions.value.find((o) => o.value === val);
  const item = form.value.items[index];
  if (opt && item) {
    item.productName = opt.name;
    item.model = opt.model;
    item.unit = opt.unit;
  }
}

function removeItem(index: number) {
  form.value.items.splice(index, 1);
}

/** 将部门接口树转为 TreeSelect treeData */
function buildDeptTreeData(nodes: SystemDeptApi.SystemDept[]): { title: string; value: string; key: string; children?: any[] }[] {
  return nodes.map((d) => ({
    title: d.name ?? String(d.id),
    value: String(d.id),
    key: String(d.id),
    children: d.children && d.children.length > 0 ? buildDeptTreeData(d.children) : undefined,
  }));
}

/** 从部门树中根据 id 查部门名称 */
function findDeptName(nodes: SystemDeptApi.SystemDept[], deptId: string): string {
  for (const d of nodes) {
    if (String(d.id) === String(deptId)) return d.name ?? '';
    if (d.children?.length) {
      const found = findDeptName(d.children, deptId);
      if (found) return found;
    }
  }
  return '';
}

function openCustomerSelectModal() {
  customerSelectModalRef.value?.open({
    onSelect(row: CustomerApi.CustomerItem) {
      form.value.customerId = row.id;
      const label = row.shortName ? `${row.fullName}（${row.shortName}）` : row.fullName;
      form.value.customerName = row.fullName;
      selectedCustomerName.value = label;
    },
  });
}

function recalcItemAmount(index: number) {
  const item = form.value.items[index];
  if (item) {
    item.amount = Number((item.qty * item.price).toFixed(2));
  }
  form.value.amount = form.value.items.reduce((sum, i) => sum + (i?.amount ?? 0), 0);
}

async function loadDetail() {
  if (!id.value) return;
  loading.value = true;
  try {
    const data = await getOrder(id.value);
    form.value = {
      customerId: data.customerId,
      customerName: data.customerName,
      projectId: data.projectId ?? '',
      contractId: data.contractId ?? '',
      orderNumber: data.orderNumber,
      type: data.type,
      status: data.status,
      amount: data.amount,
      remark: data.remark ?? '',
      ownerId: data.ownerId,
      ownerName: data.ownerName ?? '',
      deptId: data.deptId ?? '',
      deptName: data.deptName ?? '',
      projectContactName: data.projectContactName ?? '',
      projectContactPhone: data.projectContactPhone ?? '',
      warranty: data.warranty ?? '',
      contractSigningCompany: data.contractSigningCompany ?? '',
      contractTrustee: data.contractTrustee ?? '',
      needInvoice: data.needInvoice ?? false,
      installationFee: Number(data.installationFee ?? 0),
      estimatedFreight: Number(data.estimatedFreight ?? 0),
      contractFiles: (data.contractFiles ?? []).map((f: OrderApi.OrderContractFileItem) => ({
        path: f.path,
        fileName: f.fileName,
        size: f.size,
        format: f.format,
        updatedAt: f.updatedAt,
      })),
      selectedContractFileId: data.selectedContractFileId ?? '',
      isShipped: data.isShipped ?? false,
      paymentStatus: data.paymentStatus ?? '',
      contractNotCompanyTemplate: data.contractNotCompanyTemplate ?? false,
      contractDiscount: Number(data.contractDiscount ?? 0),
      contractAmount: Number(data.contractAmount ?? 0),
      receiverName: data.receiverName ?? '',
      receiverPhone: data.receiverPhone ?? '',
      receiverAddress: data.receiverAddress ?? '',
      payDate: data.payDate ? data.payDate.slice(0, 10) : '',
      deliveryDate: data.deliveryDate ? data.deliveryDate.slice(0, 10) : '',
      items: (data.items ?? []).map((i) => ({
        productId: i.productId ?? '',
        productName: i.productName,
        model: i.model ?? '',
        number: i.number ?? '',
        qty: i.qty,
        unit: i.unit ?? '',
        price: Number(i.price),
        amount: Number(i.amount),
        remark: i.remark ?? '',
      })),
    };
    if (data.customerId) {
      try {
        const customerDetail = await getCustomer(data.customerId);
        selectedCustomerName.value = customerDetail.shortName
          ? `${customerDetail.fullName}（${customerDetail.shortName}）`
          : customerDetail.fullName;
      } catch {
        selectedCustomerName.value = data.customerName ?? '';
      }
    } else {
      selectedCustomerName.value = '';
    }
  } finally {
    loading.value = false;
  }
}

onMounted(() => {
  getUserList({ pageIndex: 1, pageSize: 500 }).then((res) => {
    userOptions.value = (res.items ?? []).map((x) => ({
      label: x.realName || x.name || x.userId,
      value: String(x.userId),
    }));
  });
  getContractList({ pageIndex: 1, pageSize: 500 }).then((res) => {
    contractOptions.value = (res.items ?? []).map((x) => ({
      label: `${x.code} - ${x.title}`,
      value: x.id,
    }));
  });
  getContractTypeOptionList().then((raw) => {
    const list = Array.isArray(raw) ? (Array.isArray(raw[0]) ? (raw as ContractTypeOptionApi.ContractTypeOptionItem[][]).flat() : raw as ContractTypeOptionApi.ContractTypeOptionItem[]) : [];
    contractSigningCompanyOptions.value = list
      .filter((x) => x.orderSigningCompanyOptionDisplay === true)
      .map((x) => ({ label: x.name, value: x.name }));
  });
  getProjectList({ pageIndex: 1, pageSize: 500 }).then((res) => {
    projectOptions.value = (res.items ?? []).map((x) => ({
      label: x.name || x.id,
      value: x.id,
    }));
  });
  getProductList({ pageIndex: 1, pageSize: 1000 }).then((res) => {
    productOptions.value = (res.items ?? []).map((x) => ({
      label: `${x.code} - ${x.name}`,
      value: x.id,
      name: x.name,
      model: x.model,
      unit: x.unit,
    }));
  });
  getDeptTree().then((res) => {
    const list = Array.isArray(res) ? res : (res as any)?.data ?? [];
    deptTreeRaw.value = list as SystemDeptApi.SystemDept[];
    deptTreeData.value = buildDeptTreeData(deptTreeRaw.value);
  });
  if (!isNew.value) loadDetail();
});

function toIsoDate(v: string): string {
  if (!v) return new Date().toISOString();
  const d = new Date(v);
  return isNaN(d.getTime()) ? new Date().toISOString() : d.toISOString();
}

async function onSubmit() {
  if (!form.value.customerId || !form.value.orderNumber || !form.value.ownerId) {
    message.warning($t('ui.formRules.required', [$t('order.customer')]));
    return;
  }
  if (!form.value.projectId) {
    message.warning($t('ui.formRules.required', [$t('order.project')]));
    return;
  }
  if (!form.value.contractId) {
    message.warning($t('ui.formRules.required', [$t('order.contract')]));
    return;
  }
  if (!form.value.deptId) {
    message.warning($t('ui.formRules.required', [$t('order.dept')]));
    return;
  }
  if (!form.value.payDate || !form.value.deliveryDate) {
    message.warning($t('ui.formRules.required', [$t('order.payDate')]));
    return;
  }
  if (!form.value.items.length || form.value.items.some((i) => !i.productId)) {
    message.warning($t('ui.formRules.required', [$t('order.items')]));
    return;
  }
  submitting.value = true;
  try {
    const payload = {
      customerId: form.value.customerId,
      customerName: form.value.customerName,
      projectId: form.value.projectId,
      contractId: form.value.contractId,
      orderNumber: form.value.orderNumber,
      type: form.value.type,
      status: form.value.status,
      amount: form.value.amount,
      remark: form.value.remark ?? '',
      ownerId: form.value.ownerId,
      ownerName: form.value.ownerName ?? '',
      deptId: form.value.deptId,
      deptName: form.value.deptName ?? '',
      projectContactName: form.value.projectContactName ?? '',
      projectContactPhone: form.value.projectContactPhone ?? '',
      warranty: form.value.warranty ?? '',
      contractSigningCompany: form.value.contractSigningCompany ?? '',
      contractTrustee: form.value.contractTrustee ?? '',
      needInvoice: form.value.needInvoice,
      installationFee: form.value.installationFee,
      estimatedFreight: form.value.estimatedFreight,
      contractFiles: form.value.contractFiles,
      selectedContractFileId: form.value.selectedContractFileId ?? '',
      isShipped: form.value.isShipped,
      paymentStatus: form.value.paymentStatus ?? '',
      contractNotCompanyTemplate: form.value.contractNotCompanyTemplate,
      contractDiscount: form.value.contractDiscount,
      contractAmount: form.value.contractAmount,
      receiverName: form.value.receiverName ?? '',
      receiverPhone: form.value.receiverPhone ?? '',
      receiverAddress: form.value.receiverAddress ?? '',
      payDate: toIsoDate(form.value.payDate),
      deliveryDate: toIsoDate(form.value.deliveryDate),
      items: form.value.items.map((i) => ({
        productId: i.productId,
        productName: i.productName,
        model: i.model ?? '',
        number: i.number ?? '',
        qty: i.qty,
        unit: i.unit ?? '',
        price: i.price,
        amount: i.amount,
        remark: i.remark ?? '',
      })),
    };
    if (isNew.value) {
      await createOrder(payload);
      message.success($t('common.success'));
      router.push('/order/list');
    } else {
      await updateOrder({ ...payload, id: id.value });
      message.success($t('common.success'));
      router.push('/order/list');
    }
  } catch (e) {
    message.error(String(e));
  } finally {
    submitting.value = false;
  }
}

function goBack() {
  router.push('/order/list');
}

function goContractList() {
  if (id.value) router.push(`/contract/list?orderId=${id.value}`);
}

function goToDetail() {
  if (id.value) router.push(`/order/${id.value}`);
}

const contractFileInputRef = ref<HTMLInputElement | null>(null);
const contractFileSearchKeyword = ref('');
const previewModalVisible = ref(false);
const previewFilePath = ref('');
const previewFileName = ref('');

function triggerContractUpload() {
  contractFileInputRef.value?.click();
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

async function onContractFileSelected(e: Event) {
  const target = e.target as HTMLInputElement;
  const file = target.files?.[0];
  target.value = '';
  if (!file) return;
  const ext = file.name.split('.').pop()?.toLowerCase() ?? '';
  if (BLOCKED_EXTS.includes(ext)) {
    message.warning('不支持 .doc/.xls 旧格式文件，请转换为 .docx/.xlsx 后重新上传');
    return;
  }
  try {
    const res = await uploadFile(file);
    form.value.contractFiles.push({
      path: res.path,
      fileName: file.name,
      size: file.size,
      format: ext,
      updatedAt: new Date().toISOString(),
    });
    message.success($t('common.success'));
  } catch (err) {
    message.error(String(err));
  }
}

function refreshContractList() {
  contractFileSearchKeyword.value = '';
}

const filteredContractFiles = computed(() => {
  const kw = contractFileSearchKeyword.value.trim().toLowerCase();
  if (!kw) return form.value.contractFiles;
  return form.value.contractFiles.filter(
    (f) => f.fileName.toLowerCase().includes(kw) || (f.format && f.format.toLowerCase().includes(kw)),
  );
});

function previewContractFile(item: OrderApi.OrderContractFileItem) {
  previewFilePath.value = item.path;
  previewFileName.value = item.fileName;
  previewModalVisible.value = true;
}

async function downloadContractFile(item: OrderApi.OrderContractFileItem) {
  try {
    const blob = await fetchFileBlob(item.path);
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = item.fileName;
    a.click();
    URL.revokeObjectURL(url);
  } catch (err) {
    message.error(String(err));
  }
}

async function printContractFile(item: OrderApi.OrderContractFileItem) {
  try {
    const blob = await fetchFileBlob(item.path);
    const url = URL.createObjectURL(blob);
    const w = window.open(url, '_blank');
    if (w) w.addEventListener('load', () => { w.print(); });
    setTimeout(() => URL.revokeObjectURL(url), 10000);
  } catch (err) {
    message.error(String(err));
  }
}

function removeContractFile(item: OrderApi.OrderContractFileItem) {
  form.value.contractFiles = form.value.contractFiles.filter((f) => f.path !== item.path);
}

const contractFileColumns = [
  { title: () => $t('order.contractFileType'), dataIndex: 'format', key: 'type', width: 70 },
  { title: () => $t('order.contractFileTitle'), dataIndex: 'fileName', key: 'fileName', ellipsis: true },
  { title: () => $t('order.contractFileFormat'), dataIndex: 'format', key: 'format', width: 80 },
  { title: () => $t('order.contractFileSize'), dataIndex: 'size', key: 'size', width: 90 },
  { title: () => $t('order.contractFileUpdated'), dataIndex: 'updatedAt', key: 'updatedAt', width: 165 },
  { title: () => $t('order.contractFileStatus'), key: 'status', width: 80 },
  { title: () => $t('order.operation'), key: 'action', width: 220, fixed: 'right' as const, ellipsis: false },
];
</script>

<template>
  <Page auto-content-height :loading="loading">
    <div class="p-4">
      <!-- 顶部操作栏 -->
      <div class="mb-4 flex flex-wrap items-center gap-2">
        <div class="flex items-center gap-2">
          <Button @click="goBack">
            <ArrowLeft class="size-4" />
          </Button>
          <span class="text-lg font-medium text-foreground">{{ isNew ? $t('order.create') : $t('order.edit') }}</span>
          <Button v-if="!isNew" size="small" @click="goContractList">{{ $t('contract.list') }}</Button>
        </div>
        <template v-if="!isNew && id">
          <Button type="link" class="p-0" @click="goToDetail">
            {{ $t('order.changeRecord') }}
          </Button>
        </template>
        <Tooltip :title="$t('order.helpTip')">
          <span class="cursor-help text-muted-foreground" style="font-size: 1rem">?</span>
        </Tooltip>
      </div>

      <!-- 营销中心 -->
      <div class="border-border pt-2 first:pt-0">
        <h4 class="order-section-title">{{ $t('order.sectionMarketing') }}</h4>
        <div class="grid gap-4 md:grid-cols-4">
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground"><span class="mr-[2px] text-destructive">*</span>{{ $t('order.type') }}</label>
            <Select
              v-model:value="form.type"
              class="w-full"
              :options="[
                { label: $t('order.typeSales'), value: 0 },
                { label: $t('order.typeAfterSales'), value: 1 },
                { label: $t('order.typeSample'), value: 2 },
                { label: $t('order.typeGeneralTest'), value: 3 },
              ]"
            />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground"><span class="mr-[2px] text-destructive">*</span>{{ $t('order.status') }}</label>
            <Select
              v-model:value="form.status"
              class="w-full"
              :options="[
                { label: $t('order.statusPendingAudit'), value: 1 },
                { label: $t('order.statusOrdered'), value: 2 },
                { label: $t('order.statusCompleted'), value: 3 },
                { label: $t('order.statusRejected'), value: 4 },
                { label: $t('order.statusUnpaid'), value: 5 },
              ]"
            />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground"><span class="mr-[2px] text-destructive">*</span>{{ $t('order.orderNumber') }}</label>
            <Input v-model:value="form.orderNumber" placeholder="" />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.projectContactName') }}</label>
            <Input v-model:value="form.projectContactName" placeholder="" />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.projectContactPhone') }}</label>
            <Input v-model:value="form.projectContactPhone" placeholder="" />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.warranty') }}</label>
            <Select
              v-model:value="form.warranty"
              allow-clear
              class="w-full"
              :options="[
                { label: '1年', value: '1年' },
                { label: '2年', value: '2年' },
                { label: '3年', value: '3年' },
                { label: '5年', value: '5年' },
                { label: '其他', value: '其他' },
              ]"
              placeholder="--请选择--"
            />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground"><span class="mr-[2px] text-destructive">*</span>{{ $t('order.dept') }}</label>
            <TreeSelect
              v-model:value="form.deptId"
              allow-clear
              show-search
              class="w-full"
              :tree-data="deptTreeData"
              tree-node-filter-prop="title"
              placeholder="--请选择--"
              :filter-tree-node="(inputValue, treeNode) => treeNode?.title?.toString().toLowerCase().includes(inputValue?.toLowerCase())"
              @change="(val: string) => { form.deptName = val ? findDeptName(deptTreeRaw, val) : ''; }"
            />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground"><span class="mr-[2px] text-destructive">*</span>{{ $t('order.owner') }}</label>
            <Select
              v-model:value="form.ownerId"
              allow-clear
              class="w-full"
              :options="userOptions"
              :field-names="{ label: 'label', value: 'value' }"
              @select="(_: unknown, opt: unknown) => { form.ownerName = (opt as { label?: string })?.label ?? ''; }"
            />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground"><span class="mr-[2px] text-destructive">*</span>{{ $t('order.customer') }}</label>
            <Input
              :model-value="selectedCustomerName"
              readonly
              class="w-full cursor-pointer"
              :placeholder="$t('task.project.searchCustomer')"
              @click="openCustomerSelectModal"
            >
              <template #suffix>
                <span
                  class="cursor-pointer text-gray-500 hover:text-primary transition-colors"
                  role="button"
                  tabindex="0"
                  @click.stop="openCustomerSelectModal"
                  @keydown.enter.prevent="openCustomerSelectModal"
                >
                  <Search class="size-4" />
                </span>
              </template>
            </Input>
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground"><span class="mr-[2px] text-destructive">*</span>{{ $t('order.project') }}</label>
            <Select
              v-model:value="form.projectId"
              allow-clear
              class="w-full"
              :options="projectOptions"
              :field-names="{ label: 'label', value: 'value' }"
              placeholder="请选择项目"
            />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground"><span class="mr-[2px] text-destructive">*</span>{{ $t('order.contractSigningCompany') }}</label>
            <Select
              v-model:value="form.contractSigningCompany"
              allow-clear
              class="w-full"
              :options="contractSigningCompanyOptions"
              :field-names="{ label: 'label', value: 'value' }"
              placeholder="--请选择--"
            />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.contractTrustee') }}</label>
            <Input v-model:value="form.contractTrustee" placeholder="" />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.needInvoice') }}</label>
            <Select
              :value="form.needInvoice ? 1 : 0"
              class="w-full"
              :options="[
                { label: $t('order.yes'), value: 1 },
                { label: $t('order.no'), value: 0 },
              ]"
              @update:value="(v) => { form.needInvoice = v === 1; }"
            />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground"><span class="mr-[2px] text-destructive">*</span>{{ $t('order.installationFee') }}</label>
            <InputNumber v-model:value="form.installationFee" class="w-full" :min="0" :precision="2" />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground"><span class="mr-[2px] text-destructive">*</span>{{ $t('order.estimatedFreight') }}</label>
            <InputNumber v-model:value="form.estimatedFreight" class="w-full" :min="0" :precision="2" />
          </div>
          <!-- 合同上传与列表 -->
          <div class="md:col-span-4">
            <div class="mb-2 flex flex-wrap items-center gap-2">
              <Button type="primary" danger size="small" @click="triggerContractUpload">
                + {{ $t('order.contractUpload') }}
              </Button>
              <input
                ref="contractFileInputRef"
                type="file"
                class="hidden"
                accept=".docx,.pdf,.xlsx,.jpg,.jpeg,.png,.gif,.bmp,.txt"
                @change="onContractFileSelected"
              />
              <Button size="small" @click="refreshContractList">{{ $t('order.refresh') }}</Button>
              <Input
                v-model:value="contractFileSearchKeyword"
                size="small"
                class="ml-auto w-40"
                :placeholder="$t('order.searchPlaceholder')"
                allow-clear
              />
            </div>
            <Table
              :columns="contractFileColumns"
              :data-source="filteredContractFiles"
              :pagination="false"
              size="small"
              bordered
              row-key="path"
            >
              <template #bodyCell="{ column, record }">
                <template v-if="column.key === 'type'">
                  <span class="inline-flex h-6 w-6 items-center justify-center rounded bg-blue-100 text-xs font-bold text-blue-700">
                    {{ getFileIcon((record as OrderApi.OrderContractFileItem).format) }}
                  </span>
                </template>
                <template v-else-if="column.key === 'size'">
                  {{ formatFileSize((record as OrderApi.OrderContractFileItem).size) }}
                </template>
                <template v-else-if="column.key === 'updatedAt'">
                  {{ (record as OrderApi.OrderContractFileItem).updatedAt ? new Date((record as OrderApi.OrderContractFileItem).updatedAt).toLocaleString() : '' }}
                </template>
                <template v-else-if="column.key === 'status'">
                  <span class="rounded bg-blue-100 px-1.5 py-0.5 text-xs text-blue-700">{{ $t('order.statusGeneral') }}</span>
                </template>
                <template v-else-if="column.key === 'action'">
                  <div class="flex flex-wrap gap-x-1 gap-y-0.5">
                    <Button type="link" size="small" class="p-0 min-w-0 h-auto leading-normal" @click="previewContractFile(record as OrderApi.OrderContractFileItem)">{{ $t('order.preview') }}</Button>
                    <Button type="link" size="small" class="p-0 min-w-0 h-auto leading-normal" @click="downloadContractFile(record as OrderApi.OrderContractFileItem)">{{ $t('order.download') }}</Button>
                    <Button type="link" size="small" class="p-0 min-w-0 h-auto leading-normal" @click="printContractFile(record as OrderApi.OrderContractFileItem)">{{ $t('order.print') }}</Button>
                    <Button type="link" size="small" danger class="p-0 min-w-0 h-auto leading-normal" @click="removeContractFile(record as OrderApi.OrderContractFileItem)">{{ $t('order.delete') }}</Button>
                  </div>
                </template>
              </template>
            </Table>
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.amount') }}</label>
            <InputNumber v-model:value="form.amount" class="w-full" :min="0" :precision="2" />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground"><span class="mr-[2px] text-destructive">*</span>{{ $t('order.contract') }}</label>
            <Select
              v-model:value="form.contractId"
              allow-clear
              class="w-full"
              :options="contractOptions"
              :field-names="{ label: 'label', value: 'value' }"
              placeholder="请选择关联合同"
            />
          </div>
          <div class="md:col-span-4">
            <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.remark') }}</label>
            <Input.TextArea v-model:value="form.remark" :rows="2" placeholder="" />
          </div>
        </div>
      </div>

      <!-- 财务部 -->
      <div class="border-border mt-4 border-t pt-4">
        <h4 class="order-section-title">{{ $t('order.sectionFinance') }}</h4>
        <div class="grid gap-4 md:grid-cols-4">
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.selectContract') }}</label>
            <Select
              v-model:value="form.selectedContractFileId"
              allow-clear
              class="w-full"
              :options="[
                { label: $t('order.noneOption'), value: '' },
              ]"
              placeholder="--请选择--"
            />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.isShipped') }}</label>
            <Select
              :value="form.isShipped ? 1 : 0"
              class="w-full"
              :options="[
                { label: $t('order.yes'), value: 1 },
                { label: $t('order.no'), value: 0 },
              ]"
              @update:value="(v) => { form.isShipped = v === 1; }"
            />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.paymentStatus') }}</label>
            <Select
              v-model:value="form.paymentStatus"
              allow-clear
              class="w-full"
              :options="paymentStatusOptions"
              placeholder="--请选择--"
            />
          </div>
          <div class="flex items-center">
            <label class="flex cursor-pointer items-center gap-2 text-sm font-medium text-foreground">
              <input
                v-model="form.contractNotCompanyTemplate"
                type="checkbox"
                class="h-4 w-4 rounded border-input"
              />
              {{ $t('order.contractNotCompanyTemplate') }}
            </label>
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.contractDiscount') }}</label>
            <InputNumber v-model:value="form.contractDiscount" class="w-full" :min="0" :precision="2" />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.contractAmount') }}</label>
            <InputNumber v-model:value="form.contractAmount" class="w-full" :min="0" :precision="2" />
          </div>
        </div>
      </div>

      <!-- 物流与收货 -->
      <div class="border-border mt-4 border-t pt-4">
        <h4 class="order-section-title">{{ $t('order.sectionLogistics') }}</h4>
        <div class="grid gap-4 md:grid-cols-2">
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.receiverName') }}</label>
            <Input v-model:value="form.receiverName" />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.receiverPhone') }}</label>
            <Input v-model:value="form.receiverPhone" />
          </div>
          <div class="md:col-span-2">
            <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.receiverAddress') }}</label>
            <Input v-model:value="form.receiverAddress" />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground"><span class="mr-[2px] text-destructive">*</span>{{ $t('order.payDate') }}</label>
            <DatePicker
              v-model:value="form.payDate"
              value-format="YYYY-MM-DD"
              class="w-full"
              style="width: 100%"
              placeholder="选择付款日期"
            />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground"><span class="mr-[2px] text-destructive">*</span>{{ $t('order.deliveryDate') }}</label>
            <DatePicker
              v-model:value="form.deliveryDate"
              value-format="YYYY-MM-DD"
              class="w-full"
              style="width: 100%"
              placeholder="选择发货日期"
            />
          </div>
        </div>
      </div>

      <!-- 产品明细（卡片） -->
      <div class="border-border mt-4 border-t pt-4">
        <Card :bordered="true" class="border-border bg-card">
          <template #title>
            <div class="flex w-full items-center justify-between">
              <span class="order-section-title mb-0">{{ $t('order.sectionProductList') }}</span>
              <Button type="primary" size="small" @click="addItem">{{ $t('order.addItem') }}</Button>
            </div>
          </template>
          <Table
          :columns="itemColumns"
          :data-source="form.items"
          :pagination="false"
          :row-key="(_, i) => String(i)"
          size="small"
          bordered
        >
          <template #bodyCell="{ column, record, index }">
            <template v-if="column.key === 'productId'">
              <Select
                v-model:value="record.productId"
                allow-clear
                show-search
                class="w-full"
                :options="productOptions"
                :field-names="{ label: 'label', value: 'value' }"
                placeholder="选择产品"
                @select="(value: unknown) => onProductSelect(String(value), index)"
              />
            </template>
            <template v-else-if="column.key === 'productName'">
              <Input v-model:value="record.productName" size="small" />
            </template>
            <template v-else-if="column.key === 'model'">
              <Input v-model:value="record.model" size="small" />
            </template>
            <template v-else-if="column.key === 'number'">
              <Input v-model:value="record.number" size="small" />
            </template>
            <template v-else-if="column.key === 'qty'">
              <InputNumber
                v-model:value="record.qty"
                size="small"
                :min="1"
                class="w-full"
                @change="() => recalcItemAmount(index)"
              />
            </template>
            <template v-else-if="column.key === 'unit'">
              <Input v-model:value="record.unit" size="small" />
            </template>
            <template v-else-if="column.key === 'price'">
              <InputNumber
                v-model:value="record.price"
                size="small"
                :min="0"
                :precision="2"
                class="w-full"
                @change="() => recalcItemAmount(index)"
              />
            </template>
            <template v-else-if="column.key === 'amount'">
              {{ record.amount }}
            </template>
            <template v-else-if="column.key === 'remark'">
              <Input v-model:value="record.remark" size="small" />
            </template>
            <template v-else-if="column.key === 'action'">
              <Button type="link" size="small" danger @click="removeItem(index)">{{ $t('order.delete') }}</Button>
            </template>
          </template>
        </Table>
        </Card>
      </div>

      <div class="mt-4 flex gap-2">
        <Button type="primary" :loading="submitting" @click="onSubmit">{{ $t('common.confirm') }}</Button>
        <Button @click="goBack">{{ $t('common.cancel') }}</Button>
      </div>
    </div>
    <CustomerSelectModal ref="customerSelectModalRef" />
    <FilePreviewModal
      v-model:open="previewModalVisible"
      :file-path="previewFilePath"
      :file-name="previewFileName"
    />
  </Page>
</template>

<style scoped>
.order-section-title {
  font-size: 1.125rem;
  font-weight: 600;
  color: #1677ff;
  margin-bottom: 0.75rem;
}
</style>
