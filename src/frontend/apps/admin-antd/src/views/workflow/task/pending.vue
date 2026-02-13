<script lang="ts" setup>
import type { Recordable } from '@vben/types';

import type { WorkflowApi } from '#/api/system/workflow';

import { ref, watch } from 'vue';
import { useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';

import { Button, message, Modal, Select } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import {
  approveTask,
  delegateTask,
  getMyPendingTasks,
  rejectTask,
} from '#/api/system/workflow';
import { getUserList } from '#/api/system/user';
import { $t } from '#/locales';

const router = useRouter();

const taskTypeLabels: Record<number, string> = {
  0: $t('system.workflow.task.taskTypeApproval'),
  1: $t('system.workflow.task.taskTypeNotification'),
  2: $t('system.workflow.task.taskTypeCarbonCopy'),
};

const [Grid, gridApi] = useVbenVxeGrid<WorkflowApi.MyPendingTask>({
  formOptions: {
    schema: [
      {
        component: 'Input',
        fieldName: 'title',
        label: $t('system.workflow.task.flowTitle'),
      },
    ],
    submitOnChange: true,
  },
  gridOptions: {
    columns: [
      {
        field: 'workflowTitle',
        title: $t('system.workflow.task.flowTitle'),
        minWidth: 200,
      },
      {
        field: 'workflowDefinitionName',
        title: $t('system.workflow.task.definitionName'),
        width: 150,
      },
      {
        field: 'initiatorName',
        title: $t('system.workflow.task.initiator'),
        width: 120,
      },
      {
        field: 'nodeName',
        title: $t('system.workflow.task.nodeName'),
        width: 150,
      },
      {
        field: 'taskType',
        title: $t('system.workflow.task.taskType'),
        width: 100,
        formatter: ({ row }: { row: WorkflowApi.MyPendingTask }) =>
          taskTypeLabels[row.taskType] ?? '',
      },
      {
        field: 'createdAt',
        formatter: 'formatDateTime',
        title: $t('system.workflow.task.createdAt'),
        width: 180,
      },
      {
        align: 'center',
        field: 'operation',
        fixed: 'right',
        title: $t('system.workflow.task.operation'),
        width: 300,
        slots: { default: 'action' },
      },
    ],
    height: 'auto',
    keepSource: true,
    proxyConfig: {
      ajax: {
        query: async (
          { page }: { page: { currentPage: number; pageSize: number } },
          formValues: Recordable<any>,
        ) => {
          const result = await getMyPendingTasks({
            pageIndex: page.currentPage,
            pageSize: page.pageSize,
            countTotal: true,
            ...formValues,
          });
          return {
            items: result.items,
            total: result.total,
          };
        },
      },
    },
    rowConfig: {
      keyField: 'taskId',
    },
    toolbarConfig: {
      custom: true,
      export: false,
      refresh: true,
      search: true,
      zoom: true,
    },
  },
});

function onApprove(row: WorkflowApi.MyPendingTask) {
  Modal.confirm({
    content: '确认审批通过该任务吗？',
    title: '审批确认',
    async onOk() {
      await approveTask({
        workflowInstanceId: row.workflowInstanceId,
        taskId: row.taskId,
        comment: '同意',
      });
      message.success('审批通过');
      gridApi.query();
    },
  });
}

function onReject(row: WorkflowApi.MyPendingTask) {
  Modal.confirm({
    content: '确认驳回该任务吗？驳回后流程将终止。',
    title: '驳回确认',
    async onOk() {
      await rejectTask({
        workflowInstanceId: row.workflowInstanceId,
        taskId: row.taskId,
        comment: '不同意',
      });
      message.success('已驳回');
      gridApi.query();
    },
  });
}

function onViewDetail(row: WorkflowApi.MyPendingTask) {
  router.push(`/workflow/instance/${row.workflowInstanceId}`);
}

// 委托相关
const delegateModalVisible = ref(false);
const delegateModalLoading = ref(false);
const delegateRow = ref<WorkflowApi.MyPendingTask | null>(null);
const delegateUserOptions = ref<{ label: string; value: string }[]>([]);
const delegateUserLoading = ref(false);
const delegateSelectedUserId = ref<string | undefined>();
const delegateSelectedUserName = ref('');
const delegateComment = ref('');

watch(delegateModalVisible, (visible) => {
  if (visible) {
    delegateSelectedUserId.value = undefined;
    delegateSelectedUserName.value = '';
    delegateComment.value = '';
    loadDelegateUsers();
  }
});

async function loadDelegateUsers() {
  if (delegateUserOptions.value.length > 0) return;
  delegateUserLoading.value = true;
  try {
    const result = await getUserList({
      pageIndex: 1,
      pageSize: 500,
      countTotal: false,
    });
    delegateUserOptions.value = result.items.map((u) => ({
      label: `${u.realName || u.name}${u.deptName ? ` (${u.deptName})` : ''}`,
      value: u.userId,
    }));
  } finally {
    delegateUserLoading.value = false;
  }
}

function openDelegateModal(row: WorkflowApi.MyPendingTask) {
  delegateRow.value = row;
  delegateUserOptions.value = [];
  delegateModalVisible.value = true;
}

function filterDelegateUser(input: string, option: unknown) {
  const opt = option as { label?: string };
  return (opt?.label ?? '').toLowerCase().includes(input.toLowerCase());
}

function onDelegateUserChange(value: string | undefined) {
  const opt = delegateUserOptions.value.find((o) => o.value === value);
  delegateSelectedUserName.value = opt?.label ?? '';
}

async function handleDelegateOk() {
  const row = delegateRow.value;
  if (!row || !delegateSelectedUserId.value || !delegateSelectedUserName.value) {
    message.warning($t('system.workflow.task.selectDelegateUser'));
    throw new Error('Validation failed');
  }
  if (!delegateComment.value?.trim()) {
    message.warning($t('system.workflow.task.delegateCommentPlaceholder'));
    throw new Error('Validation failed');
  }
  delegateModalLoading.value = true;
  try {
    await delegateTask({
      instanceId: row.workflowInstanceId,
      taskId: row.taskId,
      delegateToUserId: delegateSelectedUserId.value,
      delegateToUserName: delegateSelectedUserName.value,
      comment: delegateComment.value.trim(),
    });
    message.success($t('system.workflow.task.delegateSuccess'));
    delegateModalVisible.value = false;
    gridApi.query();
  } catch (e) {
    if (e instanceof Error && e.message !== 'Validation failed') {
      message.error((e as Error).message || '委托失败');
    }
    throw e;
  } finally {
    delegateModalLoading.value = false;
  }
}
</script>
<template>
  <Page auto-content-height>
    <Grid :table-title="$t('system.workflow.task.pendingTitle')">
      <template #action="{ row }">
        <Button size="small" type="primary" @click="onApprove(row)">
          {{ $t('system.workflow.task.approve') }}
        </Button>
        <Button danger size="small" class="ml-2" @click="onReject(row)">
          {{ $t('system.workflow.task.reject') }}
        </Button>
        <Button size="small" class="ml-2" @click="openDelegateModal(row)">
          {{ $t('system.workflow.task.delegate') }}
        </Button>
        <Button size="small" class="ml-2" @click="onViewDetail(row)">
          {{ $t('system.workflow.instance.detail') }}
        </Button>
      </template>
    </Grid>
    <Modal
      v-model:open="delegateModalVisible"
      :confirm-loading="delegateModalLoading"
      :title="$t('system.workflow.task.delegateTitle')"
      cancel-text="取消"
      ok-text="确定"
      @ok="handleDelegateOk"
    >
      <div class="mb-4">
        <div class="mb-2">
          {{ $t('system.workflow.task.selectDelegateUser') }}
        </div>
        <Select
          v-model:value="delegateSelectedUserId"
          :loading="delegateUserLoading"
          :options="delegateUserOptions"
          :placeholder="$t('system.workflow.task.selectDelegateUserPlaceholder')"
          allow-clear
          class="w-full"
          show-search
          :filter-option="filterDelegateUser"
          @change="onDelegateUserChange"
        />
      </div>
      <div>
        <div class="mb-2">
          {{ $t('system.workflow.task.delegateComment') }}
        </div>
        <textarea
          v-model="delegateComment"
          :placeholder="$t('system.workflow.task.delegateCommentPlaceholder')"
          class="w-full rounded border px-3 py-2"
          rows="3"
        />
      </div>
    </Modal>
  </Page>
</template>
