<script lang="ts" setup>
import type { WorkflowApi } from '#/api/system/workflow';

import { computed, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';

import {
  Button,
  Card,
  Descriptions,
  DescriptionsItem,
  message,
  Modal,
  Tag,
  Timeline,
  TimelineItem,
} from 'ant-design-vue';

import {
  approveTask,
  cancelWorkflow,
  getInstance,
  rejectTask,
} from '#/api/system/workflow';
import { $t } from '#/locales';

const route = useRoute();
const router = useRouter();

const instanceId = computed(() => route.params.id as string);
const detail = ref<WorkflowApi.WorkflowInstanceDetail>();
const loading = ref(false);

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
};

const taskTypeLabels: Record<number, string> = {
  0: $t('system.workflow.task.taskTypeApproval'),
  1: $t('system.workflow.task.taskTypeNotification'),
  2: $t('system.workflow.task.taskTypeCarbonCopy'),
};

async function loadDetail() {
  loading.value = true;
  try {
    detail.value = await getInstance(instanceId.value);
  } finally {
    loading.value = false;
  }
}

function getTimelineColor(status: number): string {
  const colors: Record<number, string> = {
    0: 'blue',
    1: 'green',
    2: 'red',
    3: 'orange',
    4: 'gray',
  };
  return colors[status] ?? 'blue';
}

function formatDateTime(dt?: string): string {
  if (!dt) return '-';
  return new Date(dt).toLocaleString('zh-CN');
}

function onApproveTask(task: WorkflowApi.WorkflowTask) {
  Modal.confirm({
    content: '确认审批通过该任务吗？',
    title: '审批确认',
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
  Modal.confirm({
    content: '确认驳回该任务吗？',
    title: '驳回确认',
    async onOk() {
      await rejectTask({
        workflowInstanceId: instanceId.value,
        taskId: task.id,
        comment: '不同意',
      });
      message.success('已驳回');
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

function onBack() {
  router.back();
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
          <DescriptionsItem
            :label="$t('system.workflow.instance.businessType')"
          >
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
          <DescriptionsItem :label="$t('system.workflow.instance.remark')">
            {{ detail.remark || '-' }}
          </DescriptionsItem>
        </Descriptions>

        <!-- 操作按钮 -->
        <div v-if="detail.status === 0" class="mt-4">
          <Button danger @click="onCancel">
            {{ $t('system.workflow.instance.cancel') }}
          </Button>
        </div>
      </Card>

      <!-- 审批时间线 -->
      <Card
        v-if="detail && detail.tasks?.length > 0"
        :loading="loading"
        title="审批记录"
      >
        <Timeline>
          <TimelineItem
            v-for="task in detail.tasks"
            :key="task.id"
            :color="getTimelineColor(task.status)"
          >
            <div class="flex items-start justify-between">
              <div>
                <div class="font-medium">
                  {{ task.nodeName }}
                  <Tag
                    :color="
                      taskStatusLabels[task.status]?.color ?? 'default'
                    "
                    class="ml-2"
                  >
                    {{ taskStatusLabels[task.status]?.label ?? '' }}
                  </Tag>
                  <Tag class="ml-1">
                    {{ taskTypeLabels[task.taskType] ?? '' }}
                  </Tag>
                </div>
                <div class="mt-1 text-sm text-gray-500">
                  处理人: {{ task.assigneeName || '-' }}
                </div>
                <div v-if="task.comment" class="mt-1 text-sm">
                  意见: {{ task.comment }}
                </div>
                <div class="mt-1 text-xs text-gray-400">
                  创建: {{ formatDateTime(task.createdAt) }}
                  <span v-if="task.completedAt">
                    | 完成: {{ formatDateTime(task.completedAt) }}
                  </span>
                </div>
              </div>
              <!-- 当前待办任务的操作按钮 -->
              <div v-if="task.status === 0" class="ml-4 flex gap-2">
                <Button
                  size="small"
                  type="primary"
                  @click="onApproveTask(task)"
                >
                  {{ $t('system.workflow.task.approve') }}
                </Button>
                <Button danger size="small" @click="onRejectTask(task)">
                  {{ $t('system.workflow.task.reject') }}
                </Button>
              </div>
            </div>
          </TimelineItem>
        </Timeline>
      </Card>
    </div>
  </Page>
</template>
