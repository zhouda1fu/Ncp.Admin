<script lang="ts" setup>
import { computed, onMounted, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';
import { Alert, message } from 'ant-design-vue';
import type { TreeSelectProps } from 'ant-design-vue';

import { useVbenForm, z } from '#/adapter/form';
import type { RegionApi } from '#/api/system/region';
import { getRegionList } from '#/api/system/region';

import {
  getUserAudits,
  getAuthorizedRegions,
  getUserRegions,
  saveUserRegions,
} from '#/api/system/customer-sea-region';
import type { CustomerSeaRegionAssignApi } from '#/api/system/customer-sea-region';
import { $t } from '#/locales';

type RegionTreeNode = {
  label: string;
  value: string;
  children?: RegionTreeNode[];
};

const regionList = ref<RegionApi.RegionItem[]>([]);
const auditList = ref<CustomerSeaRegionAssignApi.AssignUserAuditListItem[]>([]);
const missingFromTargetRegionNames = ref<string[]>([]);
const isTargetVisible = ref<boolean>(true);

const drawerUserId = ref<string>('');

function buildRegionTree(list: RegionApi.RegionItem[]): RegionTreeNode[] {
  const nodes: Record<string, RegionTreeNode & { parentId?: string }> = {};
  for (const r of list) {
    const id = String(r.id);
    nodes[id] = { label: r.name, value: id, parentId: String(r.parentId ?? '') };
  }

  const roots: RegionTreeNode[] = [];
  for (const id of Object.keys(nodes)) {
    const node = nodes[id]!;
    const parentId = node.parentId;
    if (!parentId || parentId === '0' || !nodes[parentId]) {
      roots.push(node);
      continue;
    }

    const parentNode = nodes[parentId]!;
    parentNode.children = parentNode.children ?? [];
    parentNode.children.push(node);
  }
  return roots;
}

const regionTreeData = computed<RegionTreeNode[]>(() => buildRegionTree(regionList.value));

const regionTreeCheckProps = computed<Partial<TreeSelectProps>>(() => ({
  multiple: true,
  treeCheckable: true,
  treeData: regionTreeData.value as any,
  showSearch: true,
  fieldNames: { label: 'label', value: 'value', children: 'children' },
  treeNodeFilterProp: 'label',
  allowClear: true,
}));

const regionFormSchema = computed(() => [
  {
    component: 'TreeSelect',
    fieldName: 'selectedRegionIds',
      label: $t('customer.seaRegionAssign'),
    rules: z.any().refine((v) => Array.isArray(v), '请选择地区片区'),
    componentProps: {
      ...regionTreeCheckProps.value,
      class: 'w-full',
      placeholder: '请选择全国-省-市-区',
      // 菜单权限管理员仍应可操作；数据权限仅控制“展示片区数据”的来源
      disabled: false,
    },
  },
]);

const [Form, formApi] = useVbenForm({
  layout: 'vertical',
  schema: regionFormSchema as any,
  showDefaultActions: false,
  wrapperClass: 'grid-cols-1 gap-y-4',
});

const [Drawer, drawerApi] = useVbenDrawer({
  async onConfirm() {
    if (!isTargetVisible.value) {
      message.warning('该人员不在你的数据权限范围内，将按你的授权范围片区保存（不回显该人员原有片区）。');
    }
    const { valid } = await formApi.validate();
    if (!valid) return;

    const v = await formApi.getValues();
    const selected = Array.isArray(v.selectedRegionIds) ? v.selectedRegionIds.map(String) : [];

    drawerApi.lock();
    try {
      await saveUserRegions(drawerUserId.value, selected);
      message.success($t('common.saveSuccess'));
      drawerApi.close();
      auditList.value = [];
      emit('success');
    } finally {
      drawerApi.lock(false);
    }
  },
  onOpenChange(isOpen) {
    if (!isOpen) return;
    const data = drawerApi.getData<{ userId?: string }>();
    drawerUserId.value = data?.userId ? String(data.userId) : '';

    // 防止抽屉状态复用：打开时先重置
    isTargetVisible.value = false;
    auditList.value = [];
    missingFromTargetRegionNames.value = [];
    formApi.setValues({ selectedRegionIds: [] });

    // 并行加载回显 + 审计（如数据权限不允许，回显方法可能返回空；不影响抽屉打开）
    Promise.all([
      getUserRegions(drawerUserId.value).catch(() => []),
      getUserAudits(drawerUserId.value, { pageIndex: 1, pageSize: 20 }).catch(() => ({
        items: [],
        total: 0,
      })),
      getAuthorizedRegions(drawerUserId.value).catch(() => null),
    ]).then(([regions, audits, authorizedSummary]) => {
        isTargetVisible.value = authorizedSummary?.isTargetVisible ?? true;

        // 默认勾选：只使用“授权范围并集”
        // 目的：避免在目标人员不在数据权限范围内时，错误回填/勾选其原有片区（getUserRegions）。
        const selectedRegionIds = (authorizedSummary?.authorizedRegionIds?.length
            ? authorizedSummary?.authorizedRegionIds
            : [])
          .map(String);
        formApi.setValues({
          selectedRegionIds,
        });
        auditList.value = audits?.items ?? [];
        missingFromTargetRegionNames.value =
          authorizedSummary?.missingFromTargetRegionNames ?? [];
      })
      .catch(() => {
        // 若整体加载失败，仍保持“不可见/空选择”的安全状态
        isTargetVisible.value = false;
        message.error($t('ui.actionMessage.loadFailed'));
      });
  },
});

const diffAlertText = computed(() => {
  if (!isTargetVisible.value) {
    return '提示：该人员不在你的数据权限范围内，将按授权范围片区展示（不回显该人员原有片区）。';
  }
  if ((missingFromTargetRegionNames.value ?? []).length > 0) {
    const n = missingFromTargetRegionNames.value.length;
    return `提示：授权范围内其他人员已分配片区中，有 ${n} 个片区当前人员未包含。你保存后当前人员片区将发生新增/变动。`;
  }
  return '提示：授权范围内其他人员的片区与当前人员当前一致。';
});

const emit = defineEmits(['success']);

function open(userId: string) {
  drawerApi.setData({ userId }).open();
}

defineExpose({ open });

onMounted(async () => {
  try {
    regionList.value = await getRegionList();
  } catch {
    // ignore, opened 时仍可报错提示
  }
});

function formatAuditRegions(items: string[]) {
  return items?.length ? items.join('、') : '-';
}
</script>

<template>
  <Drawer :title="$t('customer.seaRegionAssign')">
    <div class="mx-4 mb-3">
      <Alert
        :message="diffAlertText"
        type="info"
        show-icon
      />
    </div>
    <Form class="mx-4" />

    <div class="mt-4 mx-4">
      <div class="mb-2 text-sm font-medium text-foreground">
        {{ $t('customer.seaRegionAssign') }} {{ '修改记录' }}
      </div>
      <div v-if="auditList.length === 0" class="text-muted-foreground py-3">
        暂无记录
      </div>
      <div v-else class="flex flex-col gap-3">
        <div
          v-for="a in auditList"
          :key="a.id"
          class="rounded-md border border-border bg-card p-3"
        >
          <div class="mb-1 text-xs text-muted-foreground">
            {{ a.operatorUserName }} / {{ new Date(a.createdAt).toLocaleString('zh-CN') }}
          </div>
          <div class="text-sm">
            <div class="mb-1">
              <span class="text-muted-foreground">新增：</span>
              <span>{{ formatAuditRegions(a.addedRegionNames) }}</span>
            </div>
            <div>
              <span class="text-muted-foreground">删除：</span>
              <span>{{ formatAuditRegions(a.removedRegionNames) }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </Drawer>
</template>

