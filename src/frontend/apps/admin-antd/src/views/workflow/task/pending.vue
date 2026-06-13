<script lang="ts" setup>
import type { Recordable } from '@vben/types';

import type { WorkflowApi } from '#/api/system/workflow';

import { onMounted, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';

import { Button, Checkbox, message, Modal, Select, Input } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { useListReturnState } from '#/composables/use-list-return-state';
import {
  approveTask,
  completeTask,
  delegateTask,
  getTaskReturnFields,
  getMyPendingTasks,
  readTask,
  rejectTask,
  returnTask,
} from '#/api/system/workflow';
import { getUserList } from '#/api/system/user';
import { $t } from '#/locales';
import { isAssignedWorkflowInstanceId } from '#/utils/workflow-instance-id';
import { handleVxeCellDblclick } from '#/utils/vxe-row-navigation';

const LIST_PATH = '/workflow/pending';
const SEARCH_KEYS = ['title'] as const;

const router = useRouter();
const route = useRoute();

const { shouldDeferGridAutoLoad, pagerConfig, trackPage, buildReturnQuery, restoreOnMount } =
  useListReturnState({
    route,
    listPath: LIST_PATH,
    searchKeys: SEARCH_KEYS,
  });

const taskTypeLabels: Record<number, string> = {
  0: $t('system.workflow.task.taskTypeApproval'),
  1: $t('system.workflow.task.taskTypeNotification'),
  2: $t('system.workflow.task.taskTypeCarbonCopy'),
};

/** 与后端 WorkflowTaskType 一致：0=审批，1=通知，2=抄送；仅审批任务可办理通过/驳回/委托 */
function isApprovalTaskType(taskType: number): boolean {
  return taskType === 0;
}

/** 通知/抄送待办：通知可完成，抄送可已读。 */
function isNotifyOrCarbonCopyTaskType(taskType: number): boolean {
  return taskType === 1 || taskType === 2;
}

/** 退回到发起人的补正待办只允许继续提交，不再允许在发起人节点再次驳回或退回。 */
function canRejectOrReturn(row: WorkflowApi.MyPendingTask): boolean {
  if (!isApprovalTaskType(row.taskType)) return false;
  return String(row.nodeName ?? '').trim() !== '发起人';
}

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
  gridEvents: {
    'cell-dblclick': (event: any) => handleVxeCellDblclick(event, onViewDetail),
  } as any,
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
      { field: '_flex', minWidth: 1, title: '' },
      {
        align: 'center',
        field: 'operation',
        fixed: 'right',
      showOverflow: false,
        title: $t('system.workflow.task.operation'),
        width: 360,
        slots: { default: 'action' },
      },
    ],
    height: 'auto',
    keepSource: true,
    pagerConfig,
    proxyConfig: {
      autoLoad: !shouldDeferGridAutoLoad,
      ajax: {
        query: async (
          { page }: { page: { currentPage: number; pageSize: number } },
          formValues: Recordable<any>,
        ) => {
          trackPage(page);
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
    rowClassName: () => 'vxe-row-clickable',
    toolbarConfig: {
      custom: true,
      export: false,
      refresh: true,
      search: true,
      zoom: true,
    },
  },
});

const returnModalVisible = ref(false);
const returnModalLoading = ref(false);
const returnRow = ref<WorkflowApi.MyPendingTask | null>(null);
const returnFieldMode = ref<'Disabled' | 'Required' | string>('Disabled');
const returnFieldOptions = ref<WorkflowApi.WorkflowReturnField[]>([]);
const returnSelectedFieldKeys = ref<string[]>([]);
const returnComment = ref('');

const rejectModalVisible = ref(false);
const rejectModalLoading = ref(false);
const rejectRow = ref<WorkflowApi.MyPendingTask | null>(null);
const rejectComment = ref('');

async function onApprove(row: WorkflowApi.MyPendingTask) {
  Modal.confirm({
    content: $t('system.workflow.task.approveConfirmContent'),
    title: $t('system.workflow.task.approveTitle'),
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
  rejectRow.value = row;
  rejectComment.value = '';
  rejectModalVisible.value = true;
}

async function handleRejectModalOk() {
  const row = rejectRow.value;
  if (!row) return;
  if (!rejectComment.value.trim()) {
    message.warning($t('system.workflow.task.rejectCommentRequired'));
    return Promise.reject(new Error('validation'));
  }
  rejectModalLoading.value = true;
  try {
    await rejectTask({
      workflowInstanceId: row.workflowInstanceId,
      taskId: row.taskId,
      comment: rejectComment.value.trim(),
    });
    message.success('已驳回');
    rejectModalVisible.value = false;
    gridApi.query();
  } finally {
    rejectModalLoading.value = false;
  }
}

async function onReturn(row: WorkflowApi.MyPendingTask) {
  returnRow.value = row;
  returnFieldMode.value = 'Disabled';
  returnSelectedFieldKeys.value = [];
  returnComment.value = '';
  returnModalLoading.value = true;
  returnModalVisible.value = true;
  try {
    // 打开退回弹窗时实时读取当前节点配置，避免前端缓存旧字段方案。
    const options = await getTaskReturnFields({
      workflowInstanceId: row.workflowInstanceId,
      taskId: row.taskId,
    });
    returnFieldMode.value = options.fieldMode ?? 'Disabled';
    returnFieldOptions.value = options.fields ?? [];
  } finally {
    returnModalLoading.value = false;
  }
}

async function handleReturnModalOk() {
  const row = returnRow.value;
  if (!row) return;
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
    // 提交完整字段对象而不是只提交 key，后端会再按业务适配器白名单标准化，历史记录也能保留 label/group 供详情展示。
    await returnTask({
      workflowInstanceId: row.workflowInstanceId,
      taskId: row.taskId,
      comment: returnComment.value.trim(),
      returnFields:
        returnFieldMode.value === 'Required'
          ? returnFieldOptions.value.filter((f) => selected.has(f.key))
          : [],
    });
    message.success('已退回');
    returnModalVisible.value = false;
    gridApi.query();
  } finally {
    returnModalLoading.value = false;
  }
}

async function onViewDetail(row: WorkflowApi.MyPendingTask) {
  if (!isAssignedWorkflowInstanceId(row.workflowInstanceId)) {
    message.warning('该待办关联的流程已结束或已撤回，请刷新列表');
    void gridApi.reload();
    return;
  }
  void router.push({
    path: `/workflow/instance/${row.workflowInstanceId}`,
    query: await buildReturnQuery(gridApi),
  });
}

function onCompleteNotifyOrCarbonCopy(row: WorkflowApi.MyPendingTask) {
  Modal.confirm({
    content: row.taskType === 2 ? '确认标记该抄送任务为已读吗？' : '确认完成该通知任务吗？',
    title: $t('system.workflow.task.completeNotifyCarbon'),
    async onOk() {
      if (row.taskType === 2) {
        await readTask({
          workflowInstanceId: row.workflowInstanceId,
          taskId: row.taskId,
          comment: '已读',
        });
        message.success('已标记为已读');
      } else {
        await completeTask({
          workflowInstanceId: row.workflowInstanceId,
          taskId: row.taskId,
          comment: '已完成',
        });
        message.success('已完成');
      }
      gridApi.query();
    },
  });
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

function onDelegateUserChange(value: unknown) {
  const selected = value == null ? undefined : String(value);
  const opt = delegateUserOptions.value.find((o) => o.value === selected);
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

onMounted(async () => {
  await restoreOnMount(gridApi);
});
</script>
<template>
  <Page auto-content-height>
    <div class="mb-3 text-muted-foreground text-sm">
      {{ $t('system.workflow.task.pendingTip') }}
      <Button type="link" class="p-0 h-auto min-h-0 ml-1" @click="router.push('/workflow/my-workflows')">
        {{ $t('system.workflow.task.myWorkflows') }}
      </Button>
    </div>
    <Grid :table-title="$t('system.workflow.task.pendingTitle')">
      <template #action="{ row }">
        <template v-if="isApprovalTaskType(row.taskType)">
          <Button size="small" type="primary" @click="onApprove(row)">
            {{ $t('system.workflow.task.approve') }}
          </Button>
          <Button v-if="canRejectOrReturn(row)" danger size="small" class="ml-2" @click="onReject(row)">
            {{ $t('system.workflow.task.reject') }}
          </Button>
          <Button v-if="canRejectOrReturn(row)" size="small" class="ml-2" @click="onReturn(row)">
            退回
          </Button>
          <Button size="small" class="ml-2" @click="openDelegateModal(row)">
            {{ $t('system.workflow.task.delegate') }}
          </Button>
        </template>
        <template v-else-if="isNotifyOrCarbonCopyTaskType(row.taskType)">
          <Button size="small" type="primary" @click="onCompleteNotifyOrCarbonCopy(row)">
            {{ $t('system.workflow.task.completeNotifyCarbon') }}
          </Button>
        </template>
        <Button
          size="small"
          :class="
            isApprovalTaskType(row.taskType) || isNotifyOrCarbonCopyTaskType(row.taskType) ? 'ml-2' : ''
          "
          @click="onViewDetail(row)"
        >
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
    <Modal
      v-model:open="returnModalVisible"
      :confirm-loading="returnModalLoading"
      title="退回"
      cancel-text="取消"
      ok-text="确定"
      @ok="handleReturnModalOk"
    >
      <div class="mb-4">
        <template v-if="returnFieldMode === 'Required'">
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
        </template>
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
  </Page>
</template>
