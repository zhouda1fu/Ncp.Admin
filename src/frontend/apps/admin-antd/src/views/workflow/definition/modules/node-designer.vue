<script lang="ts" setup>
import type { WorkflowApi } from '#/api/system/workflow';

import { computed, ref, watch } from 'vue';

import {
  Button,
  Card,
  Divider,
  Drawer,
  Dropdown,
  Empty,
  Form,
  Input,
  Menu,
  Popconfirm,
  Select,
  Space,
  Tag,
  Tooltip,
} from 'ant-design-vue';

import { getRoleList } from '#/api/system/role';
import { getUserList } from '#/api/system/user';
import { $t } from '#/locales';

// Node types: 1=Approval, 2=CarbonCopy, 3=Notification, 4=Condition. Parallel(5) not in backend, UI only as disabled.
const props = defineProps<{
  modelValue: WorkflowApi.WorkflowNode[];
  disabled?: boolean;
}>();

const emit = defineEmits<{
  'update:modelValue': [nodes: WorkflowApi.WorkflowNode[]];
}>();

const approvalModeOptions = computed(() => [
  { label: $t('system.workflow.node.approvalModeOrSign'), value: 0 },
  { label: $t('system.workflow.node.approvalModeCounterSign'), value: 1 },
  { label: $t('system.workflow.node.approvalModeSequential'), value: 2 },
]);

const assigneeTypeOptions = computed(() => [
  { label: $t('system.workflow.node.assigneeUser'), value: 0 },
  { label: $t('system.workflow.node.assigneeRole'), value: 1 },
]);

const userOptions = ref<Array<{ label: string; value: string }>>([]);
const roleOptions = ref<Array<{ label: string; value: string }>>([]);
const loadingUsers = ref(false);
const loadingRoles = ref(false);

async function loadUsers() {
  if (userOptions.value.length > 0) return;
  loadingUsers.value = true;
  try {
    const result = await getUserList({
      pageIndex: 1,
      pageSize: 1000,
      countTotal: false,
    });
    userOptions.value = result.items.map((u) => ({
      label: u.realName || u.name,
      value: u.userId,
    }));
  } finally {
    loadingUsers.value = false;
  }
}

async function loadRoles() {
  if (roleOptions.value.length > 0) return;
  loadingRoles.value = true;
  try {
    const result = await getRoleList({
      pageIndex: 1,
      pageSize: 1000,
      countTotal: false,
    });
    roleOptions.value = result.items.map((r) => ({
      label: r.name,
      value: r.roleId,
    }));
  } finally {
    loadingRoles.value = false;
  }
}

const nodes = ref<WorkflowApi.WorkflowNode[]>([]);

watch(
  () => props.modelValue,
  (val) => {
    if (val) {
      nodes.value = val.map((n, idx) => ({ ...n, sortOrder: idx + 1 }));
    }
  },
  { immediate: true, deep: true },
);

function emitUpdate() {
  emit(
    'update:modelValue',
    nodes.value.map((n, idx) => ({ ...n, sortOrder: idx + 1 })),
  );
}

function getNodeKey(node: WorkflowApi.WorkflowNode, index: number) {
  return node.id ?? `node-${index}-${node.sortOrder}-${node.nodeName}`;
}

function addNode(insertIndex: number | undefined, nodeType: number) {
  if (nodeType === 5) return; // Parallel: coming soon, no backend support
  const newNode: WorkflowApi.WorkflowNode = {
    nodeName: '',
    nodeType,
    assigneeType: 0,
    assigneeValue: '',
    sortOrder: 0,
    description: '',
    approvalMode: 0,
    ...(nodeType === 4
      ? { conditionExpression: '', trueNextNodeName: '', falseNextNodeName: '' }
      : {}),
  };
  if (insertIndex !== undefined && insertIndex >= 0) {
    nodes.value.splice(insertIndex + 1, 0, newNode);
  } else if (insertIndex === -1) {
    nodes.value.splice(0, 0, newNode);
  } else {
    nodes.value.push(newNode);
  }
  emitUpdate();
}

function removeNode(index: number) {
  nodes.value.splice(index, 1);
  emitUpdate();
}

function moveUp(index: number) {
  if (index <= 0) return;
  const temp = nodes.value[index]!;
  nodes.value[index] = nodes.value[index - 1]!;
  nodes.value[index - 1] = temp;
  emitUpdate();
}

function moveDown(index: number) {
  if (index >= nodes.value.length - 1) return;
  const temp = nodes.value[index]!;
  nodes.value[index] = nodes.value[index + 1]!;
  nodes.value[index + 1] = temp;
  emitUpdate();
}

function copyNode(index: number) {
  const src = nodes.value[index];
  if (!src) return;
  const suffix = $t('system.workflow.node.copySuffix');
  const newNode: WorkflowApi.WorkflowNode = {
    ...JSON.parse(JSON.stringify(src)),
    nodeName: src.nodeName ? `${src.nodeName} ${suffix}` : '',
    sortOrder: 0,
  };
  nodes.value.splice(index + 1, 0, newNode);
  emitUpdate();
}

function updateField(
  index: number,
  field: keyof WorkflowApi.WorkflowNode,
  value: any,
) {
  const node = nodes.value[index];
  if (node) {
    (node as any)[field] = value;
    if (field === 'assigneeType') {
      node.assigneeValue = '';
    }
    emitUpdate();
  }
}

const nodeTypeConfig: Record<
  number,
  { color: string; bg: string; icon: string }
> = {
  1: { color: '#1677ff', bg: '#e6f4ff', icon: '✓' },
  2: { color: '#52c41a', bg: '#f6ffed', icon: '📋' },
  3: { color: '#faad14', bg: '#fffbe6', icon: '🔔' },
  4: { color: '#722ed1', bg: '#f9f0ff', icon: '◇' },
  5: { color: '#8c8c8c', bg: '#f5f5f5', icon: '∥' }, // Parallel (placeholder)
};

function getTypeConfig(type: number) {
  return nodeTypeConfig[type] ?? { color: '#999', bg: '#fafafa', icon: '•' };
}

function getNodeTypeLabel(type: number) {
  switch (type) {
    case 1:
      return $t('system.workflow.node.typeApproval');
    case 2:
      return $t('system.workflow.node.typeCarbonCopy');
    case 3:
      return $t('system.workflow.node.typeNotification');
    case 4:
      return $t('system.workflow.node.typeCondition');
    case 5:
      return $t('system.workflow.node.typeParallel');
    default:
      return '';
  }
}

function getAssigneeDisplayText(node: WorkflowApi.WorkflowNode): string {
  if (node.nodeType === 4) return ''; // Condition has no assignee
  if (!node.assigneeValue) return '-';
  if (node.assigneeType === 0) {
    const u = userOptions.value.find((o) => o.value === node.assigneeValue);
    return u?.label ?? node.assigneeValue;
  }
  if (node.assigneeType === 1) {
    const r = roleOptions.value.find((o) => o.value === node.assigneeValue);
    return r?.label ?? node.assigneeValue;
  }
  return node.assigneeValue;
}

const drawerVisible = ref(false);
const editingIndex = ref<number | null>(null);

const editingNode = computed(() =>
  editingIndex.value !== null ? nodes.value[editingIndex.value] ?? null : null,
);

function openDrawer(index: number) {
  if (props.disabled) return;
  editingIndex.value = index;
  drawerVisible.value = true;
  if (nodes.value[index]?.assigneeType === 0) loadUsers();
  if (nodes.value[index]?.assigneeType === 1) loadRoles();
}

function closeDrawer() {
  drawerVisible.value = false;
  editingIndex.value = null;
}

function getAssigneeOptions(assigneeType: number) {
  if (assigneeType === 0) return userOptions.value;
  if (assigneeType === 1) return roleOptions.value;
  return [];
}

function isAssigneeLoading(assigneeType: number) {
  if (assigneeType === 0) return loadingUsers.value;
  if (assigneeType === 1) return loadingRoles.value;
  return false;
}

function onAssigneeDropdownOpen(assigneeType: number) {
  if (assigneeType === 0) loadUsers();
  if (assigneeType === 1) loadRoles();
}

const addInsertIndex = ref<number | undefined>(undefined);

const addMenuItems = computed(() => [
  { key: '1', label: $t('system.workflow.node.typeApproval') },
  { key: '2', label: $t('system.workflow.node.typeCarbonCopy') },
  { key: '3', label: $t('system.workflow.node.typeNotification') },
  { key: '4', label: $t('system.workflow.node.typeCondition') },
  {
    key: '5',
    disabled: true,
    label: `${$t('system.workflow.node.typeParallel')}（${$t('system.workflow.node.comingSoon')}）`,
  },
]);

function handleAddMenuItem(key: string) {
  const type = Number(key);
  if (type === 5) return;
  const idx = addInsertIndex.value;
  addNode(idx === undefined ? undefined : idx, type);
  addInsertIndex.value = undefined;
}

function setAddInsertIndex(i: number | undefined) {
  addInsertIndex.value = i;
}

const nextNodeNameOptions = computed(() =>
  nodes.value
    .filter((n) => n.nodeName)
    .map((n) => ({ label: n.nodeName!, value: n.nodeName! })),
);
</script>

<template>
  <div class="nd-wrapper">
    <div class="nd">
      <div class="nd-terminal">
        <div class="nd-terminal-dot nd-terminal-start"><span>▶</span></div>
        <span class="nd-terminal-text">{{ $t('system.workflow.node.start') }}</span>
      </div>
      <div class="nd-line" />

      <template v-if="nodes.length === 0">
        <div class="nd-empty">
          <Empty :description="$t('system.workflow.node.empty')" :image-style="{ height: '48px' }">
            <Dropdown v-if="!disabled" :trigger="['click']" @open-change="(open: boolean) => open && setAddInsertIndex(undefined)">
              <Button type="primary" size="small">
                + {{ $t('system.workflow.node.add') }}
              </Button>
              <template #overlay>
                <Menu :items="addMenuItems" @click="({ key }) => handleAddMenuItem(String(key))" />
              </template>
            </Dropdown>
          </Empty>
        </div>
        <div class="nd-line" />
      </template>

      <template v-for="(node, index) in nodes" :key="getNodeKey(node, index)">
        <div v-if="!disabled" class="nd-add-wrapper">
          <Dropdown :trigger="['click']" @open-change="(open: boolean) => open && setAddInsertIndex(index - 1)">
            <Tooltip :title="$t('system.workflow.node.addAfter')">
              <button type="button" class="nd-add-btn">
                <span>+</span>
              </button>
            </Tooltip>
            <template #overlay>
              <Menu :items="addMenuItems" @click="({ key }) => handleAddMenuItem(String(key))" />
            </template>
          </Dropdown>
        </div>
        <div v-if="!disabled" class="nd-line nd-line-short" />

        <Card class="nd-card" :bordered="false" :body-style="{ padding: '12px 16px' }">
          <div
            class="nd-card-accent"
            :style="{ backgroundColor: getTypeConfig(node.nodeType).color }"
          />
          <div class="nd-card-row">
            <div class="nd-card-left">
              <div
                class="nd-card-icon"
                :style="{
                  backgroundColor: getTypeConfig(node.nodeType).bg,
                  color: getTypeConfig(node.nodeType).color,
                }"
              >
                {{ getTypeConfig(node.nodeType).icon }}
              </div>
              <div class="nd-card-body">
                <div class="nd-card-name">
                  {{ node.nodeName || $t('system.workflow.node.namePlaceholder') }}
                </div>
                <div v-if="node.nodeType !== 4" class="nd-card-assignee">
                  {{ getAssigneeDisplayText(node) }}
                </div>
              </div>
            </div>
            <div v-if="!disabled" class="nd-card-right">
              <Tooltip :title="$t('system.workflow.node.configNode')">
                <Button type="text" size="small" @click="openDrawer(index)">
                  ⚙
                </Button>
              </Tooltip>
              <Tooltip :title="$t('system.workflow.node.copyNode')">
                <Button type="text" size="small" @click="copyNode(index)">📋</Button>
              </Tooltip>
              <Tooltip :title="$t('system.workflow.node.moveUp')">
                <Button
                  type="text"
                  size="small"
                  :disabled="index === 0"
                  @click="moveUp(index)"
                >
                  ↑
                </Button>
              </Tooltip>
              <Tooltip :title="$t('system.workflow.node.moveDown')">
                <Button
                  type="text"
                  size="small"
                  :disabled="index === nodes.length - 1"
                  @click="moveDown(index)"
                >
                  ↓
                </Button>
              </Tooltip>
              <Popconfirm
                :title="$t('system.workflow.node.delete') + '?'"
                placement="left"
                @confirm="removeNode(index)"
              >
                <Button type="text" size="small" danger>✕</Button>
              </Popconfirm>
            </div>
          </div>
          <Tag
            :color="getTypeConfig(node.nodeType).color"
            :bordered="false"
            class="nd-card-tag"
          >
            {{ getNodeTypeLabel(node.nodeType) }}
          </Tag>
        </Card>

        <div class="nd-line" />
      </template>

      <div v-if="!disabled && nodes.length > 0" class="nd-add-wrapper">
        <Dropdown :trigger="['click']" @open-change="(open: boolean) => open && setAddInsertIndex(undefined)">
          <Tooltip :title="$t('system.workflow.node.add')">
            <button type="button" class="nd-add-btn">
              <span>+</span>
            </button>
          </Tooltip>
          <template #overlay>
            <Menu :items="addMenuItems" @click="({ key }) => handleAddMenuItem(String(key))" />
          </template>
        </Dropdown>
      </div>
      <div v-if="!disabled && nodes.length > 0" class="nd-line nd-line-short" />

      <div class="nd-terminal">
        <div class="nd-terminal-dot nd-terminal-end"><span>■</span></div>
        <span class="nd-terminal-text">{{ $t('system.workflow.node.end') }}</span>
      </div>
    </div>
  </div>

  <Drawer
    v-model:open="drawerVisible"
    :title="$t('system.workflow.node.configDrawerTitle')"
    width="420"
    :body-style="{ paddingBottom: '24px' }"
    @close="closeDrawer"
  >
    <template v-if="editingNode && editingIndex !== null">
      <Form
        layout="vertical"
        class="nd-drawer-form"
        :label-col="{ style: { fontWeight: 500 } }"
      >
        <Form.Item :label="$t('system.workflow.node.name')">
          <Input
            :value="editingNode.nodeName"
            :disabled="disabled"
            :placeholder="$t('system.workflow.node.namePlaceholder')"
            @update:value="(v: string) => updateField(editingIndex!, 'nodeName', v)"
          />
        </Form.Item>
        <Form.Item :label="$t('system.workflow.node.type')">
          <div class="nd-drawer-readonly">{{ getNodeTypeLabel(editingNode.nodeType) }}</div>
        </Form.Item>
        <template v-if="editingNode.nodeType === 4">
          <Divider orientation="left" class="nd-drawer-divider">
            {{ $t('system.workflow.node.typeCondition') }}
            </Divider>
          <Form.Item :label="$t('system.workflow.node.conditionExpression')">
            <Input
              :value="editingNode.conditionExpression ?? ''"
              :disabled="disabled"
              :placeholder="$t('system.workflow.node.conditionExpressionPlaceholder')"
              @update:value="(v: string) => updateField(editingIndex!, 'conditionExpression', v)"
            />
          </Form.Item>
          <Form.Item :label="$t('system.workflow.node.trueNextNodeName')">
            <Select
              :value="editingNode.trueNextNodeName || undefined"
              :disabled="disabled"
              :options="nextNodeNameOptions"
              :placeholder="$t('system.workflow.node.nextNodePlaceholder')"
              allow-clear
              show-search
              class="w-full"
              @update:value="(v: any) => updateField(editingIndex!, 'trueNextNodeName', v ?? '')"
            />
          </Form.Item>
          <Form.Item :label="$t('system.workflow.node.falseNextNodeName')">
            <Select
              :value="editingNode.falseNextNodeName || undefined"
              :disabled="disabled"
              :options="nextNodeNameOptions"
              :placeholder="$t('system.workflow.node.nextNodePlaceholder')"
              allow-clear
              show-search
              class="w-full"
              @update:value="(v: any) => updateField(editingIndex!, 'falseNextNodeName', v ?? '')"
            />
          </Form.Item>
        </template>
        <template v-if="editingNode.nodeType !== 4">
          <Form.Item :label="$t('system.workflow.node.assigneeType')">
            <Select
              :value="editingNode.assigneeType"
              :disabled="disabled"
              :options="assigneeTypeOptions"
              class="w-full"
              @update:value="(v: any) => updateField(editingIndex!, 'assigneeType', v)"
            />
          </Form.Item>
          <Form.Item :label="$t('system.workflow.node.assignee')">
            <Select
              :value="editingNode.assigneeValue || undefined"
              :disabled="disabled"
              :loading="isAssigneeLoading(editingNode.assigneeType)"
              :options="getAssigneeOptions(editingNode.assigneeType)"
              :placeholder="$t('system.workflow.node.assigneePlaceholder')"
              show-search
              class="w-full"
              :filter-option="
                (input: string, option: any) =>
                  option.label.toLowerCase().includes(input.toLowerCase())
              "
              @update:value="(v: any) => updateField(editingIndex!, 'assigneeValue', v)"
              @dropdown-visible-change="
                (open: boolean) =>
                  open && editingNode && onAssigneeDropdownOpen(editingNode.assigneeType)
              "
            />
          </Form.Item>
        </template>
        <Form.Item
          v-if="editingNode.nodeType === 1"
          :label="$t('system.workflow.node.approvalMode')"
        >
          <Select
            :value="editingNode.approvalMode ?? 0"
            :disabled="disabled"
            :options="approvalModeOptions"
            class="w-full"
            @update:value="(v: any) => updateField(editingIndex!, 'approvalMode', v)"
          />
        </Form.Item>
        <Form.Item :label="$t('system.workflow.node.description')">
          <Input
            :value="editingNode.description"
            :disabled="disabled"
            :placeholder="$t('system.workflow.node.descriptionPlaceholder')"
            @update:value="(v: string) => updateField(editingIndex!, 'description', v)"
          />
        </Form.Item>
      </Form>
    </template>
    <template #footer>
      <Space>
        <Button @click="closeDrawer">{{ $t('system.workflow.definition.cancel') }}</Button>
        <Button type="primary" @click="closeDrawer">{{ $t('common.confirm') }}</Button>
      </Space>
    </template>
  </Drawer>
</template>

<style scoped>
/* 使用继承与 currentColor，适配明暗主题 */
.nd-wrapper {
  max-height: 560px;
  overflow-y: auto;
  overflow-x: hidden;
  padding: 4px 8px;
  margin: 0 -8px;
  color: inherit;
}

.nd {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 20px 0;
  min-height: 200px;
  color: inherit;
}

.nd-terminal {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 6px;
}

.nd-terminal-dot {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  border-radius: 50%;
  color: #fff;
  font-size: 13px;
  box-shadow: 0 2px 8px rgb(0 0 0 / 15%);
}

.nd-terminal-start {
  background: linear-gradient(135deg, #52c41a, #73d13d);
}

.nd-terminal-end {
  background: linear-gradient(135deg, #ff4d4f, #ff7875);
}

.nd-terminal-text {
  font-size: 12px;
  font-weight: 500;
  color: inherit;
  opacity: 0.7;
}

.nd-line {
  width: 2px;
  height: 28px;
  background: currentColor;
  opacity: 0.12;
}

.nd-line-short {
  height: 14px;
}

.nd-add-wrapper {
  display: flex;
  justify-content: center;
  padding: 2px 0;
  color: inherit;
}

.nd-add-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 28px;
  height: 28px;
  border-radius: 50%;
  border: 2px dashed currentColor;
  background: inherit;
  color: inherit;
  opacity: 0.5;
  font-size: 18px;
  cursor: pointer;
  transition: all 0.25s;
}

.nd-add-btn:hover {
  border-color: var(--ant-color-primary, #1677ff);
  color: var(--ant-color-primary, #1677ff);
  opacity: 1;
  background: var(--ant-color-primary-bg, rgba(22, 119, 255, 0.08));
}

.nd-card {
  width: 100%;
  max-width: 440px;
  border-radius: 10px;
  overflow: hidden;
  position: relative;
  box-shadow: 0 2px 12px rgb(0 0 0 / 6%);
}

.nd-card-accent {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 3px;
}

.nd-card-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
}

.nd-card-left {
  display: flex;
  align-items: center;
  gap: 10px;
  min-width: 0;
  flex: 1;
}

.nd-card-icon {
  width: 36px;
  height: 36px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 16px;
  flex-shrink: 0;
}

.nd-card-body {
  min-width: 0;
}

.nd-card-name {
  font-size: 14px;
  font-weight: 600;
  color: inherit;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.nd-card-assignee {
  font-size: 12px;
  margin-top: 2px;
  color: inherit;
  opacity: 0.7;
}

.nd-card-tag {
  font-size: 11px;
  margin-top: 6px;
  margin-bottom: 0;
}

.nd-card-right {
  display: flex;
  gap: 0;
  flex-shrink: 0;
  opacity: 0.7;
}

.nd-card:hover .nd-card-right {
  opacity: 1;
}

.nd-empty {
  padding: 28px 0;
}

/* 节点配置抽屉：使用 Ant Design Form 统一风格 */
.nd-drawer-form {
  padding-top: 8px;
}

.nd-drawer-form :deep(.ant-form-item) {
  margin-bottom: 18px;
}

.nd-drawer-form :deep(.ant-form-item:last-child) {
  margin-bottom: 0;
}

.nd-drawer-readonly {
  padding: 4px 0;
  font-size: 14px;
  color: inherit;
  opacity: 0.85;
}

.nd-drawer-divider {
  margin: 16px 0 12px;
  font-size: 13px;
  font-weight: 500;
}

.nd-drawer-divider :deep(.ant-divider-inner-text) {
  padding-right: 8px;
}
</style>
