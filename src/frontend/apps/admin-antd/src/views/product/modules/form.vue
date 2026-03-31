<script lang="ts" setup>
import type { ProductApi } from '#/api/system/product';
import type { ProductCategoryTreeItem } from '#/api/system/product';
import type { ProductParameterItem } from '#/api/system/product';

import { ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { Button, Card, message, Modal, Table } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import {
  createProduct,
  createProductParameter,
  deleteProductParameter,
  getProduct,
  getProductCategoryTree,
  getProductParameterList,
  getSupplierList,
  updateProduct,
  updateProductParameter,
} from '#/api/system/product';
import { $t } from '#/locales';

import { useSchema } from '../data';

function toCategoryTreeOptions(
  nodes: ProductCategoryTreeItem[],
): { value: string; label: string; children?: { value: string; label: string; children?: unknown[] }[] }[] {
  return nodes.map((n) => ({
    value: n.id,
    label: n.name,
    children: n.children?.length ? toCategoryTreeOptions(n.children) : undefined,
  }));
}

const emit = defineEmits(['success']);
const formData = ref<Partial<ProductApi.ProductItem> & { id?: string }>();
const categoryTreeOptions = ref<{ value: string; label: string; children?: unknown[] }[]>([]);
const supplierOptions = ref<{ value: string; label: string }[]>([]);
const parameterList = ref<ProductParameterItem[]>([]);
const parameterModalVisible = ref(false);
const parameterModalEditing = ref<ProductParameterItem | null>(null);
const parameterForm = ref({ year: '', description: '' });
const parameterLoading = ref(false);

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
  parameterForm.value = { year: row.year, description: row.description };
  parameterModalVisible.value = true;
}

async function saveParameter() {
  const { year, description } = parameterForm.value;
  if (!year?.trim()) {
    message.warning('请填写年份');
    return;
  }
  const productId = formData.value?.id;
  if (!productId) return;
  parameterLoading.value = true;
  try {
    if (parameterModalEditing.value) {
      await updateProductParameter(parameterModalEditing.value.id, { year: year.trim(), description: description?.trim() ?? '' });
      message.success('更新成功');
    } else {
      await createProductParameter(productId, { year: year.trim(), description: description?.trim() ?? '' });
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
      if (formData.value?.id) await loadParameters(formData.value.id);
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
  schema: useSchema({
    categoryTreeOptions,
    supplierOptions,
  }),
  showDefaultActions: false,
});

function resetForm() {
  formApi.resetForm();
  formApi.setValues(formData.value || {});
}

const [Drawer, drawerApi] = useVbenDrawer({
  async onConfirm() {
    const { valid } = await formApi.validate();
    if (!valid) return;
    drawerApi.lock();
    const data = await formApi.getValues();
    const payload: ProductApi.ProductCreateUpdate = {
      productType: Number(data.productType ?? 1),
      status: Number(data.status ?? 1),
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
      operationProcessResources: String(data.operationProcessResources ?? ''),
      introduction: String(data.introduction ?? ''),
      introductionResources: String(data.introductionResources ?? ''),
      imagePath: String(data.imagePath ?? ''),
      categoryId: data.categoryId && String(data.categoryId).trim() ? String(data.categoryId) : null,
      supplierId: data.supplierId && String(data.supplierId).trim() ? String(data.supplierId) : null,
    };
    try {
      if (formData.value?.id) {
        await updateProduct(formData.value.id, payload);
      } else {
        await createProduct(payload);
      }
      drawerApi.close();
      emit('success');
    } finally {
      drawerApi.lock(false);
    }
  },
  async onOpenChange(isOpen) {
    if (isOpen) {
      const data = drawerApi.getData<
        Partial<ProductApi.ProductItem> & { id?: string }
      >();
      formData.value = data;
      if (data?.id) {
        const [detail, treeRes, supplierRes] = await Promise.all([
          getProduct(data.id),
          getProductCategoryTree(false),
          getSupplierList(),
          loadParameters(data.id),
        ]);
        const tree = Array.isArray(treeRes) ? treeRes : [];
        categoryTreeOptions.value = toCategoryTreeOptions(tree);
        supplierOptions.value = (Array.isArray(supplierRes) ? supplierRes : []).map((s) => ({
          value: s.id,
          label: s.fullName,
        }));

        formApi.setValues({
          ...detail,
          productType: detail?.productType ?? 1,
          status: detail?.status ?? 1,
        });
      } else {
        const [treeRes, supplierRes] = await Promise.all([
          getProductCategoryTree(false),
          getSupplierList(),
        ]);
        const tree = Array.isArray(treeRes) ? treeRes : [];
        categoryTreeOptions.value = toCategoryTreeOptions(tree);
        supplierOptions.value = (Array.isArray(supplierRes) ? supplierRes : []).map((s) => ({
          value: s.id,
          label: s.fullName,
        }));
        formApi.setValues({
          productType: data?.productType ?? 1,
          status: data?.status ?? 1,
          name: data?.name ?? '',
          code: data?.code ?? '',
          model: data?.model ?? '',
          unit: data?.unit ?? '',
          barcode: data?.barcode ?? '',
          activationCode: data?.activationCode ?? '',
          priceStandard: data?.priceStandard ?? '',
          marketSales: data?.marketSales ?? '',
          description: data?.description ?? '',
          costPrice: data?.costPrice ?? 0,
          customerPrice: data?.customerPrice ?? 0,
          qty: data?.qty ?? 0,
          tags: data?.tags ?? '',
          feature: data?.feature ?? '',
          configuration: data?.configuration ?? '',
          instructions: data?.instructions ?? '',
          installProcess: data?.installProcess ?? '',
          operationProcessResources: data?.operationProcessResources ?? '',
          introduction: data?.introduction ?? '',
          introductionResources: data?.introductionResources ?? '',
          imagePath: data?.imagePath ?? '',
          categoryId: data?.categoryId ?? undefined,
          supplierId: data?.supplierId ?? undefined,
        });
      }
    }
  },
});
</script>

<template>
  <Drawer
    :title="
      formData?.id
        ? $t('product.edit')
        : $t('product.create')
    "
  >
    <Form class="mx-4" />
    <Card v-if="formData?.id" class="mx-4 mt-4" title="产品参数" size="small">
      <template #extra>
        <Button size="small" type="primary" @click="openAddParameter">添加参数</Button>
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
            <Button type="link" size="small" @click="openEditParameter(record as ProductParameterItem)">编辑</Button>
            <Button type="link" danger size="small" @click="confirmDeleteParameter(record as ProductParameterItem)">删除</Button>
          </template>
        </template>
      </Table>
    </Card>
    <Modal
      v-model:open="parameterModalVisible"
      :title="parameterModalEditing ? '编辑参数' : '添加参数'"
      :confirm-loading="parameterLoading"
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
    <template #prepend-footer>
      <Button type="primary" danger @click="resetForm">
        {{ $t('common.reset') }}
      </Button>
    </template>
  </Drawer>
</template>
