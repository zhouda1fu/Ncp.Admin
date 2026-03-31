<script lang="ts" setup>
import type { ProductCategoryTreeItem } from '#/api/system/product';

import { computed, onMounted, ref } from 'vue';
import { Page, useVbenDrawer } from '@vben/common-ui';
import { Plus } from '@vben/icons';

import { Button, Card, message, Modal, Spin, Tree } from 'ant-design-vue';
import type { TreeProps } from 'ant-design-vue';

import {
  deleteProductCategory,
  getProductCategoryTree,
} from '#/api/system/product';
import { $t } from '#/locales';

import CategoryForm from './modules/category-form.vue';
import type { CategoryFormData } from './modules/category-form.vue';

const loading = ref(true);
const treeData = ref<TreeProps['treeData']>([]);
const categoryTreeRaw = ref<ProductCategoryTreeItem[]>([]);

const [FormDrawer, formDrawerApi] = useVbenDrawer({
  connectedComponent: CategoryForm,
  destroyOnClose: true,
});

function toTreeNodes(nodes: ProductCategoryTreeItem[]): TreeProps['treeData'] {
  return nodes.map((n) => ({
    key: n.id,
    title: n.name,
    children: n.children?.length ? toTreeNodes(n.children) : undefined,
  }));
}

function flattenCategoryNodes(nodes: ProductCategoryTreeItem[]): ProductCategoryTreeItem[] {
  const result: ProductCategoryTreeItem[] = [];
  for (const n of nodes) {
    result.push(n);
    if (n.children?.length) result.push(...flattenCategoryNodes(n.children));
  }
  return result;
}

const categoryNodeMap = computed(() => {
  const flat = flattenCategoryNodes(categoryTreeRaw.value);
  return Object.fromEntries(flat.map((n) => [n.id, n]));
});

/** 从接口节点中取出 id 字符串（兼容 id 为对象或 string） */
function getNodeId(node: unknown): string {
  if (!node || typeof node !== 'object') return '';
  const n = node as Record<string, unknown>;
  const id = n.id ?? n.Id;
  if (typeof id === 'string') return id;
  if (id && typeof id === 'object' && typeof (id as Record<string, unknown>).value === 'string') return (id as Record<string, unknown>).value as string;
  return id != null ? String(id) : '';
}

/** 将接口返回的树节点规范为 ProductCategoryTreeItem（兼容多种返回格式） */
function normalizeTreeList(nodes: unknown): ProductCategoryTreeItem[] {
  if (!Array.isArray(nodes)) return [];
  return nodes.map((node) => {
    const n = node as Record<string, unknown>;
    const children = Array.isArray(n.children) ? normalizeTreeList(n.children) : [];
    return {
      id: getNodeId(n) || (n.id as string) || '',
      name: String(n.name ?? n.Name ?? ''),
      remark: String(n.remark ?? n.Remark ?? ''),
      parentId: n.parentId != null ? String(n.parentId) : (n.ParentId != null ? String(n.ParentId) : null),
      sortOrder: Number(n.sortOrder ?? n.SortOrder ?? 0),
      visible: Boolean(n.visible ?? n.Visible ?? true),
      isDiscount: Boolean(n.isDiscount ?? n.IsDiscount ?? false),
      children,
    } as ProductCategoryTreeItem;
  });
}

async function loadTree(forceRefresh = false) {
  loading.value = true;
  try {
    let data: unknown;
    try {
      data = await getProductCategoryTree(true, forceRefresh);
    } catch (e) {
      message.error('加载产品分类失败，请检查网络或权限');
      return;
    }
    const raw = Array.isArray(data) ? data : (data && typeof data === 'object' && 'data' in data && Array.isArray((data as { data: unknown }).data) ? (data as { data: unknown[] }).data : []);
    const list = normalizeTreeList(raw);
    categoryTreeRaw.value = list;
    treeData.value = toTreeNodes(list);
  } finally {
    loading.value = false;
  }
}

function onSuccess() {
  loadTree(true);
}

function openAddRoot() {
  formDrawerApi.setData<CategoryFormData>({}).open();
}

function openAddChild(parentId: string) {
  formDrawerApi.setData<CategoryFormData>({ parentId }).open();
}

async function openEdit(id: string) {
  formDrawerApi.setData<CategoryFormData>({ id }).open();
}

function getNodeById(id: string): ProductCategoryTreeItem | undefined {
  return categoryNodeMap.value[id];
}

function confirmDelete(node: ProductCategoryTreeItem) {
  if (node.children?.length) {
    message.warning('请先删除子分类');
    return;
  }
  Modal.confirm({
    title: $t('product.confirmDeleteCategory'),
    onOk: async () => {
      await deleteProductCategory(node.id);
      message.success($t('common.success'));
      await loadTree(true);
    },
  });
}

onMounted(loadTree);
</script>

<template>
  <Page content-class="p-4" :description="$t('product.categoryList')" :title="$t('product.categoryList')">
    <FormDrawer @success="onSuccess" />
    <Card :bordered="true" class="border-border bg-card">
      <div class="mb-3 flex justify-between">
        <Button type="primary" class="inline-flex items-center gap-1" @click="openAddRoot">
          <Plus class="size-5 shrink-0" />
          {{ $t('product.categoryAdd') }}
        </Button>
      </div>
      <Spin :spinning="loading">
        <Tree
          v-if="treeData?.length"
          :tree-data="treeData"
          block-node
          default-expand-all
          show-line
        >
          <template #title="{ key, title }">
            <span class="group flex w-full items-center justify-between gap-2">
              <span>{{ title }}</span>
              <span class="flex shrink-0 gap-1 opacity-0 group-hover:opacity-100">
                <Button type="link" size="small" @click.stop="openEdit(key as string)">
                  {{ $t('product.categoryEdit') }}
                </Button>
                <Button type="link" size="small" @click.stop="openAddChild(key as string)">
                  {{ $t('product.categoryAddChild') }}
                </Button>
                <Button
                  type="link"
                  danger
                  size="small"
                  @click.stop="() => { const n = getNodeById(key as string); if (n) confirmDelete(n); }"
                >
                  {{ $t('product.categoryDelete') }}
                </Button>
              </span>
            </span>
          </template>
        </Tree>
        <div v-else-if="!loading" class="py-8 text-center text-muted-foreground">
          暂无分类数据
        </div>
      </Spin>
    </Card>
  </Page>
</template>
