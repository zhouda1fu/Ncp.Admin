<script lang="ts" setup>
import { computed, nextTick, onMounted, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { useAccessStore, useUserStore } from '@vben/stores';
import { Page } from '@vben/common-ui';
import { Search } from '@vben/icons';

import {
  Button,
  Card,
  DatePicker,
  Input,
  InputNumber,
  message,
  Modal,
  Select,
  Tag,
  Table,
  TreeSelect,
} from 'ant-design-vue';

import type { OrderApi } from '#/api/system/order';
import {
  createOrder,
  getOrder,
  updateOrder,
  submitOrder,
  getOrderRemarks,
  createOrderRemark,
  updateOrderRemark,
  deleteOrderRemark,
  OrderTypeEnum,
  OrderStatusEnum,
} from '#/api/system/order';
import { getContractList } from '#/api/system/contract';
import type { ContractTypeOptionApi } from '#/api/system/contract-type';
import { getContractTypeOptionList } from '#/api/system/contract-type';
import type { CustomerApi } from '#/api/system/customer';
import { getCustomer } from '#/api/system/customer';
import type { OrderInvoiceTypeOptionApi } from '#/api/system/order-invoice-type';
import { getOrderInvoiceTypeOptionList } from '#/api/system/order-invoice-type';
import { getDeptTree } from '#/api/system/dept';
import type { SystemDeptApi } from '#/api/system/dept';
import { fetchFileBlob, uploadFile } from '#/api/system/file';
import { getProductList } from '#/api/system/product';
import { getProjectList } from '#/api/system/project';
import { getOrderLogisticsCompanyList } from '#/api/system/order-logistics-company';
import { getOrderLogisticsMethodList } from '#/api/system/order-logistics-method';
import { getUserList } from '#/api/system/user';
import { PermissionCodes } from '#/constants/permission-codes';
import { productCategoryNameInitial } from '#/utils/product-category-label';
import { $t } from '#/locales';
import FilePreviewModal from '#/components/file-preview/FilePreviewModal.vue';
import CustomerSelectModal from '#/views/task/projects/modules/customer-select-modal.vue';

const route = useRoute();
const router = useRouter();

const id = computed(() => route.params.id as string);
const isNew = computed(() => !id.value);
const workflowId = computed(() => route.query.workflowId as string);

// 判断是否需要审批（当有workflowId时）
const needApproval = computed(() => !!workflowId.value);
/** 当前订单状态（编辑时从详情加载，用于控制是否显示「提交审批」） */
const orderStatus = ref<number | null>(null);

const loading = ref(false);
const submitting = ref(false);
const userOptions = ref<{ label: string; value: string }[]>([]);
const customerSelectModalRef = ref<InstanceType<typeof CustomerSelectModal> | null>(null);
const selectedCustomerName = ref('');
const contractOptions = ref<{ label: string; value: string }[]>([]);
/** 合同签订公司选项：来自合同类型中「订单签订公司选项展示」为是的项 */
const contractSigningCompanyOptions = ref<{ label: string; value: string }[]>([]);
const projectOptions = ref<{ label: string; value: string }[]>([]);
const productOptions = ref<{ label: string; value: string; code: string; name: string; model: string; unit: string; categoryId: string; categoryName: string; productTypeId: string; imagePath: string }[]>([]);
const invoiceTypeOptions = ref<{ label: string; value: number }[]>([]);
const logisticsCompanyOptions = ref<{ label: string; value: string }[]>([]);
const logisticsMethodOptions = ref<{ label: string; value: string }[]>([]);
/** 部门树（原始接口数据，用于按 id 查名称） */
const deptTreeRaw = ref<SystemDeptApi.SystemDept[]>([]);
/** 部门树选项（TreeSelect tree-data 格式） */
const deptTreeData = ref<{ title: string; value: string; key: string; children?: any[] }[]>([]);
type DataScope = 0 | 1 | 2 | 3 | 4;
const scopeContext = ref<{
  scope: DataScope;
  deptId: string;
  authorizedDeptIds: string[];
}>({
  scope: 0,
  deptId: '',
  authorizedDeptIds: [],
});

const paymentStatusOptions = [
  { label: $t('order.paymentStatusFullPayment'), value: 0 },
  { label: $t('order.paymentStatusPartialPayment'), value: 1 },
  { label: $t('order.paymentStatusInstallmentUrgent'), value: 2 },
  { label: $t('order.paymentStatusPendingConfirmation'), value: 3 },
];

const logisticsPaymentMethodOptions = [
  { label: $t('order.logisticsPaymentMethodNotYetShipped'), value: 0 },
  { label: $t('order.logisticsPaymentMethodShipped'), value: 1 },
];

const form = ref({
  customerId: '',
  customerName: '',
  projectId: '' as string,
  contractId: '' as string,
  orderNumber: '',
  type: OrderTypeEnum.Sales as number,
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
  invoiceTypeId: 0 as number,
  installationFee: 0,
  estimatedFreight: 0,
  contractFiles: [] as OrderApi.OrderContractFileItem[],
  stockFiles: [] as OrderApi.OrderContractFileItem[],
  selectedContractFileId: 0 as number,
  isShipped: false,
  // 默认到款情况：待确认
  paymentStatus: 3 as number,
  contractNotCompanyTemplate: false,
  contractAmount: 0,
  receiverName: '',
  receiverPhone: '',
  receiverAddress: '',
  payDate: '' as string,
  deliveryDate: '' as string,
  orderLogisticsCompanyId: '' as string,
  orderLogisticsMethodId: '' as string,
  logisticsPaymentMethodId: 0 as number,
  waybillNumber: '',
  shippingFee: 0,
  surcharge: 0,
  shippingFeeIsPay: false,
  items: [] as {
    productId: string;
    productCategoryId: string;
    productCategoryName: string;
    productTypeId: string;
    imagePath: string;
    installNotes: string;
    trainingDuration: string;
    packingStatus: number;
    reviewStatus: number;
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

/** 订单备注列表（仅 TypeId=0：分期加急发货提示） */
const orderRemarks = ref<OrderApi.OrderRemarkDto[]>([]);
const remarkModalOpen = ref(false);
const remarkContent = ref('');
const addingOrderRemark = ref(false);
const remarkEditModalOpen = ref(false);
const remarkEditContent = ref('');
const editingOrderRemarkId = ref('');
const updatingOrderRemark = ref(false);
const INSTALLMENT_URGENT_PAYMENT_STATUS = 2;

const orderRemarkColumns = [
  {
    title: () => $t('order.addedBy'),
    dataIndex: 'userName',
    key: 'userName',
    width: 120,
  },
  {
    title: () => $t('order.addedAt'),
    dataIndex: 'addedAt',
    key: 'addedAt',
    width: 180,
  },
  {
    title: () => $t('order.remarkContent'),
    dataIndex: 'content',
    key: 'content',
  },
  {
    title: () => $t('order.operation'),
    dataIndex: 'action',
    key: 'action',
    width: 180,
  },
];

/** 无有效产品分类时的空 GUID（与后端一致） */
const EMPTY_PRODUCT_CATEGORY_ID = '00000000-0000-0000-0000-000000000000';

/** 明细行中去重后的有效产品分类（用于按分类合同优惠） */
const distinctCategoriesFromItems = computed(() => {
  const map = new Map<string, string>();
  for (const it of form.value.items) {
    const cid = String(it.productCategoryId || '').trim();
    if (!cid || cid === EMPTY_PRODUCT_CATEGORY_ID) continue;
    const name = (it.productCategoryName || '').trim() || cid;
    if (!map.has(cid)) map.set(cid, name);
  }
  return [...map.entries()].map(([productCategoryId, categoryName]) => ({ productCategoryId, categoryName }));
});

/** 按分类的合同优惠编辑行（与 order_band 对应） */
const orderCategoriesForm = ref<
  { productCategoryId: string; categoryName: string; discountPoints: number; remark: string }[]
>([]);

/** 加载详情时避免 watch 用空数据覆盖接口返回的 orderCategories */
const suppressOrderCategoriesRebuild = ref(false);

function rebuildOrderCategoriesMerge(apiRows?: OrderApi.OrderCategoryDto[]) {
  const cats = distinctCategoriesFromItems.value;
  const apiById = new Map((apiRows ?? []).map((b) => [String(b.productCategoryId), b]));
  const prevById = new Map(orderCategoriesForm.value.map((x) => [x.productCategoryId, x]));
  orderCategoriesForm.value = cats.map((c) => {
    const api = apiById.get(c.productCategoryId);
    const prev = prevById.get(c.productCategoryId);
    return {
      productCategoryId: c.productCategoryId,
      categoryName: c.categoryName,
      discountPoints: api != null ? Number(api.discountPoints) : (prev?.discountPoints ?? 0),
      remark: api != null ? (api.remark ?? '') : (prev?.remark ?? ''),
    };
  });
}

watch(
  () =>
    form.value.items
      .map((i) => `${i.productId}|${i.productCategoryId}|${i.productCategoryName}`)
      .join(';'),
  () => {
    if (suppressOrderCategoriesRebuild.value) return;
    rebuildOrderCategoriesMerge();
  },
);

const itemColumns = [
  { title: () => $t('order.productCategory'), dataIndex: 'productCategoryName', width: 120, key: 'productCategoryName' },
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
    productCategoryId: '',
    productCategoryName: '',
    productTypeId: '',
    imagePath: '',
    installNotes: '',
    trainingDuration: '',
    packingStatus: 0,
    reviewStatus: 0,
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

/** 根据下拉选项（productOptions）回填明细行；用 productId 查找，避免 Select 的 @select 第二参数不含自定义字段 */
function syncOrderItemFromProduct(productId: string | undefined | null, index: number) {
  const item = form.value.items[index];
  if (!item) return;
  if (productId == null || productId === '') {
    item.productName = '';
    item.model = '';
    item.unit = '';
    item.number = '';
    item.productCategoryId = '';
    item.productCategoryName = '';
    item.productTypeId = '';
    item.imagePath = '';
    return;
  }
  const opt = productOptions.value.find((p) => String(p.value) === String(productId));
  if (!opt) return;
  item.productName = opt.name;
  item.model = opt.model ?? '';
  item.unit = opt.unit ?? '';
  item.number = opt.code ?? '';
  item.productCategoryId = opt.categoryId ?? '';
  item.productCategoryName = opt.categoryName ?? '';
  item.productTypeId = opt.productTypeId ?? '';
  item.imagePath = opt.imagePath ?? '';
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

/** 生成订单编号，格式：YYYYMMDDHHmmss，与项目编号一致 */
function generateOrderNumber(): string {
  const now = new Date();
  const y = now.getFullYear();
  const m = String(now.getMonth() + 1).padStart(2, '0');
  const d = String(now.getDate()).padStart(2, '0');
  const h = String(now.getHours()).padStart(2, '0');
  const min = String(now.getMinutes()).padStart(2, '0');
  const s = String(now.getSeconds()).padStart(2, '0');
  return `${y}${m}${d}${h}${min}${s}`;
}

function openCustomerSelectModal() {
  if (!form.value.ownerId) {
    message.warning($t('ui.formRules.required', [$t('order.owner')]));
    return;
  }
  customerSelectModalRef.value?.open({
    ownerId: form.value.ownerId,
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

function parseJwtClaims(accessToken?: string | null) {
  if (!accessToken) return null;
  const tokenParts = accessToken.split('.');
  if (tokenParts.length < 2) return null;
  try {
    const payloadPart = tokenParts[1] ?? '';
    const base64 = payloadPart.replace(/-/g, '+').replace(/_/g, '/');
    const padded = base64.padEnd(Math.ceil(base64.length / 4) * 4, '=');
    const decoded = decodeURIComponent(
      atob(padded)
        .split('')
        .map((c) => `%${`00${c.charCodeAt(0).toString(16)}`.slice(-2)}`)
        .join(''),
    );
    return JSON.parse(decoded) as Record<string, unknown>;
  } catch {
    return null;
  }
}

function initScopeContextFromToken() {
  const payload = parseJwtClaims(accessStore.accessToken);
  const scopeValue = Number(payload?.data_scope ?? 0);
  const scope = (Number.isNaN(scopeValue) ? 0 : scopeValue) as DataScope;
  const deptId = String(payload?.dept_id ?? '').trim();
  const authorizedRaw = String(payload?.authorized_dept_ids ?? '').trim();
  const authorizedDeptIds = authorizedRaw
    ? authorizedRaw.split(',').map((x) => x.trim()).filter(Boolean)
    : [];
  scopeContext.value = { scope, deptId, authorizedDeptIds };
}

function flattenDeptIds(nodes: SystemDeptApi.SystemDept[]): string[] {
  const ids: string[] = [];
  for (const node of nodes) {
    ids.push(String(node.id));
    if (node.children?.length) {
      ids.push(...flattenDeptIds(node.children));
    }
  }
  return ids;
}

function filterDeptTreeByAllowed(
  nodes: SystemDeptApi.SystemDept[],
  allowed: Set<string>,
): SystemDeptApi.SystemDept[] {
  const result: SystemDeptApi.SystemDept[] = [];
  for (const node of nodes) {
    const children = node.children?.length
      ? filterDeptTreeByAllowed(node.children, allowed)
      : [];
    const selfIncluded = allowed.has(String(node.id));
    if (selfIncluded || children.length > 0) {
      result.push({
        ...node,
        children,
      });
    }
  }
  return result;
}

function getAllowedDeptIdSet(): Set<string> | null {
  if (scopeContext.value.scope === 0) return null;
  const ids = scopeContext.value.authorizedDeptIds.length > 0
    ? scopeContext.value.authorizedDeptIds
    : (scopeContext.value.deptId ? [scopeContext.value.deptId] : []);
  return new Set(ids.map(String));
}

async function loadUserOptionsByDept(deptId?: string) {
  const res = await getUserList({
    pageIndex: 1,
    pageSize: 500,
    deptId: deptId || undefined,
  });
  userOptions.value = (res.items ?? []).map((x) => ({
    label: x.realName || x.name || x.userId,
    value: String(x.userId),
  }));
}

function syncOwnerNameById(ownerId: string) {
  const target = userOptions.value.find((x) => x.value === ownerId);
  form.value.ownerName = target?.label ?? '';
}

function clearSelectedCustomer() {
  form.value.customerId = '';
  form.value.customerName = '';
  selectedCustomerName.value = '';
}

async function onDeptChange(val: string) {
  form.value.deptName = val ? findDeptName(deptTreeRaw.value, val) : '';
  await loadUserOptionsByDept(val || undefined);
  if (form.value.ownerId && !userOptions.value.some((x) => x.value === form.value.ownerId)) {
    form.value.ownerId = '';
    form.value.ownerName = '';
    clearSelectedCustomer();
    message.warning('负责人不在当前部门范围内，请重新选择');
  }
}

function onOwnerSelect(_: unknown, opt: unknown) {
  form.value.ownerName = (opt as { label?: string })?.label ?? '';
  clearSelectedCustomer();
}

async function applyCreateDefaults() {
  if (isNew.value) {
    form.value.orderNumber = generateOrderNumber();
  }
  if (isMarketingPersonalCreateMode.value) {
    const deptId = currentUserDeptId.value || scopeContext.value.deptId;
    form.value.deptId = deptId;
    form.value.deptName = currentUserDeptName.value || findDeptName(deptTreeRaw.value, deptId);
    await loadUserOptionsByDept(deptId || undefined);
    if (currentUserId.value) {
      if (!userOptions.value.some((x) => x.value === currentUserId.value)) {
        userOptions.value.unshift({
          label: currentUserName.value || currentUserId.value,
          value: currentUserId.value,
        });
      }
      form.value.ownerId = currentUserId.value;
      form.value.ownerName = currentUserName.value || currentUserId.value;
    }
    return;
  }
  await loadUserOptionsByDept(form.value.deptId || undefined);
}

async function loadDetail() {
  if (!id.value) return;
  loading.value = true;
  try {
    const data = await getOrder(id.value);
    const detail = data as OrderApi.OrderDetail;
    const nameByCat = new Map(
      (detail.orderCategories ?? []).map((b) => [String(b.productCategoryId), b.categoryName ?? '']),
    );
    suppressOrderCategoriesRebuild.value = true;
    form.value = {
      customerId: data.customerId,
      customerName: data.customerName,
      projectId: data.projectId ?? '',
      contractId: data.contractId ?? '',
      orderNumber: data.orderNumber,
      type: data.type,
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
      invoiceTypeId: normalizeOptionId((data as any).invoiceTypeId),
      installationFee: Number(data.installationFee ?? 0),
      estimatedFreight: Number(data.estimatedFreight ?? 0),
      contractFiles: (data.contractFiles ?? []).map((f: OrderApi.OrderContractFileItem) => ({
        path: f.path,
        fileName: f.fileName,
        size: f.size,
        format: f.format,
        updatedAt: f.updatedAt,
      })),
      stockFiles: (data.stockFiles ?? []).map((f: OrderApi.OrderContractFileItem) => ({
        path: f.path,
        fileName: f.fileName,
        size: f.size,
        format: f.format,
        updatedAt: f.updatedAt,
      })),
      selectedContractFileId: Number((data as any).selectedContractFileId ?? 0),
      isShipped: data.isShipped ?? false,
      paymentStatus: Number((data as any).paymentStatus ?? 3),
      contractNotCompanyTemplate: data.contractNotCompanyTemplate ?? false,
      contractAmount: Number(data.contractAmount ?? 0),
      receiverName: data.receiverName ?? '',
      receiverPhone: data.receiverPhone ?? '',
      receiverAddress: data.receiverAddress ?? '',
      payDate: data.payDate ? data.payDate.slice(0, 10) : '',
      deliveryDate: data.deliveryDate ? data.deliveryDate.slice(0, 10) : '',
      orderLogisticsCompanyId: isEmptyGuid((data as any).orderLogisticsCompanyId) ? '' : (data as any).orderLogisticsCompanyId,
      orderLogisticsMethodId: isEmptyGuid((data as any).orderLogisticsMethodId) ? '' : (data as any).orderLogisticsMethodId,
      logisticsPaymentMethodId: Number((data as any).logisticsPaymentMethodId ?? 0),
      waybillNumber: (data as any).waybillNumber ?? '',
      shippingFee: Number((data as any).shippingFee ?? 0),
      surcharge: Number((data as any).surcharge ?? 0),
      shippingFeeIsPay: Boolean((data as any).shippingFeeIsPay ?? false),
      items: (data.items ?? []).map((i) => {
        const product = productOptions.value.find((p) => p.value === i.productId);
        const cid = String((i as any).productCategoryId || '');
        return {
          ...(i as any),
          productCategoryName: product?.categoryName ?? nameByCat.get(cid) ?? '',
        };
      }),
    };
    await nextTick();
    rebuildOrderCategoriesMerge(detail.orderCategories);
    suppressOrderCategoriesRebuild.value = false;
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
    orderStatus.value = data.status ?? null;
  } finally {
    suppressOrderCategoriesRebuild.value = false;
      if (!isNew.value) {
        // 非新建模式下加载备注（仅 TypeId=0）
        await loadOrderRemarks();
      }
    loading.value = false;
  }
}

async function loadOrderRemarks() {
  if (!id.value) {
    orderRemarks.value = [];
    return;
  }

  try {
    const res = await getOrderRemarks(id.value);
    orderRemarks.value = res ?? [];
  } catch (e) {
    // 列表用于展示，失败时不影响订单表单本身
    orderRemarks.value = [];
    message.error(String(e));
  }
}

function openAddOrderRemarkModal() {
  remarkContent.value = '';
  remarkModalOpen.value = true;
}

async function confirmAddOrderRemark() {
  const content = (remarkContent.value ?? '').trim();
  if (!content) {
    message.warning($t('ui.formRules.required', [$t('order.remarkContent')]));
    return;
  }
  if (!id.value) return;

  try {
    addingOrderRemark.value = true;
    await createOrderRemark(id.value, content);
    message.success($t('common.success'));
    remarkModalOpen.value = false;
    await loadOrderRemarks();
  } catch (e) {
    message.error(String(e));
  } finally {
    addingOrderRemark.value = false;
  }
}

function openEditOrderRemarkModal(record: OrderApi.OrderRemarkDto) {
  editingOrderRemarkId.value = record.id;
  remarkEditContent.value = record.content ?? '';
  remarkEditModalOpen.value = true;
}

async function confirmEditOrderRemark() {
  const content = (remarkEditContent.value ?? '').trim();
  if (!content) {
    message.warning($t('ui.formRules.required', [$t('order.remarkContent')]));
    return;
  }
  if (!id.value || !editingOrderRemarkId.value) return;

  try {
    updatingOrderRemark.value = true;
    await updateOrderRemark(id.value, editingOrderRemarkId.value, content);
    message.success($t('common.success'));
    remarkEditModalOpen.value = false;
    await loadOrderRemarks();
  } catch (e) {
    message.error(String(e));
  } finally {
    updatingOrderRemark.value = false;
  }
}

function confirmDeleteOrderRemark(record: OrderApi.OrderRemarkDto) {
  if (!id.value) return;

  Modal.confirm({
    title: $t('order.deleteOrderRemarkConfirmTitle'),
    content: $t('order.deleteOrderRemarkConfirmContent'),
    async onOk() {
      try {
        await deleteOrderRemark(id.value, record.id);
        message.success($t('common.success'));
        await loadOrderRemarks();
      } catch (e) {
        message.error(String(e));
      }
    },
  });
}

const statusText = computed(() => {
  if (isNew.value) {
    return $t('order.statusDraft');
  }
  switch (orderStatus.value) {
    case OrderStatusEnum.Draft:
      return $t('order.statusDraft');
    case OrderStatusEnum.PendingAudit:
      return $t('order.statusPendingAudit');
    case OrderStatusEnum.Ordered:
      return $t('order.statusOrdered');
    case OrderStatusEnum.Completed:
      return $t('order.statusCompleted');
    case OrderStatusEnum.Rejected:
      return $t('order.statusRejected');
    case OrderStatusEnum.Unpaid:
      return $t('order.statusUnpaid');
    default:
      return '-';
  }
});

const statusTagColor = computed(() => {
  switch (orderStatus.value) {
    case OrderStatusEnum.PendingAudit:
      return 'processing';
    case OrderStatusEnum.Completed:
      return 'success';
    case OrderStatusEnum.Rejected:
      return 'error';
    case OrderStatusEnum.Unpaid:
      return 'warning';
    default:
      return 'default';
  }
});

const accessStore = useAccessStore();
const userStore = useUserStore();
const currentUserId = computed(() => String(userStore.userInfo?.userId ?? ''));
const currentUserName = computed(() => userStore.userInfo?.realName || userStore.userInfo?.name || '');
const currentUserDeptId = computed(() => String(userStore.userInfo?.deptId ?? ''));
const currentUserDeptName = computed(() => userStore.userInfo?.deptName || '');
const canSubmitOrder = computed(
  () => accessStore.accessCodes?.includes(PermissionCodes.OrderSubmit) ?? false,
);

const hasPermission = (code: string) =>
  accessStore.accessCodes?.includes(code) ?? false;

// 获取当前用户角色和部门信息
const userRole = computed(() => {
  // 从userStore的userRoles获取角色
  if (userStore.userRoles && userStore.userRoles.length > 0) {
    return userStore.userRoles[0];
  }
  return userStore.userInfo?.roles?.[0] || '';
});
const userDept = computed(() => userStore.userInfo?.deptName || '');

// 权限控制：根据角色/部门控制可编辑模块
const canEditMarketing = computed(() => {
  // 营销中心模块：营销部门或管理员可编辑
  return userDept.value?.includes('营销') || userRole.value === '管理员';
});

const canEditFinance = computed(() => {
  // 财务部模块：财务部或管理员可编辑
  return userDept.value?.includes('财务') || userRole.value === '管理员';
});

const canEditLogistics = computed(() => {
  // 物流与收货模块：物流/事务部门或管理员可编辑
  // 说明：审核流程到“事务部”节点时，该部门需要能够编辑物流与收货模块。
  return userDept.value?.includes('物流') || userDept.value?.includes('事务') || userRole.value === '管理员';
});

const canEditProducts = computed(() => {
  // 产品明细：营销中心或管理员可编辑
  return userDept.value?.includes('营销') || userRole.value === '管理员';
});
const isPersonalScope = computed(() => scopeContext.value.scope === 3);
const isMarketingPersonalCreateMode = computed(
  () => isNew.value && isPersonalScope.value && userDept.value?.includes('营销') && userRole.value !== '管理员',
);
/** 是否显示「推送」按钮：有权限且（需要审批 或 新建模式 或 编辑且状态为草稿/已驳回） */
const showSubmitApprovalButton = computed(
  () =>
    canSubmitOrder.value &&
    (needApproval.value ||
      isNew.value ||
      (!!id.value &&
        (orderStatus.value === OrderStatusEnum.Draft ||
          orderStatus.value === OrderStatusEnum.Rejected ||
          orderStatus.value === OrderStatusEnum.PendingAudit))),
);

onMounted(async () => {
  loading.value = true;
  try {
    initScopeContextFromToken();

    const [
      contractRes,
      contractTypeRaw,
      projectRes,
      logisticsCompanyList,
      logisticsMethodList,
      invoiceTypeList,
      productRes,
      deptRes,
    ] = await Promise.all([
      getContractList({ pageIndex: 1, pageSize: 500 }),
      getContractTypeOptionList(),
      getProjectList({ pageIndex: 1, pageSize: 500 }),
      getOrderLogisticsCompanyList(),
      getOrderLogisticsMethodList(),
      getOrderInvoiceTypeOptionList(),
      getProductList({ pageIndex: 1, pageSize: 1000 }),
      getDeptTree(),
    ]);

    // Process results
    contractOptions.value = (contractRes.items ?? []).map((x) => ({
      label: `${x.code} - ${x.title}`,
      value: x.id,
    }));

    const normalized = Array.isArray(contractTypeRaw)
      ? (Array.isArray(contractTypeRaw[0]) ? (contractTypeRaw as unknown[]).flat() : contractTypeRaw)
      : [];
    const list = normalized.filter(
      (item): item is ContractTypeOptionApi.ContractTypeOptionItem =>
        typeof item === 'object' && item !== null && 'name' in item,
    );
    contractSigningCompanyOptions.value = list
      .filter((x) => x.orderSigningCompanyOptionDisplay === true)
      .map((x) => ({ label: x.name, value: x.name }));

    projectOptions.value = (projectRes.items ?? []).map((x) => ({
      label: x.name || x.id,
      value: x.id,
    }));

    logisticsCompanyOptions.value = (logisticsCompanyList ?? []).map((x) => ({
      label: x.name,
      value: x.id,
    }));

    logisticsMethodOptions.value = (logisticsMethodList ?? []).map((x) => ({
      label: x.name,
      value: x.id,
    }));

    invoiceTypeOptions.value = (invoiceTypeList ?? []).map((x: OrderInvoiceTypeOptionApi.OrderInvoiceTypeOptionItem) => ({
      label: x.name,
      value: Number(x.id),
    }));

    productOptions.value = (productRes.items ?? []).map((x) => ({
      label: `${x.code} - ${x.name}`,
      value: x.id,
      code: x.code,
      name: x.name,
      model: x.model,
      unit: x.unit,
      categoryId: x.categoryId != null && x.categoryId !== '' ? String(x.categoryId) : '',
      categoryName: x.categoryName ?? '',
      productTypeId: x.productTypeId ?? '',
      imagePath: x.imagePath ?? '',
    }));

    const deptList = (Array.isArray(deptRes) ? deptRes : (deptRes as any)?.data ?? []) as SystemDeptApi.SystemDept[];
    const allowedDeptIdSet = getAllowedDeptIdSet();
    deptTreeRaw.value = allowedDeptIdSet
      ? filterDeptTreeByAllowed(deptList, allowedDeptIdSet)
      : deptList;
    deptTreeData.value = buildDeptTreeData(deptTreeRaw.value);

    if (!isNew.value) {
      await loadDetail(); // Now this is safe
    }

    if (allowedDeptIdSet) {
      const allowedIds = new Set(flattenDeptIds(deptTreeRaw.value));
      if (form.value.deptId && !allowedIds.has(String(form.value.deptId))) {
        form.value.deptId = '';
        form.value.deptName = '';
      }
    }

    await applyCreateDefaults();
  } finally {
    loading.value = false;
  }
});

watch(
  () => form.value.ownerId,
  () => {
    if (!form.value.ownerId) {
      form.value.ownerName = '';
      clearSelectedCustomer();
    }
  },
);

function toIsoDate(v: string): string {
  if (!v) return new Date().toISOString();
  const d = new Date(v);
  return isNaN(d.getTime()) ? new Date().toISOString() : d.toISOString();
}

function normalizeOptionId(value: unknown): number {
  if (value == null) return 0;
  if (typeof value === 'number') return value;
  if (typeof value === 'string') {
    const v = Number(value);
    return Number.isNaN(v) ? 0 : v;
  }
  if (typeof value === 'object') {
    const v = Number((value as { value?: unknown; id?: unknown })?.value ?? (value as { id?: unknown }).id);
    return Number.isNaN(v) ? 0 : v;
  }
  return 0;
}

function isEmptyGuid(value: unknown): boolean {
  if (typeof value !== 'string') return true;
  const emptyGuids = ['00000000-0000-0000-0000-000000000000', ''];
  return emptyGuids.includes(value.toLowerCase());
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
  if (!form.value.deptId) {
    message.warning($t('ui.formRules.required', [$t('order.dept')]));
    return;
  }
  if (form.value.needInvoice && !form.value.invoiceTypeId) {
    message.warning($t('ui.formRules.required', [$t('order.invoiceType')]));
    return;
  }
  if (!form.value.stockFiles.length) {
    message.warning($t('ui.formRules.required', [$t('order.stockUpload')]));
    return;
  }
  if (!form.value.items.length || form.value.items.some((i) => !i.productId)) {
    message.warning($t('ui.formRules.required', [$t('order.items')]));
    return;
  }
  submitting.value = true;
  try {
    const payload = buildOrderPayload();
    if (isNew.value) {
      await createOrder(payload);
      message.success($t('common.success'));
      router.push('/order/list');
    } else {
      await updateOrder(id.value!, payload);
      message.success($t('common.success'));
      router.push('/order/list');
    }
  } catch (e) {
    message.error(String(e));
  } finally {
    submitting.value = false;
  }
}

function buildOrderPayload() {
  return {
    customerId: form.value.customerId,
    customerName: form.value.customerName,
    projectId: form.value.projectId,
    contractId: form.value.contractId,
    orderNumber: form.value.orderNumber,
    type: form.value.type,
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
    invoiceTypeId: Number(form.value.invoiceTypeId ?? 0),
    installationFee: form.value.installationFee,
    estimatedFreight: form.value.estimatedFreight,
    contractFiles: form.value.contractFiles,
    stockFiles: form.value.stockFiles,
    selectedContractFileId: Number(form.value.selectedContractFileId ?? 0),
    isShipped: form.value.isShipped,
    paymentStatus: Number(form.value.paymentStatus ?? 0),
    contractNotCompanyTemplate: form.value.contractNotCompanyTemplate,
    orderCategories:
      orderCategoriesForm.value.length > 0
        ? orderCategoriesForm.value.map((r) => ({
            productCategoryId: r.productCategoryId,
            categoryName: r.categoryName ?? '',
            discountPoints: r.discountPoints,
            remark: r.remark ?? '',
          }))
        : [],
    contractAmount: form.value.contractAmount,
    receiverName: form.value.receiverName ?? '',
    receiverPhone: form.value.receiverPhone ?? '',
    receiverAddress: form.value.receiverAddress ?? '',
    payDate: toIsoDate(form.value.payDate),
    deliveryDate: toIsoDate(form.value.deliveryDate),
    orderLogisticsCompanyId: form.value.orderLogisticsCompanyId,
    orderLogisticsMethodId: form.value.orderLogisticsMethodId,
    logisticsPaymentMethodId: Number(form.value.logisticsPaymentMethodId ?? 0),
    waybillNumber: form.value.waybillNumber ?? '',
    shippingFee: form.value.shippingFee,
    shippingFeeIsPay: form.value.shippingFeeIsPay,
    surcharge: form.value.surcharge,
    items: form.value.items.map((i) => ({
      productId: i.productId,
      productCategoryId: i.productCategoryId,
      productTypeId: i.productTypeId,
      imagePath: i.imagePath,
      installNotes: i.installNotes,
      trainingDuration: i.trainingDuration,
      packingStatus: i.packingStatus,
      reviewStatus: i.reviewStatus,
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
}

function validateOrderForm(): boolean {
  if (!form.value.customerId || !form.value.orderNumber || !form.value.ownerId) {
    message.warning($t('ui.formRules.required', [$t('order.customer')]));
    return false;
  }
  if (!form.value.projectId) {
    message.warning($t('ui.formRules.required', [$t('order.project')]));
    return false;
  }
  if (!form.value.deptId) {
    message.warning($t('ui.formRules.required', [$t('order.dept')]));
    return false;
  }
  if (!form.value.stockFiles.length) {
    message.warning($t('ui.formRules.required', [$t('order.stockUpload')]));
    return false;
  }
  if (!form.value.items.length || form.value.items.some((i) => !i.productId)) {
    message.warning($t('ui.formRules.required', [$t('order.items')]));
    return false;
  }
  return true;
}

async function onSubmitForApproval() {
  if (!validateOrderForm()) return;

  Modal.confirm({
    title: $t('order.submitApprovalConfirmTitle'),
    content: $t('order.submitApprovalConfirmContent'),
    async onOk() {
      submitting.value = true;
      try {
        if (isNew.value) {
          const payload = buildOrderPayload();
          const res = await createOrder(payload);
          const orderId = typeof res === 'object' && res?.id ? res.id : String(res);
          await submitOrder(orderId, '');
          message.success($t('order.submitApprovalSuccess'));
          router.push('/order/list');
        } else {
          // 如果是从工作流详情页面进入的且订单状态为审核中，先保存修改的内容，然后返回工作流详情页面
          if (workflowId.value && orderStatus.value === OrderStatusEnum.PendingAudit) {
            const payload = buildOrderPayload();
            await updateOrder(id.value!, payload);
            message.success('修改已保存');
            router.push(`/workflow/instance/${workflowId.value}`);
          } else {
            // 先保存修改的内容
            const payload = buildOrderPayload();
            await updateOrder(id.value!, payload);
            // 然后提交审批
            await submitOrder(id.value!, '');
            message.success($t('order.submitApprovalSuccess'));
            // 如果是从工作流详情页面进入的，提交后返回工作流详情页面
            if (workflowId.value) {
              router.push(`/workflow/instance/${workflowId.value}`);
            } else {
              await loadDetail();
            }
          }
        }
      } catch (e) {
        message.error(String(e));
      } finally {
        submitting.value = false;
      }
    },
  });
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
const stockFileInputRef = ref<HTMLInputElement | null>(null);
const stockFileSearchKeyword = ref('');
const previewModalVisible = ref(false);
const previewFilePath = ref('');
const previewFileName = ref('');

function triggerContractUpload() {
  contractFileInputRef.value?.click();
}

function triggerStockUpload() {
  stockFileInputRef.value?.click();
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

function refreshStockList() {
  stockFileSearchKeyword.value = '';
}

const filteredContractFiles = computed(() => {
  const kw = contractFileSearchKeyword.value.trim().toLowerCase();
  if (!kw) return form.value.contractFiles;
  return form.value.contractFiles.filter(
    (f) => f.fileName.toLowerCase().includes(kw) || (f.format && f.format.toLowerCase().includes(kw)),
  );
});

const filteredStockFiles = computed(() => {
  const kw = stockFileSearchKeyword.value.trim().toLowerCase();
  if (!kw) return form.value.stockFiles;
  return form.value.stockFiles.filter(
    (f) => f.fileName.toLowerCase().includes(kw) || (f.format && f.format.toLowerCase().includes(kw)),
  );
});

function previewContractFile(item: OrderApi.OrderContractFileItem) {
  previewFilePath.value = item.path;
  previewFileName.value = item.fileName;
  previewModalVisible.value = true;
}

function previewStockFile(item: OrderApi.OrderContractFileItem) {
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

async function downloadStockFile(item: OrderApi.OrderContractFileItem) {
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

async function printStockFile(item: OrderApi.OrderContractFileItem) {
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

function removeStockFile(item: OrderApi.OrderContractFileItem) {
  form.value.stockFiles = form.value.stockFiles.filter((f) => f.path !== item.path);
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

const stockFileColumns = contractFileColumns;

async function onStockFileSelected(e: Event) {
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
    form.value.stockFiles.push({
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
</script>

<template>
  <Page auto-content-height :loading="loading">
    <div class="p-4">
      <!-- 顶部操作栏（仅保留辅助入口） -->
      <div class="mb-4 flex flex-wrap items-center gap-2">
        <div class="flex items-center gap-2">
          <Button v-if="!isNew" size="small" @click="goContractList">{{ $t('contract.list') }}</Button>
        </div>
        <template v-if="!isNew && id">
          <Button type="link" class="p-0" @click="goToDetail">
            {{ $t('order.changeRecord') }}
          </Button>
        </template>
      </div>

      <!-- 表单区块：标题 + 说明，与下方表单卡片明确区分 -->
      <div class="border-border border-b pb-5">
        <h2 class="mb-1.5 text-lg font-semibold text-foreground">
          {{ $t('order.formSectionTitle') }}
        </h2>
        <p class="text-sm text-muted-foreground">
          {{ $t('order.formSectionDesc') }}
        </p>
      </div>

      <!-- 订单基础信息卡片：营销中心 + 财务部 + 物流与收货 -->
      <Card :bordered="true" class="border-border bg-card mt-5">
        <template #title>
          <div class="flex items-center gap-2">
            <span class="text-base font-medium">{{ $t('order.formCardTitle') }}</span>
            <Tag :color="statusTagColor">{{ statusText }}</Tag>
          </div>
        </template>

      <!-- 营销中心 -->
      <div class="border-border pt-2 first:pt-0">
        <h4 class="order-section-title">{{ $t('order.sectionMarketing') }}</h4>
        <div class="grid gap-4 md:grid-cols-4">
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground"><span class="mr-[2px] text-destructive">*</span>{{ $t('order.type') }}</label>
            <Select
              v-model:value="form.type"
              class="w-full"
              :disabled="!canEditMarketing"
              :options="[
                { label: $t('order.typeSales'), value: 0 },
                { label: $t('order.typeAfterSales'), value: 1 },
                { label: $t('order.typeSample'), value: 2 },
                { label: $t('order.typeGeneralTest'), value: 3 },
              ]"
            />
          </div>
          <!-- 订单状态由工作流控制，显示在卡片标题 -->
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground"><span class="mr-[2px] text-destructive">*</span>{{ $t('order.orderNumber') }}</label>
            <Input v-model:value="form.orderNumber" placeholder="" :disabled="!canEditMarketing" />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.projectContactName') }}</label>
            <Input v-model:value="form.projectContactName" placeholder="" :disabled="!canEditMarketing" />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.projectContactPhone') }}</label>
            <Input v-model:value="form.projectContactPhone" placeholder="" :disabled="!canEditMarketing" />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.warranty') }}</label>
            <Select
              v-model:value="form.warranty"
              allow-clear
              class="w-full"
              :disabled="!canEditMarketing"
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
              :disabled="!canEditMarketing || isMarketingPersonalCreateMode"
              :tree-data="deptTreeData"
              tree-node-filter-prop="title"
              placeholder="--请选择--"
              :filter-tree-node="(inputValue, treeNode) => treeNode?.title?.toString().toLowerCase().includes(inputValue?.toLowerCase())"
              @change="onDeptChange"
            />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground"><span class="mr-[2px] text-destructive">*</span>{{ $t('order.owner') }}</label>
            <Select
              v-model:value="form.ownerId"
              allow-clear
              class="w-full"
              :disabled="!canEditMarketing || isMarketingPersonalCreateMode"
              :options="userOptions"
              :field-names="{ label: 'label', value: 'value' }"
              @select="onOwnerSelect"
            />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground"><span class="mr-[2px] text-destructive">*</span>{{ $t('order.customer') }}</label>
            <Input
              :value="selectedCustomerName"
              :readonly="!canEditMarketing"
              class="w-full cursor-pointer"
              :placeholder="$t('task.project.searchCustomer')"
              @click="canEditMarketing && openCustomerSelectModal"
            >
              <template #suffix>
                <span
                  v-if="canEditMarketing"
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
              :disabled="!canEditMarketing"
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
              :disabled="!canEditMarketing"
              :options="contractSigningCompanyOptions"
              :field-names="{ label: 'label', value: 'value' }"
              placeholder="--请选择--"
            />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.contractTrustee') }}</label>
            <Input v-model:value="form.contractTrustee" placeholder="" :disabled="!canEditMarketing" />
          </div>
          <div v-if="hasPermission(PermissionCodes.OrderNeedInvoice)">
            <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.needInvoice') }}</label>
            <Select
              :value="form.needInvoice ? 1 : 0"
              class="w-full"
              :disabled="!canEditMarketing"
              :options="[
                { label: $t('order.yes'), value: 1 },
                { label: $t('order.no'), value: 0 },
              ]"
              @update:value="(v) => { form.needInvoice = v === 1; if (!form.needInvoice) form.invoiceTypeId = 0; }"
            />
          </div>
          <div v-if="form.needInvoice && hasPermission(PermissionCodes.OrderNeedInvoice)">
            <label class="mb-1 block text-sm font-medium text-foreground">
              <span class="mr-[2px] text-destructive">*</span>{{ $t('order.invoiceType') }}
            </label>
            <Select
              v-model:value="form.invoiceTypeId"
              class="w-full"
              :disabled="!canEditMarketing"
              :options="invoiceTypeOptions"
              :field-names="{ label: 'label', value: 'value' }"
              placeholder="--请选择--"
            />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground"><span class="mr-[2px] text-destructive">*</span>{{ $t('order.installationFee') }}</label>
            <InputNumber v-model:value="form.installationFee" class="w-full" :min="0" :precision="2" :disabled="!canEditMarketing" />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground"><span class="mr-[2px] text-destructive">*</span>{{ $t('order.estimatedFreight') }}</label>
            <InputNumber v-model:value="form.estimatedFreight" class="w-full" :min="0" :precision="2" :disabled="!canEditMarketing" />
          </div>
          <!-- 合同上传与列表 -->
          <div v-if="hasPermission(PermissionCodes.OrderContractUpload)" class="md:col-span-4">
            <div class="mb-2 flex flex-wrap items-center gap-2">
              <Button v-if="canEditMarketing" type="primary" danger size="small" @click="triggerContractUpload">
                + {{ $t('order.contractUpload') }}
              </Button>
              <input
                v-if="canEditMarketing"
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
                    <Button v-if="canEditMarketing" type="link" size="small" danger class="p-0 min-w-0 h-auto leading-normal" @click="removeContractFile(record as OrderApi.OrderContractFileItem)">{{ $t('order.delete') }}</Button>
                  </div>
                </template>
              </template>
            </Table>
          </div>


          <div class="md:col-span-4">
            <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.remark') }}</label>
            <Input.TextArea v-model:value="form.remark" :rows="2" placeholder="" :disabled="!canEditMarketing" />
          </div>
        </div>
      </div>

      <!-- 财务部 -->
      <div class="border-border mt-4 border-t pt-4">
        <h4 class="order-section-title">{{ $t('order.sectionFinance') }}</h4>
        <div class="grid gap-4 md:grid-cols-4">
          <div v-if="hasPermission(PermissionCodes.OrderContractSelect)">
            <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.selectContract') }}</label>
            <Select
              v-model:value="form.selectedContractFileId"
              allow-clear
              class="w-full"
              :disabled="!canEditFinance"
              :options="[
                { label: $t('order.no'), value: 0 },
                { label: $t('order.yes'), value: 1 },
              ]"
              placeholder="--请选择--"
            />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.isShipped') }}</label>
            <Select
              :value="form.isShipped ? 1 : 0"
              class="w-full"
              :disabled="!canEditFinance"
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
              :disabled="!canEditFinance"
              :options="paymentStatusOptions"
              placeholder="--请选择--"
            />
          </div>
          <div v-if="hasPermission(PermissionCodes.OrderContractNotCompanyTemplate)" class="flex items-center">
            <label class="flex cursor-pointer items-center gap-2 text-sm font-medium text-foreground">
              <input
                v-model="form.contractNotCompanyTemplate"
                type="checkbox"
                class="h-4 w-4 rounded border-input"
                :disabled="!canEditFinance"
              />
              {{ $t('order.contractNotCompanyTemplate') }}
            </label>
          </div>
          <template v-if="distinctCategoriesFromItems.length > 0">
            <div v-for="(row, idx) in orderCategoriesForm" :key="row.productCategoryId">
              <label class="mb-1 block text-sm font-medium text-foreground">{{
                $t('order.contractDiscountWithCategory', [productCategoryNameInitial(row.categoryName)])
              }}</label>
              <InputNumber
                v-model:value="orderCategoriesForm[idx].discountPoints"
                class="w-full"
                :min="0"
                :precision="2"
                :disabled="!canEditFinance"
              />
            </div>
          </template>
          <div v-if="hasPermission(PermissionCodes.OrderContractAmount)">
            <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.contractAmount') }}</label>
            <InputNumber v-model:value="form.contractAmount" class="w-full" :min="0" :precision="2" :disabled="!canEditFinance" />
          </div>
        </div>

        <!-- 订单备注（财务可新增） -->
        <div class="mt-4">
          <div class="mb-2 flex items-center justify-between gap-2">
            <span class="text-sm font-medium text-foreground">{{ $t('order.orderRemarks') }}</span>
            <Button
              v-if="
                !isNew &&
                hasPermission(PermissionCodes.OrderContractAmount) &&
                Number(form.paymentStatus) === INSTALLMENT_URGENT_PAYMENT_STATUS
              "
              type="primary"
              size="small"
              @click="openAddOrderRemarkModal"
            >
              + {{ $t('order.addOrderRemark') }}
            </Button>
          </div>

          <Table
            v-if="orderRemarks.length > 0"
            :columns="orderRemarkColumns"
            :data-source="orderRemarks"
            :pagination="false"
            size="small"
            bordered
            row-key="id"
          >
            <template #bodyCell="{ column, record }">
              <template v-if="column.key === 'action'">
                <div class="flex flex-wrap gap-x-1 gap-y-0.5">
                  <Button
                    v-if="
                      !isNew &&
                      hasPermission(PermissionCodes.OrderContractAmount) &&
                      Number(form.paymentStatus) === INSTALLMENT_URGENT_PAYMENT_STATUS
                    "
                    type="link"
                    size="small"
                    class="p-0 min-w-0 h-auto leading-normal"
                    @click="openEditOrderRemarkModal(record as OrderApi.OrderRemarkDto)"
                  >
                    {{ $t('order.editOrderRemark') }}
                  </Button>
                  <Button
                    v-if="
                      !isNew &&
                      hasPermission(PermissionCodes.OrderContractAmount) &&
                      Number(form.paymentStatus) === INSTALLMENT_URGENT_PAYMENT_STATUS
                    "
                    type="link"
                    size="small"
                    danger
                    class="p-0 min-w-0 h-auto leading-normal"
                    @click="confirmDeleteOrderRemark(record as OrderApi.OrderRemarkDto)"
                  >
                    {{ $t('order.deleteOrderRemark') }}
                  </Button>
                </div>
              </template>
              <template v-else-if="column.key === 'userName'">
                {{ (record as OrderApi.OrderRemarkDto).userName || '-' }}
              </template>
              <template v-else-if="column.key === 'addedAt'">
                {{ (record as OrderApi.OrderRemarkDto).addedAt ? new Date((record as OrderApi.OrderRemarkDto).addedAt).toLocaleString() : '' }}
              </template>
              <template v-else-if="column.key === 'content'">
                <span class="whitespace-pre-wrap">{{ (record as OrderApi.OrderRemarkDto).content }}</span>
              </template>
            </template>
          </Table>
          <div v-else class="text-sm text-muted-foreground">暂无订单备注</div>
        </div>
      </div>

      <!-- 物流与收货 -->
      <div class="border-border mt-4 border-t pt-4">
        <h4 class="order-section-title">{{ $t('order.sectionLogistics') }}</h4>
        <div class="grid gap-4 md:grid-cols-4">
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.receiverName') }}</label>
            <Input v-model:value="form.receiverName" :disabled="!canEditLogistics" />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.receiverPhone') }}</label>
            <Input v-model:value="form.receiverPhone" :disabled="!canEditLogistics" />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.receiverAddress') }}</label>
            <Input v-model:value="form.receiverAddress" :disabled="!canEditLogistics" />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.deliveryDate') }}</label>
            <DatePicker
              v-model:value="form.deliveryDate"
              value-format="YYYY-MM-DD"
              class="w-full"
              style="width: 100%"
              :disabled="!canEditLogistics"
              placeholder="选择发货日期"
            />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.logisticsCompany') }}</label>
            <Select
              v-model:value="form.orderLogisticsCompanyId"
              allow-clear
              class="w-full"
              :disabled="!canEditLogistics"
              :options="logisticsCompanyOptions"
              :field-names="{ label: 'label', value: 'value' }"
              placeholder="--请选择--"
            />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.logisticsMethod') }}</label>
            <Select
              v-model:value="form.orderLogisticsMethodId"
              allow-clear
              class="w-full"
              :disabled="!canEditLogistics"
              :options="logisticsMethodOptions"
              :field-names="{ label: 'label', value: 'value' }"
              placeholder="--请选择--"
            />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.logisticsPaymentMethod') }}</label>
            <Select
              v-model:value="form.logisticsPaymentMethodId"
              allow-clear
              class="w-full"
              :disabled="!canEditLogistics"
              :options="logisticsPaymentMethodOptions"
              placeholder="--请选择--"
            />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.waybillNumber') }}</label>
            <Input v-model:value="form.waybillNumber" :disabled="!canEditLogistics" />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.shippingFee') }}</label>
            <InputNumber v-model:value="form.shippingFee" class="w-full" :min="0" :precision="2" :disabled="!canEditLogistics" />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.surcharge') }}</label>
            <InputNumber v-model:value="form.surcharge" class="w-full" :min="0" :precision="2" :disabled="!canEditLogistics" />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.payDate') }}</label>
            <DatePicker
              v-model:value="form.payDate"
              value-format="YYYY-MM-DD"
              class="w-full"
              style="width: 100%"
              :disabled="!canEditLogistics"
              placeholder="选择付款日期"
            />
          </div>
          <div class="flex items-center">
            <label class="flex cursor-pointer items-center gap-2 text-sm font-medium text-foreground">
              <input
                v-model="form.shippingFeeIsPay"
                type="checkbox"
                class="h-4 w-4 rounded border-input"
                :disabled="!canEditLogistics"
              />
              {{ $t('order.shippingFeeIsPay') }}
            </label>
          </div>

        </div>
      </div>

        <!-- 卡片底部操作按钮 -->
        <div class="mt-6 flex justify-end gap-3 border-t border-border pt-4">
          <Button @click="goBack">{{ $t('common.cancel') }}</Button>
          <Button
            v-if="showSubmitApprovalButton"
            type="dashed"
            :loading="submitting"
            @click="onSubmitForApproval"
          >
            {{ $t('order.submit') }}
          </Button>
          <Button type="primary" :loading="submitting" @click="onSubmit">
            {{ $t('common.confirm') }}
          </Button>
        </div>
      </Card>

      <!-- 产品明细（卡片） -->
      <div class="border-border mt-4 border-t pt-4">
        <Card :bordered="true" class="border-border bg-card">
          <template #title>
            <div class="flex w-full items-center justify-between">
              <span class="order-section-title mb-0">{{ $t('order.sectionProductList') }}</span>
              <Button v-if="canEditProducts" type="primary" size="small" @click="addItem">{{ $t('order.addItem') }}</Button>
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
            <template v-if="column.key === 'productCategoryName'">
              <span>{{ record.productCategoryName || '-' }}</span>
            </template>
            <template v-else-if="column.key === 'productId'">
              <Select
                v-model:value="record.productId"
                allow-clear
                show-search
                class="w-full"
                :disabled="!canEditProducts"
                :options="productOptions"
                :field-names="{ label: 'label', value: 'value' }"
                placeholder="选择产品"
                @change="(val: string | undefined) => canEditProducts && syncOrderItemFromProduct(val, index)"
              />
            </template>
            <template v-else-if="column.key === 'productName'">
              <Input v-model:value="record.productName" size="small" :disabled="!canEditProducts" />
            </template>
            <template v-else-if="column.key === 'model'">
              <Input v-model:value="record.model" size="small" :disabled="!canEditProducts" />
            </template>
            <template v-else-if="column.key === 'number'">
              <Input v-model:value="record.number" size="small" :disabled="!canEditProducts" />
            </template>
            <template v-else-if="column.key === 'qty'">
              <InputNumber
                v-model:value="record.qty"
                size="small"
                :min="1"
                class="w-full"
                :disabled="!canEditProducts"
                @change="() => canEditProducts && recalcItemAmount(index)"
              />
            </template>
            <template v-else-if="column.key === 'unit'">
              <Input v-model:value="record.unit" size="small" :disabled="!canEditProducts" />
            </template>
            <template v-else-if="column.key === 'price'">
              <InputNumber
                v-model:value="record.price"
                size="small"
                :min="0"
                :precision="2"
                class="w-full"
                :disabled="!canEditProducts"
                @change="() => canEditProducts && recalcItemAmount(index)"
              />
            </template>
            <template v-else-if="column.key === 'amount'">
              {{ record.amount }}
            </template>
            <template v-else-if="column.key === 'remark'">
              <Input v-model:value="record.remark" size="small" :disabled="!canEditProducts" />
            </template>
            <template v-else-if="column.key === 'action'">
              <Button v-if="canEditProducts" type="link" size="small" danger @click="removeItem(index)">{{ $t('order.delete') }}</Button>
            </template>
          </template>
        </Table>
        </Card>
      </div>

      <!-- 备货单 -->
      <div class="border-border mt-4 border-t pt-4">
        <Card :bordered="true" class="border-border bg-card">
          <template #title>
            <span class="order-section-title mb-0">{{ $t('order.stockUpload') }}</span>
          </template>
          <div class="md:col-span-4">
            <div class="mb-2 flex flex-wrap items-center gap-2">
              <Button v-if="canEditMarketing" type="primary" danger size="small" @click="triggerStockUpload">
                + {{ $t('order.stockUpload') }}
              </Button>
              <input
                v-if="canEditMarketing"
                ref="stockFileInputRef"
                type="file"
                class="hidden"
                accept=".docx,.pdf,.xlsx,.jpg,.jpeg,.png,.gif,.bmp,.txt"
                @change="onStockFileSelected"
              />
              <Button size="small" @click="refreshStockList">{{ $t('order.refresh') }}</Button>
              <Input
                v-model:value="stockFileSearchKeyword"
                size="small"
                class="ml-auto w-40"
                :placeholder="$t('order.searchPlaceholder')"
                allow-clear
              />
            </div>
            <Table
              :columns="stockFileColumns"
              :data-source="filteredStockFiles"
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
                    <Button type="link" size="small" class="p-0 min-w-0 h-auto leading-normal" @click="previewStockFile(record as OrderApi.OrderContractFileItem)">{{ $t('order.preview') }}</Button>
                    <Button type="link" size="small" class="p-0 min-w-0 h-auto leading-normal" @click="downloadStockFile(record as OrderApi.OrderContractFileItem)">{{ $t('order.download') }}</Button>
                    <Button type="link" size="small" class="p-0 min-w-0 h-auto leading-normal" @click="printStockFile(record as OrderApi.OrderContractFileItem)">{{ $t('order.print') }}</Button>
                    <Button v-if="canEditMarketing" type="link" size="small" danger class="p-0 min-w-0 h-auto leading-normal" @click="removeStockFile(record as OrderApi.OrderContractFileItem)">{{ $t('order.delete') }}</Button>
                  </div>
                </template>
              </template>
            </Table>
          </div>
        </Card>
      </div>

    </div>

    <Modal
      v-model:open="remarkModalOpen"
      :title="$t('order.addOrderRemark')"
      :confirm-loading="addingOrderRemark"
      :ok-text="$t('common.confirm')"
      :cancel-text="$t('common.cancel')"
      @ok="confirmAddOrderRemark"
      @cancel="remarkModalOpen = false"
    >
      <Input.TextArea
        v-model:value="remarkContent"
        :rows="6"
        :placeholder="$t('order.remarkContent')"
      />
    </Modal>

    <Modal
      v-model:open="remarkEditModalOpen"
      :title="$t('order.editOrderRemark')"
      :confirm-loading="updatingOrderRemark"
      :ok-text="$t('common.confirm')"
      :cancel-text="$t('common.cancel')"
      @ok="confirmEditOrderRemark"
      @cancel="remarkEditModalOpen = false"
    >
      <Input.TextArea
        v-model:value="remarkEditContent"
        :rows="6"
        :placeholder="$t('order.remarkContent')"
      />
    </Modal>

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
