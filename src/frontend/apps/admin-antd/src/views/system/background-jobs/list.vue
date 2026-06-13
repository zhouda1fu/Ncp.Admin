<script lang="ts" setup>
import type { BackgroundJobApi } from '#/api/system/background-job';
import type { TableColumnsType } from 'ant-design-vue';

import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';
import { IconifyIcon } from '@vben/icons';

import dayjs from 'dayjs';

import {
  Button,
  Input,
  message,
  Modal,
  Space,
  Table,
  Tag,
  Tooltip,
} from 'ant-design-vue';

import {
  getKnownRecurringJobs,
  getRecurringJobs,
  removeRecurringJob,
  triggerRecurringJob,
  upsertKnownRecurringJob,
} from '#/api/system/background-job';
import { PermissionCodes } from '#/constants/permission-codes';
import { useAccessStore } from '@vben/stores';

const router = useRouter();
const accessStore = useAccessStore();
const loading = ref(false);
const jobs = ref<BackgroundJobApi.RecurringJob[]>([]);
const knownJobs = ref<BackgroundJobApi.KnownRecurringJob[]>([]);
const cronDrafts = ref<Record<string, string>>({});

const canManage = computed(
  () => accessStore.accessCodes?.includes(PermissionCodes.BackgroundJobTrigger) ?? false,
);

const columns: TableColumnsType = [
  { dataIndex: 'displayName', fixed: 'left', title: '任务', width: 220 },
  { dataIndex: 'cron', title: 'Cron', width: 170 },
  { dataIndex: 'lastJobState', title: '最近状态', width: 110 },
  { dataIndex: 'lastExecution', title: '最近执行', width: 170 },
  { dataIndex: 'nextExecution', title: '下次执行', width: 170 },
  { dataIndex: 'queue', title: '队列', width: 100 },
  { dataIndex: 'actions', fixed: 'right', title: '操作', width: 320 },
];

function formatDate(value?: null | string) {
  return value ? dayjs(value).format('YYYY-MM-DD HH:mm:ss') : '-';
}

function stateColor(state?: null | string) {
  if (!state) return 'default';
  if (state === 'Succeeded') return 'green';
  if (state === 'Failed') return 'red';
  if (state === 'Processing') return 'blue';
  if (state === 'Enqueued') return 'cyan';
  return 'default';
}

async function loadJobs() {
  loading.value = true;
  try {
    const [jobResult, knownResult] = await Promise.all([
      getRecurringJobs(),
      getKnownRecurringJobs(),
    ]);
    jobs.value = jobResult;
    knownJobs.value = knownResult;
    cronDrafts.value = Object.fromEntries(
      knownResult.map((item) => [
        item.id,
        jobs.value.find((job) => job.id === item.id)?.cron || item.configuredCron,
      ]),
    );
  } finally {
    loading.value = false;
  }
}

async function onTrigger(row: BackgroundJobApi.RecurringJob) {
  await triggerRecurringJob(row.id);
  message.success(`已触发任务：${row.displayName}`);
  await loadJobs();
}

function onRemove(row: BackgroundJobApi.RecurringJob) {
  Modal.confirm({
    content: `停用后任务不会再按计划自动执行。系统内置任务可在下方重新启用。`,
    title: `停用 ${row.displayName}`,
    async onOk() {
      await removeRecurringJob(row.id);
      message.success('已停用定时任务');
      await loadJobs();
    },
  });
}

async function onEnable(job: BackgroundJobApi.KnownRecurringJob) {
  const cron = cronDrafts.value[job.id]?.trim();
  if (!cron) {
    message.warning('请填写 Cron 表达式');
    return;
  }

  const ok = await upsertKnownRecurringJob(job.id, cron);
  if (!ok) {
    message.error('该任务暂不支持在前端启用');
    return;
  }

  message.success('任务已启用或更新');
  await loadJobs();
}

function isRunning(id: string) {
  return jobs.value.some((job) => job.id === id);
}

function onOpenSettings(row: BackgroundJobApi.RecurringJob) {
  const path = row.settingsPath?.trim();
  if (!path) return;
  router.push(path);
}

onMounted(loadJobs);
</script>

<template>
  <Page auto-content-height content-class="flex min-h-0 flex-col">
    <div class="job-page">
      <section class="job-toolbar shrink-0">
        <div>
          <h2>定时任务</h2>
          <p>管理系统后台的 Hangfire 定时任务，支持查看计划、立即执行和停用任务。</p>
        </div>
        <Button :loading="loading" class="inline-flex items-center gap-1" @click="loadJobs">
          <IconifyIcon icon="mdi:refresh" class="size-4" />
          刷新
        </Button>
      </section>

      <div class="job-scroll min-h-0 flex-1 overflow-auto">
      <Table
        :columns="columns"
        :data-source="jobs"
        :loading="loading"
        :pagination="false"
        row-key="id"
        bordered
        size="middle"
        :scroll="{ x: 1180 }"
      >
        <template #bodyCell="{ column, record }: any">
          <template v-if="column.dataIndex === 'displayName'">
            <div class="job-name">
              <span>{{ record.displayName }}</span>
              <Tag v-if="record.isKnown" color="blue">内置</Tag>
            </div>
            <div class="job-id">{{ record.id }}</div>
            <div v-if="record.description" class="job-desc">{{ record.description }}</div>
            <div v-if="record.error" class="job-error">{{ record.error }}</div>
          </template>

          <template v-else-if="column.dataIndex === 'lastJobState'">
            <Tag :color="stateColor(record.lastJobState)">
              {{ record.lastJobState || '未执行' }}
            </Tag>
          </template>

          <template v-else-if="column.dataIndex === 'lastExecution'">
            {{ formatDate(record.lastExecution) }}
          </template>

          <template v-else-if="column.dataIndex === 'nextExecution'">
            {{ formatDate(record.nextExecution) }}
          </template>

          <template v-else-if="column.dataIndex === 'actions'">
            <Space>
              <Tooltip title="立即执行">
                <Button
                  :disabled="!canManage"
                  class="inline-flex items-center gap-1"
                  size="small"
                  type="primary"
                  @click="onTrigger(record as BackgroundJobApi.RecurringJob)"
                >
                  <IconifyIcon icon="mdi:play" class="size-4" />
                  执行
                </Button>
              </Tooltip>
              <Tooltip title="停用定时计划">
                <Button
                  :disabled="!canManage"
                  class="inline-flex items-center gap-1"
                  danger
                  size="small"
                  @click="onRemove(record as BackgroundJobApi.RecurringJob)"
                >
                  <IconifyIcon icon="mdi:delete-outline" class="size-4" />
                  停用
                </Button>
              </Tooltip>
              <Tooltip v-if="record.settingsPath" title="通知接收人与提醒频率">
                <Button
                  :disabled="!canManage"
                  class="inline-flex items-center gap-1"
                  size="small"
                  @click="onOpenSettings(record as BackgroundJobApi.RecurringJob)"
                >
                  <IconifyIcon icon="mdi:cog-outline" class="size-4" />
                  设置
                </Button>
              </Tooltip>
            </Space>
          </template>
        </template>
      </Table>

      <section class="known-jobs">
        <div class="section-title">
          <h3>系统内置任务</h3>
          <p>被移除的内置任务可以在这里重新启用，也可以调整本次注册到 Hangfire 的 Cron。</p>
        </div>
        <div class="known-grid">
          <div v-for="job in knownJobs" :key="job.id" class="known-item">
            <div class="known-main">
              <div class="known-title">
                <span>{{ job.displayName }}</span>
                <Tag :color="isRunning(job.id) ? 'green' : 'default'">
                  {{ isRunning(job.id) ? '运行中' : '未注册' }}
                </Tag>
              </div>
              <p>{{ job.description }}</p>
            </div>
            <div class="known-actions">
              <Input
                v-model:value="cronDrafts[job.id]"
                :disabled="!canManage"
                class="cron-input"
                placeholder="Cron"
              />
              <Button
                :disabled="!canManage"
                class="inline-flex items-center gap-1"
                type="primary"
                @click="onEnable(job)"
              >
                <IconifyIcon icon="mdi:content-save-outline" class="size-4" />
                保存
              </Button>
              <Button
                v-if="job.settingsPath"
                :disabled="!canManage"
                class="inline-flex items-center gap-1"
                @click="router.push(job.settingsPath!)"
              >
                <IconifyIcon icon="mdi:cog-outline" class="size-4" />
                设置
              </Button>
            </div>
          </div>
        </div>
      </section>
      </div>
    </div>
  </Page>
</template>

<style scoped>
.job-page {
  display: flex;
  flex: 1;
  flex-direction: column;
  gap: 16px;
  min-height: 0;
  color: hsl(var(--foreground));
}

.job-scroll {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.job-toolbar,
.section-title {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
}

.job-toolbar h2,
.section-title h3 {
  margin: 0;
  font-size: 18px;
  font-weight: 600;
  color: hsl(var(--foreground));
}

.job-toolbar p,
.section-title p,
.known-main p {
  margin: 4px 0 0;
  color: hsl(var(--muted-foreground));
}

.job-name,
.known-title {
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: 600;
  color: hsl(var(--foreground));
}

.job-id,
.job-desc,
.job-error {
  margin-top: 4px;
  font-size: 12px;
}

.job-id {
  color: hsl(var(--muted-foreground));
}

.job-desc {
  color: hsl(var(--muted-foreground) / 0.85);
}

.job-error {
  color: hsl(var(--destructive));
}

.known-jobs {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.known-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 12px;
}

.known-item {
  display: flex;
  flex-direction: column;
  align-items: stretch;
  gap: 12px;
  padding: 14px 16px;
  border: 1px solid hsl(var(--border));
  border-radius: 8px;
  background: hsl(var(--card));
  color: hsl(var(--card-foreground));
}

.known-main {
  min-width: 0;
}

.known-actions {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 8px;
}

.cron-input {
  flex: 1;
  min-width: 120px;
}

@media (max-width: 1200px) {
  .known-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 768px) {
  .job-toolbar,
  .section-title {
    align-items: stretch;
    flex-direction: column;
  }

  .known-grid {
    grid-template-columns: 1fr;
  }

  .known-actions {
    width: 100%;
  }

  .cron-input {
    width: 100%;
  }
}

.job-page :deep(.ant-btn-default) {
  color: hsl(var(--foreground) / 90%);
  border-color: hsl(var(--border));
}

.dark .job-page :deep(.ant-btn-default) {
  color: hsl(var(--foreground) / 95%);
  background: hsl(var(--accent));
}

.job-page :deep(.ant-btn-default:hover) {
  color: hsl(var(--foreground));
  border-color: hsl(var(--border));
  background: hsl(var(--accent-hover));
}
</style>
