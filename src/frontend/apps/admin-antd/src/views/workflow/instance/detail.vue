<script lang="ts" setup>
import type { WorkflowApi } from '#/api/system/workflow';

import { computed, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';
import { useUserStore } from '@vben/stores';

import {
  Button,
  Card,
  Checkbox,
  Descriptions,
  DescriptionsItem,
  Input,
  message,
  Modal,
  Step,
  Steps,
  Tag,
  Timeline,
  TimelineItem,
} from 'ant-design-vue';

import {
  approveTask,
  cancelWorkflow,
  completeTask,
  getDefinition,
  getInstance,
  getTaskReturnFields,
  readTask,
  rejectTask,
  returnTask,
} from '#/api/system/workflow';
import { $t } from '#/locales';
import { navigateBackToList } from '#/utils/list-return-state';
import { isAssignedWorkflowInstanceId } from '#/utils/workflow-instance-id';
import {
  designerSchemaJsonToStepList,
  findCurrentStepIndex,
  mapProgressStepsToStepItems,
  type StepItem,
} from './definitionToSteps';

const route = useRoute();
const router = useRouter();
const userStore = useUserStore();

const instanceId = computed(() => route.params.id as string);
const currentUserId = computed(() => String(userStore.userInfo?.userId ?? ''));
const detail = ref<WorkflowApi.WorkflowInstanceDetail>();
const loading = ref(false);
const definitionLoading = ref(false);
const rawStepList = ref<StepItem[]>([]);
const stepList = ref<StepItem[]>([]);
const designerSchemaJsonCache = ref<string | null>(null);

const returnModalVisible = ref(false);
const returnModalLoading = ref(false);
const returnTaskRef = ref<WorkflowApi.WorkflowTask | null>(null);
const returnFieldMode = ref<'Disabled' | 'Required' | string>('Disabled');
const returnFieldOptions = ref<WorkflowApi.WorkflowReturnField[]>([]);
const returnSelectedFieldKeys = ref<string[]>([]);
const returnComment = ref('');

const rejectModalVisible = ref(false);
const rejectModalLoading = ref(false);
const rejectTaskRef = ref<WorkflowApi.WorkflowTask | null>(null);
const rejectComment = ref('');

const showCancelWorkflowButton = computed(() => {
  const d = detail.value;
  if (!d || d.status !== 0) return false;
  const initiatorId = String(d.initiatorId ?? '').trim();
  return initiatorId.length > 0 && initiatorId === currentUserId.value;
});

function showTimelineStandardApprovalActions(task: WorkflowApi.WorkflowTask): boolean {
  return task.status === 0 && !!task.canOperate && isApprovalWorkflowTask(task.taskType);
}

const instanceStatusLabels: Record<
  number,
  { color: string; label: string }
> = {
  0: {
    color: 'processing',
    label: $t('system.workflow.instance.statusRunning'),
  },
  1: {
    color: 'warning',
    label: $t('system.workflow.instance.statusSuspended'),
  },
  2: {
    color: 'success',
    label: $t('system.workflow.instance.statusCompleted'),
  },
  3: {
    color: 'error',
    label: $t('system.workflow.instance.statusRejected'),
  },
  4: {
    color: 'default',
    label: $t('system.workflow.instance.statusCancelled'),
  },
  5: {
    color: 'error',
    label: $t('system.workflow.instance.statusFaulted'),
  },
};

const taskStatusLabels: Record<
  number,
  { color: string; label: string }
> = {
  0: {
    color: 'processing',
    label: $t('system.workflow.task.statusPending'),
  },
  1: {
    color: 'success',
    label: $t('system.workflow.task.statusApproved'),
  },
  2: { color: 'error', label: $t('system.workflow.task.statusRejected') },
  3: {
    color: 'warning',
    label: $t('system.workflow.task.statusTransferred'),
  },
  4: {
    color: 'default',
    label: $t('system.workflow.task.statusCancelled'),
  },
  5: {
    color: 'warning',
    label: $t('system.workflow.task.statusDelegated'),
  },
  6: {
    color: 'success',
    label: $t('system.workflow.task.statusRead'),
  },
  7: {
    color: 'success',
    label: $t('system.workflow.task.statusTaskCompleted'),
  },
  8: {
    color: 'default',
    label: $t('system.workflow.task.statusAutoSkipped'),
  },
  9: {
    color: 'warning',
    label: '已退回',
  },
};

const taskTypeLabels: Record<number, string> = {
  0: $t('system.workflow.task.taskTypeApproval'),
  1: $t('system.workflow.task.taskTypeNotification'),
  2: $t('system.workflow.task.taskTypeCarbonCopy'),
};

/** 退回创建的新待办带有 returnContext，展示为“退回处理”而不是普通审批任务。 */
function isWorkflowReturnHandlingTask(task: WorkflowApi.WorkflowTask): boolean {
  return task.status === 0 && !!task.returnContext;
}

/** 时间线任务类型标签；退回待办是补正编辑语义，不是发起人重新审批。 */
function getTaskTypeTimelineLabel(task: WorkflowApi.WorkflowTask): string {
  if (isWorkflowReturnHandlingTask(task)) return '退回处理';
  return taskTypeLabels[task.taskType] ?? '';
}

/** 发起人退回处理待办只负责补正后继续提交，不再提供驳回或继续退回动作。 */
function canRejectOrReturnTimelineTask(task: WorkflowApi.WorkflowTask): boolean {
  return !isWorkflowReturnHandlingTask(task);
}

/** 仅审批任务显示办理按钮（抄送/通知为知会，无通过驳回） */
function isApprovalWorkflowTask(taskType: number): boolean {
  return taskType === 0;
}

function isNotifyOrCarbonCopyWorkflowTask(taskType: number): boolean {
  return taskType === 1 || taskType === 2;
}

/** 流程正常完成时，引擎对未办理的抄送/通知待办调用 Cancel，状态仍为 Cancelled；时间线应展示为「已发送」。 */
function isNotifyOrCcClosedByWorkflowCompletion(task: WorkflowApi.WorkflowTask): boolean {
  return (
    (task.taskType === 1 || task.taskType === 2) &&
    task.status === 4 &&
    detail.value?.status === 2
  );
}

function getTaskTimelineStatusTag(task: WorkflowApi.WorkflowTask): { color: string; label: string } {
  if (isNotifyOrCcClosedByWorkflowCompletion(task)) {
    return {
      color: 'success',
      label: $t('system.workflow.task.statusSentOnCompletion'),
    };
  }
  return taskStatusLabels[task.status] ?? { color: 'default', label: '' };
}

function syncVisibleProgressSteps() {
  stepList.value = rawStepList.value;
}

function applyProgressStepsFromDetail() {
  const steps = detail.value?.progressSteps;
  if (steps && steps.length > 0) {
    rawStepList.value = mapProgressStepsToStepItems(steps);
    syncVisibleProgressSteps();
    return true;
  }
  return false;
}

async function loadDetail() {
  if (!isAssignedWorkflowInstanceId(instanceId.value)) {
    message.warning('流程实例无效或已解除关联，请到「我的待办」查看当前任务');
    void router.replace('/workflow/pending');
    return;
  }
  loading.value = true;
  try {
    detail.value = await getInstance(instanceId.value);
    if (applyProgressStepsFromDetail()) {
      syncVisibleProgressSteps();
    } else {
      await loadDefinitionStepsFallback();
    }
  } finally {
    loading.value = false;
  }
}

/** 详情未返回 progressSteps 时，从定义 JSON 展平步骤用于展示。 */
async function loadDefinitionStepsFallback() {
  const defId = detail.value?.workflowDefinitionId;
  if (!defId) {
    rawStepList.value = [];
    stepList.value = [];
    designerSchemaJsonCache.value = null;
    return;
  }
  definitionLoading.value = true;
  try {
    const def = await getDefinition(defId);
    designerSchemaJsonCache.value = def?.designerSchemaJson ?? null;
    rawStepList.value = designerSchemaJsonToStepList(designerSchemaJsonCache.value);
    syncVisibleProgressSteps();
  } catch {
    rawStepList.value = [];
    stepList.value = [];
    designerSchemaJsonCache.value = null;
  } finally {
    definitionLoading.value = false;
  }
}

/** 展示用步骤列表：在解析出的节点后追加「结束」一步 */
const displayStepList = computed(() => {
  if (stepList.value.length === 0) return [];
  return [
    ...stepList.value,
    { title: $t('system.workflow.instance.progressEnd') },
  ];
});

const currentStepIndex = computed(() => {
  if (!detail.value || displayStepList.value.length === 0) return 0;
  const status = detail.value.status;
  const isEnded =
    status === 2 ||
    status === 3 ||
    status === 4 ||
    status === 5; /* 已完成/已驳回/已取消/异常 */
  if (isEnded) return displayStepList.value.length - 1;
  return findCurrentStepIndex(
    stepList.value,
    detail.value.currentNodeName,
    detail.value.currentNodeKey,
  );
});

function getTimelineColorForTask(task: WorkflowApi.WorkflowTask): string {
  if (isNotifyOrCcClosedByWorkflowCompletion(task)) return 'green';
  const colors: Record<number, string> = {
    0: 'blue',
    1: 'green',
    2: 'red',
    3: 'orange',
    4: 'gray',
    5: 'orange',
    6: 'green',
    7: 'green',
    8: 'gray',
    9: 'orange',
  };
  return colors[task.status] ?? 'blue';
}

function formatDateTime(dt?: string): string {
  if (!dt) return '-';
  return new Date(dt).toLocaleString('zh-CN');
}

/** 后端未设置的可选时间可能为 DateTimeOffset.MinValue（0001-01-01） */
function formatOptionalDateTime(dt?: string | null): string {
  if (!dt || dt.startsWith('0001-01-01')) return '-';
  const time = Date.parse(dt);
  if (Number.isNaN(time) || new Date(time).getFullYear() <= 1900) return '-';
  return new Date(time).toLocaleString('zh-CN');
}

/** 后端默认 DateTime 可能序列化为 0001-01-01；待处理任务或无效日期不展示完成时间。 */
function hasValidCompletedAt(task: WorkflowApi.WorkflowTask): boolean {
  if (!task.completedAt || task.status === 0) return false;
  const time = Date.parse(task.completedAt);
  if (Number.isNaN(time)) return false;
  return new Date(time).getFullYear() > 1900;
}

/** 发起记录使用实例 startedAt，和后续审批任务共同组成完整时间线起点。 */
function getWorkflowStartedAt(): string | undefined {
  return detail.value?.startedAt || (detail.value as any)?.createdAt;
}

async function onApproveTask(task: WorkflowApi.WorkflowTask) {
  Modal.confirm({
    content: $t('system.workflow.task.approveConfirmContent'),
    title: $t('system.workflow.task.approveTitle'),
    async onOk() {
      await approveTask({
        workflowInstanceId: instanceId.value,
        taskId: task.id,
        comment: '同意',
      });
      message.success('审批通过');
      loadDetail();
    },
  });
}

function onRejectTask(task: WorkflowApi.WorkflowTask) {
  rejectTaskRef.value = task;
  rejectComment.value = '';
  rejectModalVisible.value = true;
}

async function handleRejectModalOk() {
  const task = rejectTaskRef.value;
  if (!task) return;
  if (!rejectComment.value.trim()) {
    message.warning($t('system.workflow.task.rejectCommentRequired'));
    return Promise.reject(new Error('validation'));
  }
  rejectModalLoading.value = true;
  try {
    await rejectTask({
      workflowInstanceId: instanceId.value,
      taskId: task.id,
      comment: rejectComment.value.trim(),
    });
    message.success('已驳回');
    rejectModalVisible.value = false;
    loadDetail();
  } finally {
    rejectModalLoading.value = false;
  }
}

async function onReturnTask(task: WorkflowApi.WorkflowTask) {
  returnTaskRef.value = task;
  returnFieldMode.value = 'Disabled';
  returnSelectedFieldKeys.value = [];
  returnComment.value = '';
  returnModalLoading.value = true;
  returnModalVisible.value = true;
  try {
    // 实例详情里的退回按钮和待办列表共用同一后端配置，保证字段必选规则一致。
    const options = await getTaskReturnFields({
      workflowInstanceId: instanceId.value,
      taskId: task.id,
    });
    returnFieldMode.value = options.fieldMode ?? 'Disabled';
    returnFieldOptions.value = options.fields ?? [];
  } finally {
    returnModalLoading.value = false;
  }
}

async function handleReturnModalOk() {
  const task = returnTaskRef.value;
  if (!task) return;
  if (returnFieldMode.value === 'Required' && returnSelectedFieldKeys.value.length === 0) {
    message.warning('请选择需要修改的字段');
    return Promise.reject(new Error('validation'));
  }
  if (!returnComment.value.trim()) {
    message.warning('请填写退回说明');
    return Promise.reject(new Error('validation'));
  }

  returnModalLoading.value = true;
  try {
    const selected = new Set(returnSelectedFieldKeys.value);
    // Disabled 模式只提交退回说明；Required 模式提交勾选字段对象，后端继续做白名单校验。
    await returnTask({
      workflowInstanceId: instanceId.value,
      taskId: task.id,
      comment: returnComment.value.trim(),
      returnFields:
        returnFieldMode.value === 'Required'
          ? returnFieldOptions.value.filter((f) => selected.has(f.key))
          : [],
    });
    message.success('已退回');
    returnModalVisible.value = false;
    loadDetail();
  } finally {
    returnModalLoading.value = false;
  }
}

function onCompleteNotifyOrCarbonCopyTask(task: WorkflowApi.WorkflowTask) {
  Modal.confirm({
    content: task.taskType === 2 ? '确认标记该抄送任务为已读吗？' : '确认完成该通知任务吗？',
    title: $t('system.workflow.task.completeNotifyCarbon'),
    async onOk() {
      if (task.taskType === 2) {
        await readTask({
          workflowInstanceId: instanceId.value,
          taskId: task.id,
          comment: '已读',
        });
        message.success('已标记为已读');
      } else {
        await completeTask({
          workflowInstanceId: instanceId.value,
          taskId: task.id,
          comment: '已完成',
        });
        message.success('已完成');
      }
      loadDetail();
    },
  });
}

function onCancel() {
  if (!detail.value) return;
  Modal.confirm({
    content: '确认要撤销此流程吗？撤销后不可恢复。',
    title: '撤销确认',
    async onOk() {
      await cancelWorkflow(instanceId.value);
      message.success('流程已撤销');
      loadDetail();
    },
  });
}

const WORKFLOW_INSTANCE_RETURN_PATHS = [
  '/workflow/pending',
  '/workflow/completed',
  '/workflow/my-workflows',
  '/workflow/monitor',
] as const;

function onBack() {
  const fallback =
    (route.meta.activePath as string | undefined) ?? '/workflow/monitor';
  void navigateBackToList(router, route, WORKFLOW_INSTANCE_RETURN_PATHS, fallback);
}

onMounted(() => {
  loadDetail();
});
</script>
<template>
  <Page auto-content-height>
    <div class="space-y-4 p-4">
      <!-- 返回按钮 -->
      <div>
        <Button @click="onBack">返回</Button>
      </div>

      <!-- 基本信息 -->
      <Card v-if="detail" :loading="loading" :title="detail.title">
        <Descriptions :column="3" bordered size="small">
          <DescriptionsItem
            :label="$t('system.workflow.instance.definitionName')"
          >
            {{ detail.workflowDefinitionName }}
          </DescriptionsItem>
          <DescriptionsItem
            :label="$t('system.workflow.instance.initiator')"
          >
            {{ detail.initiatorName }}
          </DescriptionsItem>
          <DescriptionsItem :label="$t('system.workflow.instance.status')">
            <Tag
              :color="
                instanceStatusLabels[detail.status]?.color ?? 'default'
              "
            >
              {{ instanceStatusLabels[detail.status]?.label ?? '' }}
            </Tag>
          </DescriptionsItem>
          <DescriptionsItem
            :label="$t('system.workflow.instance.businessKey')"
          >
            {{ detail.businessKey || '-' }}
          </DescriptionsItem>
          <DescriptionsItem :label="$t('system.workflow.instance.businessType')">
            {{ detail.businessType || '-' }}
          </DescriptionsItem>
          <DescriptionsItem
            :label="$t('system.workflow.instance.currentNode')"
          >
            {{ detail.currentNodeName || '-' }}
          </DescriptionsItem>
          <DescriptionsItem
            :label="$t('system.workflow.instance.startedAt')"
          >
            {{ formatDateTime(detail.startedAt) }}
          </DescriptionsItem>
          <DescriptionsItem
            :label="$t('system.workflow.instance.completedAt')"
          >
            {{ formatDateTime(detail.completedAt) }}
          </DescriptionsItem>
          <DescriptionsItem
            :label="$t('system.workflow.instance.suspendedAt')"
          >
            {{ formatOptionalDateTime(detail.suspendedAt) }}
          </DescriptionsItem>
          <DescriptionsItem
            :label="$t('system.workflow.instance.resumedAt')"
          >
            {{ formatOptionalDateTime(detail.resumedAt) }}
          </DescriptionsItem>
          <DescriptionsItem :label="$t('system.workflow.instance.remark')">
            {{ detail.remark || '-' }}
          </DescriptionsItem>
        </Descriptions>

        <div v-if="showCancelWorkflowButton" class="mt-4">
          <Button danger @click="onCancel">
            {{ $t('system.workflow.instance.cancel') }}
          </Button>
        </div>
      </Card>

      <!-- 流程进度 -->
      <Card
        v-if="detail && displayStepList.length > 0"
        :loading="definitionLoading"
        :title="$t('system.workflow.instance.progressTitle')"
      >
        <Steps :current="currentStepIndex" size="small">
          <Step
            v-for="(step, index) in displayStepList"
            :key="index"
            :title="step.title"
          />
        </Steps>
      </Card>

      <!-- 审批时间线 -->
      <Card
        v-if="detail && detail.tasks?.length > 0"
        :loading="loading"
        title="审批记录"
      >
        <Timeline>
          <TimelineItem color="green">
            <div class="font-medium">
              发起人
              <Tag color="success" class="ml-2">已提交</Tag>
              <Tag class="ml-1">发起</Tag>
            </div>
            <div class="mt-1 text-sm text-gray-500">
              发起人: {{ detail.initiatorName || '-' }}
            </div>
            <div class="mt-1 text-xs text-gray-400">
              提交: {{ formatDateTime(getWorkflowStartedAt()) }}
            </div>
          </TimelineItem>
          <TimelineItem
            v-for="task in detail.tasks"
            :key="task.id"
            :color="getTimelineColorForTask(task)"
          >
            <div class="flex items-start justify-between">
              <div>
                <div class="font-medium">
                  {{ task.nodeName }}
                  <Tag
                    :color="getTaskTimelineStatusTag(task).color"
                    class="ml-2"
                  >
                    {{ getTaskTimelineStatusTag(task).label }}
                  </Tag>
                  <Tag class="ml-1">
                    {{ getTaskTypeTimelineLabel(task) }}
                  </Tag>
                </div>
                <div class="mt-1 text-sm text-gray-500">
                  处理人: {{ task.assigneeName || '-' }}
                </div>
                <div
                  v-if="task.completedByUserDisplayName"
                  class="mt-1 text-sm text-gray-500"
                >
                  实际操作人: {{ task.completedByUserDisplayName }}
                </div>
                <div v-if="task.comment" class="mt-1 text-sm">
                  意见: {{ task.comment }}
                </div>
                <div v-if="task.returnContext" class="mt-1 text-sm text-orange-600">
                  退回到节点: {{ task.returnContext.returnToNodeName || '-' }}
                  <span class="ml-2">
                    需修改字段:
                    {{ task.returnContext.returnFields?.map((f) => f.label).join('、') || '-' }}
                  </span>
                </div>
                <div v-if="task.returnContext?.comment" class="mt-1 text-sm text-orange-600">
                  退回说明: {{ task.returnContext.comment }}
                </div>
                <div class="mt-1 text-xs text-gray-400">
                  创建: {{ formatDateTime(task.createdAt) }}
                  <span v-if="hasValidCompletedAt(task)">
                    | 完成: {{ formatDateTime(task.completedAt) }}
                  </span>
                </div>
              </div>
              <!-- 当前待办：仅审批任务可通过/驳回 -->
              <div
                v-if="showTimelineStandardApprovalActions(task)"
                class="ml-4 flex gap-2"
              >
                <Button
                  size="small"
                  type="primary"
                  @click="onApproveTask(task)"
                >
                  {{ $t('system.workflow.task.approve') }}
                </Button>
                <Button v-if="canRejectOrReturnTimelineTask(task)" danger size="small" @click="onRejectTask(task)">
                  {{ $t('system.workflow.task.reject') }}
                </Button>
                <Button v-if="canRejectOrReturnTimelineTask(task)" size="small" @click="onReturnTask(task)">
                  退回
                </Button>
              </div>
              <div
                v-else-if="task.status === 0 && task.canOperate && isNotifyOrCarbonCopyWorkflowTask(task.taskType)"
                class="ml-4 flex gap-2"
              >
                <Button
                  size="small"
                  type="primary"
                  @click="onCompleteNotifyOrCarbonCopyTask(task)"
                >
                  {{ $t('system.workflow.task.completeNotifyCarbon') }}
                </Button>
              </div>
            </div>
          </TimelineItem>
        </Timeline>
      </Card>

      <Modal
        v-model:open="returnModalVisible"
        :confirm-loading="returnModalLoading"
        title="退回"
        cancel-text="取消"
        ok-text="确定"
        @ok="handleReturnModalOk"
      >
        <div v-if="returnFieldMode === 'Required'" class="mb-4">
          <div class="mb-2">需修改字段</div>
          <Checkbox.Group v-model:value="returnSelectedFieldKeys" class="w-full">
            <div class="grid grid-cols-2 gap-2">
              <Checkbox
                v-for="field in returnFieldOptions"
                :key="field.key"
                :value="field.key"
              >
                {{ field.label }}
              </Checkbox>
            </div>
          </Checkbox.Group>
        </div>
        <div>
          <div class="mb-2">退回说明</div>
          <Input.TextArea v-model:value="returnComment" :rows="3" />
        </div>
      </Modal>

      <Modal
        v-model:open="rejectModalVisible"
        :confirm-loading="rejectModalLoading"
        :title="$t('system.workflow.task.rejectTitle')"
        cancel-text="取消"
        ok-text="确认驳回"
        ok-type="danger"
        @ok="handleRejectModalOk"
      >
        <div class="mb-3 text-muted-foreground text-sm">
          {{ $t('system.workflow.task.rejectConfirmHint') }}
        </div>
        <div>
          <div class="mb-2">{{ $t('system.workflow.task.rejectComment') }}</div>
          <Input.TextArea
            v-model:value="rejectComment"
            :placeholder="$t('system.workflow.task.rejectCommentPlaceholder')"
            :rows="3"
          />
        </div>
      </Modal>

    </div>
  </Page>
</template>
