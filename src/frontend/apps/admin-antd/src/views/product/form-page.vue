<script lang="ts" setup>
import type { ProductApi } from '#/api/system/product';
import type { ProductCategoryTreeItem } from '#/api/system/product';
import type { ProductParameterItem } from '#/api/system/product';

import { computed, onMounted, provide, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';
import { ArrowLeft } from '@vben/icons';

import { Button, Card, message, Modal, Table } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import {
  createProduct,
  createProductParameter,
  deleteProductParameter,
  getProduct,
  getProductCategoryTree,
  getProductParameterList,
  getProductTypeList,
  getSupplierList,
  updateProduct,
  updateProductParameter,
} from '#/api/system/product';
import { uploadFile } from '#/api/system/file';
import { $t } from '#/locales';

import { useSchema } from './data';

function toCategoryTreeOptions(
  nodes: ProductCategoryTreeItem[],
): { value: string; label: string; children?: { value: string; label: string; children?: unknown[] }[] }[] {
  return nodes.map((n) => ({
    value: n.id,
    label: n.name,
    children: n.children?.length ? toCategoryTreeOptions(n.children) : undefined,
  }));
}

const route = useRoute();
const router = useRouter();

const id = computed(() => route.params.id as string | undefined);
const isNew = computed(() => !id.value);

const categoryTreeOptions = ref<{ value: string; label: string; children?: unknown[] }[]>([]);
const productTypeOptions = ref<{ value: string; label: string }[]>([]);
const supplierOptions = ref<{ value: string; label: string }[]>([]);
const parameterList = ref<ProductParameterItem[]>([]);
const parameterModalVisible = ref(false);
const parameterModalEditing = ref<ProductParameterItem | null>(null);
const parameterForm = ref({ year: '', description: '' });
const parameterLoading = ref(false);
const pageLoading = ref(true);
const submitting = ref(false);

/** 操作流程资源、产品介绍资源：上传得到的 path 列表（提交时序列化为 JSON 字符串） */
const operationProcessResourcesList = ref<string[]>([]);
const introductionResourcesList = ref<string[]>([]);

const operationProcessUploading = ref(false);
const introductionUploading = ref(false);

async function loadParameters(productId: string) {
  try {
    const list = await getProductParameterList(productId);
    parameterList.value = Array.isArray(list) ? list : [];
  } catch {
    parameterList.value = [];
  }
}

function openAddParameter() {
  parameterModalEditing.value = null;
  parameterForm.value = { year: '', description: '' };
  parameterModalVisible.value = true;
}

function openEditParameter(row: ProductParameterItem) {
  parameterModalEditing.value = row;
  parameterForm.value = { year: row.year, description: row.description ?? '' };
  parameterModalVisible.value = true;
}

async function saveParameter() {
  const { year, description } = parameterForm.value;
  if (!year?.trim()) {
    message.warning('请填写年份');
    return;
  }
  const productId = id.value;
  if (!productId) return;
  parameterLoading.value = true;
  try {
    if (parameterModalEditing.value) {
      await updateProductParameter(parameterModalEditing.value.id, {
        year: year.trim(),
        description: description?.trim() ?? '',
      });
      message.success('更新成功');
    } else {
      await createProductParameter(productId, {
        year: year.trim(),
        description: description?.trim() ?? '',
      });
      message.success('添加成功');
    }
    parameterModalVisible.value = false;
    await loadParameters(productId);
  } finally {
    parameterLoading.value = false;
  }
}

function confirmDeleteParameter(row: ProductParameterItem) {
  Modal.confirm({
    title: '确定删除该参数？',
    onOk: async () => {
      await deleteProductParameter(row.id);
      message.success('已删除');
      if (id.value) await loadParameters(id.value);
    },
  });
}

const parameterColumns = [
  { title: '年份', dataIndex: 'year', key: 'year', width: 100 },
  { title: '描述', dataIndex: 'description', key: 'description', ellipsis: true },
  { title: '操作', key: 'action', width: 120 },
];

const [Form, formApi] = useVbenForm({
  layout: 'vertical',
  schema: computed(() => useSchema({ categoryTreeOptions, productTypeOptions, supplierOptions })),
  showDefaultActions: false,
  wrapperClass: 'grid-cols-2 gap-y-4',
});

async function loadOptions() {
  const [treeRes, typeRes, supplierRes] = await Promise.all([
    getProductCategoryTree(false),
    getProductTypeList(false),
    getSupplierList(),
  ]);
  const tree = Array.isArray(treeRes) ? treeRes : [];
  categoryTreeOptions.value = toCategoryTreeOptions(tree);
  productTypeOptions.value = (Array.isArray(typeRes) ? typeRes : []).map((t) => ({
    value: t.id,
    label: t.name,
  }));
  supplierOptions.value = (Array.isArray(supplierRes) ? supplierRes : []).map((s) => ({
    value: s.id,
    label: s.fullName,
  }));
}

async function loadProductDetail() {
  const productId = id.value;
  if (!productId) {
    operationProcessResourcesList.value = [];
    introductionResourcesList.value = [];
    formApi.setValues({
      productTypeId: '',
      status: true,
      name: '',
      code: '',
      model: '',
      unit: '',
      barcode: '',
      activationCode: '',
      priceStandard: '',
      marketSales: '',
      description: '',
      costPrice: 0,
      customerPrice: 0,
      qty: 0,
      tags: '',
      feature: '',
      configuration: '',
      instructions: '',
      installProcess: '',
      introduction: '',
      imagePath: '',
      categoryId: undefined,
      supplierId: undefined,
    });
    return;
  }
  const [detail] = await Promise.all([getProduct(productId), loadParameters(productId)]);
  try {
    operationProcessResourcesList.value = JSON.parse(detail?.operationProcessResources ?? '[]');
  } catch {
    operationProcessResourcesList.value = [];
  }
  try {
    introductionResourcesList.value = JSON.parse(detail?.introductionResources ?? '[]');
  } catch {
    introductionResourcesList.value = [];
  }
  formApi.setValues({
    productTypeId: detail?.productTypeId ?? '',
    status: detail?.status ?? true,
    name: detail?.name ?? '',
    code: detail?.code ?? '',
    model: detail?.model ?? '',
    unit: detail?.unit ?? '',
    barcode: detail?.barcode ?? '',
    activationCode: detail?.activationCode ?? '',
    priceStandard: detail?.priceStandard ?? '',
    marketSales: detail?.marketSales ?? '',
    description: detail?.description ?? '',
    costPrice: detail?.costPrice ?? 0,
    customerPrice: detail?.customerPrice ?? 0,
    qty: detail?.qty ?? 0,
    tags: detail?.tags ?? '',
    feature: detail?.feature ?? '',
    configuration: detail?.configuration ?? '',
    instructions: detail?.instructions ?? '',
    installProcess: detail?.installProcess ?? '',
    introduction: detail?.introduction ?? '',
    imagePath: detail?.imagePath ?? '',
    categoryId: detail?.categoryId ?? undefined,
    supplierId: detail?.supplierId ?? undefined,
  });
}

onMounted(async () => {
  pageLoading.value = true;
  try {
    await loadOptions();
    await loadProductDetail();
  } finally {
    pageLoading.value = false;
  }
});

async function onSubmit() {
  const { valid } = await formApi.validate();
  if (!valid) return;
  submitting.value = true;
  try {
    const data = await formApi.getValues();
    const payload: ProductApi.ProductCreateUpdate = {
      productTypeId: String(data.productTypeId ?? ''),
      status: Boolean(data.status ?? true),
      name: String(data.name ?? ''),
      code: String(data.code ?? ''),
      model: String(data.model ?? ''),
      unit: String(data.unit ?? ''),
      barcode: String(data.barcode ?? ''),
      activationCode: String(data.activationCode ?? ''),
      priceStandard: String(data.priceStandard ?? ''),
      marketSales: String(data.marketSales ?? ''),
      description: String(data.description ?? ''),
      costPrice: Number(data.costPrice ?? 0),
      customerPrice: Number(data.customerPrice ?? 0),
      qty: Number(data.qty ?? 0),
      tags: String(data.tags ?? ''),
      feature: String(data.feature ?? ''),
      configuration: String(data.configuration ?? ''),
      instructions: String(data.instructions ?? ''),
      installProcess: String(data.installProcess ?? ''),
      operationProcessResources: JSON.stringify(operationProcessResourcesList.value),
      introduction: String(data.introduction ?? ''),
      introductionResources: JSON.stringify(introductionResourcesList.value),
      imagePath: String(data.imagePath ?? ''),
      categoryId: data.categoryId && String(data.categoryId).trim() ? String(data.categoryId) : null,
      supplierId: data.supplierId && String(data.supplierId).trim() ? String(data.supplierId) : null,
    };
    if (id.value) {
      await updateProduct(id.value, payload);
      message.success($t('common.success'));
    } else {
      await createProduct(payload);
      message.success($t('common.success'));
    }
    router.push('/product/list');
  } finally {
    submitting.value = false;
  }
}

function goBack() {
  router.push('/product/list');
}

function resetForm() {
  formApi.resetForm();
  loadProductDetail();
}

const operationProcessFileInput = ref<HTMLInputElement | null>(null);
const introductionFileInput = ref<HTMLInputElement | null>(null);

async function onOperationProcessFileChange(e: Event) {
  const input = e.target as HTMLInputElement;
  const file = input.files?.[0];
  if (!file) return;
  operationProcessUploading.value = true;
  try {
    const { path } = await uploadFile(file);
    operationProcessResourcesList.value = [...operationProcessResourcesList.value, path];
    message.success($t('common.success'));
  } catch {
    message.error($t('common.uploadFailed') ?? '上传失败');
  } finally {
    operationProcessUploading.value = false;
    input.value = '';
  }
}

async function onIntroductionFileChange(e: Event) {
  const input = e.target as HTMLInputElement;
  const file = input.files?.[0];
  if (!file) return;
  introductionUploading.value = true;
  try {
    const { path } = await uploadFile(file);
    introductionResourcesList.value = [...introductionResourcesList.value, path];
    message.success($t('common.success'));
  } catch {
    message.error($t('common.uploadFailed') ?? '上传失败');
  } finally {
    introductionUploading.value = false;
    input.value = '';
  }
}

function removeOperationProcessPath(index: number) {
  operationProcessResourcesList.value = operationProcessResourcesList.value.filter((_, i) => i !== index);
}

function removeIntroductionPath(index: number) {
  introductionResourcesList.value = introductionResourcesList.value.filter((_, i) => i !== index);
}

// 操作流程/产品介绍资源上传块（由 schema 内 ProductResourceUpload 通过 inject 使用）
provide('productResourceUpload', {
  operation: {
    list: operationProcessResourcesList,
    loading: operationProcessUploading,
    trigger: () => operationProcessFileInput.value?.click(),
    remove: removeOperationProcessPath,
  },
  introduction: {
    list: introductionResourcesList,
    loading: introductionUploading,
    trigger: () => introductionFileInput.value?.click(),
    remove: removeIntroductionPath,
  },
});
</script>

<template>
  <Page
    :loading="pageLoading"
    auto-content-height
    content-class="flex flex-col"
  >
    <div class="mb-4 flex items-center gap-2">
      <Button class="inline-flex items-center gap-1" @click="goBack">
        <ArrowLeft class="size-4 shrink-0" />
        {{ $t('common.back') }}
      </Button>
      <span class="text-lg font-medium">
        {{ isNew ? $t('product.create') : $t('product.edit') }}
      </span>
    </div>

    <div class="w-full min-w-0 flex-1 space-y-6">
      <Form />
      <!-- 隐藏的 file input，供表单内「上传操作资源/上传介绍资源」按钮触发 -->
      <div class="hidden">
        <input
          ref="operationProcessFileInput"
          type="file"
          @change="onOperationProcessFileChange"
        />
        <input
          ref="introductionFileInput"
          type="file"
          @change="onIntroductionFileChange"
        />
      </div>
      <!-- 参数上传（仅编辑页展示） -->
      <template v-if="id">
        <Card
          :bordered="true"
          class="border-border bg-card"
          size="small"
          :title="$t('product.parameterUpload')"
        >
          <template #extra>
            <Button size="small" type="primary" @click="openAddParameter">
              添加参数
            </Button>
          </template>
          <Table
            :columns="parameterColumns"
            :data-source="parameterList"
            :pagination="false"
            row-key="id"
            size="small"
          >
            <template #bodyCell="{ column, record }">
              <template v-if="column.key === 'action'">
                <Button
                  type="link"
                  size="small"
                  @click="openEditParameter(record as ProductParameterItem)"
                >
                  编辑
                </Button>
                <Button
                  type="link"
                  danger
                  size="small"
                  @click="confirmDeleteParameter(record as ProductParameterItem)"
                >
                  删除
                </Button>
              </template>
            </template>
          </Table>
        </Card>
      </template>
      <div class="mt-6 flex gap-2">
        <Button type="primary" :loading="submitting" :disabled="submitting" @click="onSubmit">
          {{ $t('common.confirm') }}
        </Button>
        <Button type="primary" danger @click="resetForm">
          {{ $t('common.reset') }}
        </Button>
        <Button @click="goBack">
          {{ $t('common.cancel') }}
        </Button>
      </div>
    </div>

    <Modal
      v-model:open="parameterModalVisible"
      :confirm-loading="parameterLoading"
      :title="parameterModalEditing ? '编辑参数' : '添加参数'"
      @ok="saveParameter"
    >
      <div class="py-2">
        <div class="mb-2">
          <span class="text-red-500">*</span> 年份：
          <input
            v-model="parameterForm.year"
            class="w-full rounded border px-2 py-1"
            placeholder="如 2024"
          />
        </div>
        <div>
          描述：
          <textarea
            v-model="parameterForm.description"
            class="w-full rounded border px-2 py-1"
            placeholder="参数内容"
            rows="3"
          />
        </div>
      </div>
    </Modal>
  </Page>
</template>
