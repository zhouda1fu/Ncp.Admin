<script lang="ts" setup>
import type { Recordable } from '@vben/types';

import type { SystemLogApi } from '#/api/system/system-log';

import { Page } from '@vben/common-ui';
import { Button, Modal, Tabs, Tag } from 'ant-design-vue';
import { computed, ref } from 'vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { getSystemLogDetail, getSystemLogList } from '#/api/system/system-log';

import { formatLevel, levelColor, useColumns, useGridFormSchema } from './data';

const detailOpen = ref(false);
const activeTab = ref<'exception' | 'properties' | 'basic'>('exception');
const currentRow = ref<SystemLogApi.SystemLogDetail | null>(null);

const propertiesText = computed(() => prettyJson(currentRow.value?.propertiesJson));

const [Grid] = useVbenVxeGrid<SystemLogApi.SystemLogItem>({
  formOptions: {
    fieldMappingTime: [['timestamp', ['startTime', 'endTime']]],
    schema: useGridFormSchema(),
    submitOnChange: true,
  },
  gridOptions: {
    columns: useColumns(),
    height: 'auto',
    keepSource: true,
    proxyConfig: {
      ajax: {
        query: async (
          { page }: { page: { currentPage: number; pageSize: number } },
          formValues: Recordable<any>,
        ) => {
          const result = await getSystemLogList({
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
      keyField: 'id',
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

async function onViewDetail(row: SystemLogApi.SystemLogItem) {
  currentRow.value = await getSystemLogDetail(row.id);
  activeTab.value = currentRow.value?.exception ? 'exception' : 'basic';
  detailOpen.value = true;
}

function prettyJson(text?: null | string) {
  if (!text) return '';
  try {
    const obj = JSON.parse(text);
    return JSON.stringify(obj, null, 2);
  } catch {
    return text;
  }
}
</script>

<template>
  <div>
    <Page auto-content-height>
      <Grid table-title="系统日志列表">
        <template #level="{ row }">
          <Tag :color="levelColor(row.level)">
            {{ formatLevel(row.level) }}
          </Tag>
        </template>
        <template #exception="{ row }">
          <Tag v-if="row.hasException" color="error">有</Tag>
          <span v-else>-</span>
        </template>
        <template #action="{ row }">
          <Button size="small" @click="onViewDetail(row)">详情</Button>
        </template>
      </Grid>
    </Page>

    <Modal
      v-model:open="detailOpen"
      title="系统日志详情"
      :footer="null"
      width="920px"
      destroy-on-close
    >
      <Tabs v-model:activeKey="activeTab">
        <Tabs.TabPane key="exception" tab="异常堆栈">
          <pre class="syslog-pre">{{ currentRow?.exception || '无异常堆栈' }}</pre>
        </Tabs.TabPane>
        <Tabs.TabPane key="properties" tab="结构化属性">
          <pre class="syslog-pre">{{ propertiesText || '无结构化属性' }}</pre>
        </Tabs.TabPane>
        <Tabs.TabPane key="basic" tab="基础信息">
          <div class="syslog-basic">
            <div><span>时间</span>{{ currentRow?.timestamp }}</div>
            <div><span>级别</span>{{ formatLevel(currentRow?.level) }}</div>
            <div><span>来源</span>{{ currentRow?.category }}</div>
            <div><span>消息</span>{{ currentRow?.message }}</div>
            <div><span>请求路径</span>{{ currentRow?.requestPath || '-' }}</div>
            <div><span>用户ID</span>{{ currentRow?.userId || '-' }}</div>
            <div><span>客户端IP</span>{{ currentRow?.clientIp || '-' }}</div>
            <div><span>TraceId</span>{{ currentRow?.traceId || '-' }}</div>
          </div>
        </Tabs.TabPane>
      </Tabs>
    </Modal>
  </div>
</template>

<style scoped>
.syslog-pre {
  max-height: 60vh;
  overflow: auto;
  padding: 12px;
  border: 1px solid var(--ant-color-border);
  border-radius: 6px;
  background: var(--ant-color-bg-container);
  white-space: pre-wrap;
  word-break: break-word;
  font-size: 12px;
  line-height: 1.6;
}

.syslog-basic {
  display: grid;
  grid-template-columns: 1fr;
  gap: 10px;
}

.syslog-basic div {
  display: grid;
  grid-template-columns: 96px minmax(0, 1fr);
  gap: 12px;
  align-items: start;
  word-break: break-word;
}

.syslog-basic span {
  color: var(--ant-color-text-secondary);
}
</style>
