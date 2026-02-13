<script lang="ts" setup>
import type { WorkflowApi } from '#/api/system/workflow';

import { computed, ref, watch } from 'vue';

import {
  Button,
  Card,
  Empty,
  Input,
  Popconfirm,
  Select,
  Tag,
  Tooltip,
} from 'ant-design-vue';

import { getRoleList } from '#/api/system/role';
import { getUserList } from '#/api/system/user';
import { $t } from '#/locales';

const props = defineProps<{
  modelValue: WorkflowApi.WorkflowNode[];
  disabled?: boolean;
}>();

const emit = defineEmits<{
  'update:modelValue': [nodes: WorkflowApi.WorkflowNode[]];
}>();

// 节点类型选项
const nodeTypeOptions = computed(() => [
  { label: $t('system.workflow.node.typeApproval'), value: 1 },
  { label: $t('system.workflow.node.typeCarbonCopy'), value: 2 },
  { label: $t('system.workflow.node.typeNotification'), value: 3 },
]);

// 审批方式选项（仅审批节点有效）：0=或签 1=会签 2=依次审批
const approvalModeOptions = computed(() => [
  { label: $t('system.workflow.node.approvalModeOrSign'), value: 0 },
  { label: $t('system.workflow.node.approvalModeCounterSign'), value: 1 },
  { label: $t('system.workflow.node.approvalModeSequential'), value: 2 },
]);

// 审批人类型选项（指定用户、指定角色；部门主管与发起人自选暂不支持）
const assigneeTypeOptions = computed(() => [
  { label: $t('system.workflow.node.assigneeUser'), value: 0 },
  { label: $t('system.workflow.node.assigneeRole'), value: 1 },
]);

// 用户列表和角色列表缓存
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

// 内部节点列表
const nodes = ref<WorkflowApi.WorkflowNode[]>([]);

// 节点折叠状态（按 index 存储）
const collapsedMap = ref<Record<number, boolean>>({});

function getNodeKey(node: WorkflowApi.WorkflowNode, index: number) {
  return node.id ?? `node-${index}-${node.sortOrder}-${node.nodeName}`;
}

function toggleCollapsed(index: number) {
  if (props.disabled) return;
  collapsedMap.value[index] = !collapsedMap.value[index];
}

function isCollapsed(index: number) {
  return collapsedMap.value[index] ?? false;
}

watch(
  () => props.modelValue,
  (val) => {
    if (val) {
      nodes.value = val.map((n, idx) => ({ ...n, sortOrder: idx + 1 }));
      collapsedMap.value = {};
    }
  },
  { immediate: true, deep: true },
);

function emitUpdate() {
  const updated = nodes.value.map((n, idx) => ({
    ...n,
    sortOrder: idx + 1,
  }));
  emit('update:modelValue', updated);
}

function addNode(index?: number) {
  const newNode: WorkflowApi.WorkflowNode = {
    nodeName: '',
    nodeType: 1,
    assigneeType: 0,
    assigneeValue: '',
    sortOrder: 0,
    description: '',
    approvalMode: 0,
  };
  if (index !== undefined) {
    nodes.value.splice(index + 1, 0, newNode);
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

// 节点类型配置
const nodeTypeConfig: Record<
  number,
  { color: string; bg: string; icon: string }
> = {
  1: { color: '#1677ff', bg: '#e6f4ff', icon: '✓' },
  2: { color: '#52c41a', bg: '#f6ffed', icon: '📋' },
  3: { color: '#faad14', bg: '#fffbe6', icon: '🔔' },
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
    default:
      return '';
  }
}

function needsAssigneeSelect(assigneeType: number) {
  return assigneeType === 0 || assigneeType === 1;
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
</script>

<template>
  <div class="nd-wrapper">
  <div class="nd">
    <!-- ========== 开始节点 ========== -->
    <div class="nd-terminal">
      <div class="nd-terminal-dot nd-terminal-start">
        <span>▶</span>
      </div>
      <span class="nd-terminal-text">{{ $t('system.workflow.node.start') }}</span>
    </div>

    <div class="nd-line" />

    <!-- ========== 空状态 ========== -->
    <template v-if="nodes.length === 0">
      <div class="nd-empty">
        <Empty :description="$t('system.workflow.node.empty')" :image-style="{ height: '48px' }">
          <Button v-if="!disabled" type="primary" size="small" @click="addNode()">
            + {{ $t('system.workflow.node.add') }}
          </Button>
        </Empty>
      </div>
      <div class="nd-line" />
    </template>

    <!-- ========== 节点列表 ========== -->
    <template v-for="(node, index) in nodes" :key="getNodeKey(node, index)">
      <!-- 添加按钮（节点上方） -->
      <div v-if="!disabled" class="nd-add-wrapper">
        <Tooltip :title="$t('system.workflow.node.addAfter')">
          <button class="nd-add-btn" @click="addNode(index - 1)">
            <span>+</span>
          </button>
        </Tooltip>
      </div>
      <div v-if="!disabled" class="nd-line nd-line-short" />

      <Card
        class="nd-card"
        :bordered="false"
        :body-style="{ padding: '16px' }"
      >
        <!-- 卡片顶部色条 -->
        <div
          class="nd-card-accent"
          :style="{ backgroundColor: getTypeConfig(node.nodeType).color }"
        />

        <!-- 头部（可点击折叠） -->
        <div
          class="nd-card-head"
          :class="{ 'nd-card-head-collapsed': isCollapsed(index) }"
          @click="toggleCollapsed(index)"
        >
          <div class="nd-card-title">
            <div
              class="nd-card-icon"
              :style="{
                backgroundColor: getTypeConfig(node.nodeType).bg,
                color: getTypeConfig(node.nodeType).color,
              }"
            >
              {{ getTypeConfig(node.nodeType).icon }}
            </div>
            <div>
              <div class="nd-card-name">
                {{ node.nodeName || $t('system.workflow.node.namePlaceholder') }}
              </div>
              <Tag
                :color="getTypeConfig(node.nodeType).color"
                :bordered="false"
                style="font-size: 11px; margin: 0; line-height: 18px"
              >
                {{ getNodeTypeLabel(node.nodeType) }}
              </Tag>
            </div>
            <span v-if="!disabled" class="nd-collapse-icon">
              {{ isCollapsed(index) ? '▶' : '▼' }}
            </span>
          </div>
          <div v-if="!disabled" class="nd-card-actions" @click.stop>
            <Tooltip :title="$t('system.workflow.node.copyNode')">
              <Button type="text" size="small" @click="copyNode(index)">
                <span style="font-size: 14px">📋</span>
              </Button>
            </Tooltip>
            <Tooltip :title="$t('system.workflow.node.moveUp')">
              <Button type="text" size="small" :disabled="index === 0" @click="moveUp(index)">
                <span style="font-size: 16px">↑</span>
              </Button>
            </Tooltip>
            <Tooltip :title="$t('system.workflow.node.moveDown')">
              <Button
                type="text"
                size="small"
                :disabled="index === nodes.length - 1"
                @click="moveDown(index)"
              >
                <span style="font-size: 16px">↓</span>
              </Button>
            </Tooltip>
            <Popconfirm
              :title="$t('system.workflow.node.delete') + '?'"
              placement="left"
              @confirm="removeNode(index)"
            >
              <Button type="text" size="small" danger>
                <span style="font-size: 14px">✕</span>
              </Button>
            </Popconfirm>
          </div>
        </div>

        <!-- 表单（可折叠） -->
        <div v-show="!isCollapsed(index)" class="nd-card-form">
          <div class="nd-field">
            <label>{{ $t('system.workflow.node.name') }}</label>
            <Input
              :value="node.nodeName"
              :disabled="disabled"
              size="small"
              :placeholder="$t('system.workflow.node.namePlaceholder')"
              @update:value="(v: string) => updateField(index, 'nodeName', v)"
            />
          </div>

          <div class="nd-field-row">
            <div class="nd-field nd-field-half">
              <label>{{ $t('system.workflow.node.type') }}</label>
              <Select
                :value="node.nodeType"
                :disabled="disabled"
                :options="nodeTypeOptions"
                size="small"
                style="width: 100%"
                @update:value="(v: any) => updateField(index, 'nodeType', v)"
              />
            </div>
            <div class="nd-field nd-field-half">
              <label>{{ $t('system.workflow.node.assigneeType') }}</label>
              <Select
                :value="node.assigneeType"
                :disabled="disabled"
                :options="assigneeTypeOptions"
                size="small"
                style="width: 100%"
                @update:value="(v: any) => updateField(index, 'assigneeType', v)"
              />
            </div>
          </div>

          <div v-if="needsAssigneeSelect(node.assigneeType)" class="nd-field">
            <label>{{ $t('system.workflow.node.assignee') }}</label>
            <Select
              :value="node.assigneeValue || undefined"
              :disabled="disabled"
              :loading="isAssigneeLoading(node.assigneeType)"
              :options="getAssigneeOptions(node.assigneeType)"
              :placeholder="$t('system.workflow.node.assigneePlaceholder')"
              show-search
              size="small"
              :filter-option="
                (input: string, option: any) =>
                  option.label.toLowerCase().includes(input.toLowerCase())
              "
              style="width: 100%"
              @update:value="(v: any) => updateField(index, 'assigneeValue', v)"
              @dropdown-visible-change="
                (open: boolean) => open && onAssigneeDropdownOpen(node.assigneeType)
              "
            />
          </div>

          <div v-if="node.nodeType === 1" class="nd-field">
            <label>{{ $t('system.workflow.node.approvalMode') }}</label>
            <Select
              :value="node.approvalMode ?? 0"
              :disabled="disabled"
              :options="approvalModeOptions"
              size="small"
              style="width: 100%"
              @update:value="(v: any) => updateField(index, 'approvalMode', v)"
            />
          </div>

          <div class="nd-field">
            <label>{{ $t('system.workflow.node.description') }}</label>
            <Input
              :value="node.description"
              :disabled="disabled"
              size="small"
              :placeholder="$t('system.workflow.node.descriptionPlaceholder')"
              @update:value="(v: string) => updateField(index, 'description', v)"
            />
          </div>
        </div>
      </Card>

      <div class="nd-line" />
    </template>

    <!-- 底部添加按钮 -->
    <div v-if="!disabled && nodes.length > 0" class="nd-add-wrapper">
      <Tooltip :title="$t('system.workflow.node.add')">
        <button class="nd-add-btn" @click="addNode()">
          <span>+</span>
        </button>
      </Tooltip>
    </div>
    <div v-if="!disabled && nodes.length > 0" class="nd-line nd-line-short" />

    <!-- ========== 结束节点 ========== -->
    <div class="nd-terminal">
      <div class="nd-terminal-dot nd-terminal-end">
        <span>■</span>
      </div>
      <span class="nd-terminal-text">{{ $t('system.workflow.node.end') }}</span>
    </div>
  </div>
  </div>
</template>

<style scoped>
.nd-wrapper {
  max-height: 480px;
  overflow-y: auto;
  overflow-x: hidden;
  padding: 4px 8px;
  margin: 0 -8px;
}

.nd {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 20px 0;
  min-height: 200px;
}

/* ===== 开始/结束节点 ===== */
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
  transition: transform 0.2s;
}

.nd-terminal-dot:hover {
  transform: scale(1.05);
}

.nd-terminal-start {
  background: linear-gradient(135deg, #52c41a, #73d13d);
}

.nd-terminal-end {
  background: linear-gradient(135deg, #ff4d4f, #ff7875);
}

.nd-terminal-text {
  font-size: 12px;
  color: #8c8c8c;
  font-weight: 500;
  letter-spacing: 1px;
}

/* ===== 连接线 ===== */
.nd-line {
  width: 2px;
  height: 28px;
  background: linear-gradient(to bottom, #d9d9d9, #bfbfbf);
  position: relative;
}

.nd-line-short {
  height: 14px;
}

.nd-line::after {
  content: '';
  position: absolute;
  bottom: -3px;
  left: 50%;
  transform: translateX(-50%);
  width: 0;
  height: 0;
  border-left: 4px solid transparent;
  border-right: 4px solid transparent;
  border-top: 5px solid #bfbfbf;
}

.nd-line-short::after {
  display: none;
}

/* ===== 添加按钮 ===== */
.nd-add-wrapper {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2px 0;
}

.nd-add-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 28px;
  height: 28px;
  border-radius: 50%;
  border: 2px dashed #d9d9d9;
  background: #fff;
  color: #bfbfbf;
  font-size: 18px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.25s;
  line-height: 1;
}

.nd-add-btn:hover {
  border-color: #1677ff;
  color: #1677ff;
  background: #e6f4ff;
  transform: scale(1.15);
  box-shadow: 0 2px 8px rgb(22 119 255 / 20%);
}

/* ===== 节点卡片 ===== */
.nd-card {
  width: 100%;
  max-width: 440px;
  border-radius: 10px;
  overflow: hidden;
  position: relative;
  box-shadow: 0 2px 12px rgb(0 0 0 / 6%);
  border: 1px solid #f0f0f0;
  transition: box-shadow 0.25s, transform 0.2s;
}

.nd-card:hover {
  box-shadow: 0 4px 20px rgb(0 0 0 / 10%);
  transform: translateY(-1px);
}

.nd-card-accent {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 3px;
}

/* ===== 卡片头部 ===== */
.nd-card-head {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  margin-bottom: 14px;
  cursor: pointer;
  user-select: none;
}

.nd-card-head-collapsed {
  margin-bottom: 0;
}

.nd-collapse-icon {
  font-size: 10px;
  color: #8c8c8c;
  margin-left: 6px;
  flex-shrink: 0;
}

.nd-card-title {
  display: flex;
  align-items: center;
  gap: 10px;
}

.nd-card-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 36px;
  height: 36px;
  border-radius: 8px;
  font-size: 16px;
  flex-shrink: 0;
}

.nd-card-name {
  font-size: 14px;
  font-weight: 600;
  color: #262626;
  line-height: 1.4;
  max-width: 240px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.nd-card-actions {
  display: flex;
  gap: 0;
  opacity: 0.5;
  transition: opacity 0.2s;
}

.nd-card:hover .nd-card-actions {
  opacity: 1;
}

/* ===== 表单字段 ===== */
.nd-card-form {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.nd-field {
  display: flex;
  flex-direction: column;
  gap: 3px;
}

.nd-field label {
  font-size: 12px;
  color: #8c8c8c;
  font-weight: 500;
}

.nd-field-row {
  display: flex;
  gap: 10px;
}

.nd-field-half {
  flex: 1;
  min-width: 0;
}

/* ===== 空状态 ===== */
.nd-empty {
  padding: 28px 0;
}
</style>
