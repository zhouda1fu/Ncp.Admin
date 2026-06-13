<script lang="ts" setup>
import type { PermissionTreeNode } from '#/utils/permission-tree';

import { computed, ref } from 'vue';

import { Tree } from '@vben/common-ui';
import { IconifyIcon } from '@vben/icons';

import { Input } from 'ant-design-vue';

import {
  isSyntheticPermissionTreeKey,
  stripSyntheticPermissionTreeKeys,
} from '#/constants/module-menu-categories';
import { enrichPermissionTreeSelection, expandLegacyPermissionSelection } from '#/utils/permission-tree';
import { $t } from '#/locales';

const props = defineProps<{
  permissions: PermissionTreeNode[];
  modelValue?: string[];
}>();

const emit = defineEmits<{
  'update:modelValue': [string[]];
}>();

const keyword = ref('');

const selectedCount = computed(() => (props.modelValue ?? []).length);
const selectedCodeSet = computed(
  () => new Set(expandLegacyPermissionSelection(props.modelValue ?? [])),
);

const filteredPermissions = computed(() => {
  const text = keyword.value.trim().toLowerCase();
  if (!text) return props.permissions;
  return filterTree(props.permissions, text);
});

const treeData = computed(() => filteredPermissions.value);

const displayModelValue = computed(() =>
  enrichPermissionTreeSelection(
    expandLegacyPermissionSelection(props.modelValue ?? []),
    props.permissions,
  ),
);

const permissionNodeStats = computed(() => {
  const map = new Map<string, { selected: number; total: number }>();
    const visit = (node: PermissionTreeNode): { selected: number; total: number } => {
      const children = node.children ?? [];
      const childStats = children.map(visit);
      const isSynthetic = isSyntheticPermissionTreeKey(node.value);
      const hasChildren = children.length > 0;
      const selfTotal = isSynthetic || hasChildren ? 0 : 1;
      const selfSelected = !isSynthetic && !hasChildren && selectedCodeSet.value.has(node.value) ? 1 : 0;
    const total = selfTotal + childStats.reduce((sum, item) => sum + item.total, 0);
    const selected = selfSelected + childStats.reduce((sum, item) => sum + item.selected, 0);
    map.set(node.value, { selected, total });
    return { selected, total };
  };
  props.permissions.forEach(visit);
  return map;
});

function getNodeStats(node: PermissionTreeNode) {
  const stats = permissionNodeStats.value.get(node.value);
  if (!stats || stats.total <= 1) return undefined;
  return stats;
}

function getNodeHint(node: PermissionTreeNode) {
  return node.hint;
}

function filterTree(nodes: PermissionTreeNode[], text: string): PermissionTreeNode[] {
  const result: PermissionTreeNode[] = [];
  for (const node of nodes) {
    const label = String(node.label ?? '').toLowerCase();
    const value = String(node.value ?? '').toLowerCase();
    const children = node.children ? filterTree(node.children, text) : [];
    const matched = label.includes(text) || value.includes(text);
    if (matched || children.length > 0) {
      result.push({
        ...node,
        children: matched ? node.children : children,
      });
    }
  }
  return result;
}

function isSameCodes(a: string[], b: string[]) {
  if (a.length !== b.length) return false;
  const set = new Set(a);
  return b.every((code) => set.has(code));
}

function onUpdate(next: string[]) {
  const prev = props.modelValue ?? [];
  const reconciled = stripSyntheticPermissionTreeKeys(next);
  if (isSameCodes(reconciled, prev)) return;
  emit('update:modelValue', reconciled);
}

function getPermissionNodeClass(item: any) {
  return item.value?.nodeType === 'group' ? 'permission-tree-page-group' : '';
}
</script>

<template>
  <div class="flex w-full flex-col gap-2">
    <div class="flex flex-wrap items-center gap-2">
      <Input
        v-model:value="keyword"
        allow-clear
        class="max-w-md flex-1"
        :placeholder="$t('system.role.permissionSearchPlaceholder')"
      />
      <span class="text-muted-foreground text-sm">
        {{ $t('system.role.selectedPermissionCount', [selectedCount]) }}
      </span>
    </div>
    <Tree
      :model-value="displayModelValue"
      :tree-data="treeData"
      :default-expanded-level="keyword ? 8 : 0"
      :get-node-class="getPermissionNodeClass"
      class="permission-codes-tree max-h-[60vh] overflow-auto"
      multiple
      bordered
      value-field="value"
      label-field="label"
      icon-field="icon"
      @update:model-value="onUpdate"
    >
      <template #node="{ value }">
        <span class="permission-node-content">
          <IconifyIcon v-if="value.icon" class="permission-node-icon" :icon="value.icon" />
          <span class="permission-node-main">
            <span class="permission-node-label">{{ value.label }}</span>
            <span v-if="getNodeHint(value)" class="permission-node-hint">
              {{ getNodeHint(value) }}
            </span>
          </span>
          <span
            v-if="getNodeStats(value)"
            class="permission-node-count"
            :class="{ 'is-empty': getNodeStats(value)?.selected === 0 }"
          >
            {{ getNodeStats(value)?.selected }}/{{ getNodeStats(value)?.total }}
          </span>
        </span>
      </template>
    </Tree>
  </div>
</template>

<style scoped>
:deep(.permission-codes-tree) {
  background: hsl(var(--background));
}

:deep(.permission-tree-page-group) {
  background: hsl(var(--muted) / 45%);
}

:deep(.permission-tree-page-group .permission-node-label) {
  font-weight: 600;
}

.permission-node-content {
  display: inline-flex;
  min-width: 0;
  align-items: center;
  gap: 4px;
}

.permission-node-icon {
  width: 16px;
  height: 16px;
  flex: none;
}

.permission-node-main {
  display: inline-flex;
  min-width: 0;
  align-items: center;
  gap: 6px;
}

.permission-node-label {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.permission-node-hint {
  max-width: 18rem;
  overflow: hidden;
  color: hsl(var(--muted-foreground));
  font-size: 11px;
  line-height: 1.2;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.permission-node-count {
  flex: none;
  border-radius: 999px;
  background: hsl(var(--primary) / 12%);
  color: hsl(var(--primary));
  font-size: 11px;
  font-weight: 500;
  line-height: 16px;
  padding: 0 6px;
}

.permission-node-count.is-empty {
  background: hsl(var(--muted));
  color: hsl(var(--muted-foreground));
}
</style>
