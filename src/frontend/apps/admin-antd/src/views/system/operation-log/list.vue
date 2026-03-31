<script lang="ts" setup>
import type { Recordable } from '@vben/types';

import type { OperationLogApi } from '#/api/system/operation-log';

import { Page } from '@vben/common-ui';
import { Button, Modal, Tabs } from 'ant-design-vue';
import { computed, ref } from 'vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { getOperationLogList } from '#/api/system/operation-log';
import { $t } from '#/locales';

import { useColumns, useGridFormSchema } from './data';

const detailOpen = ref(false);
const activeTab = ref<'request' | 'response'>('request');
const currentRow = ref<OperationLogApi.OperationLogItem | null>(null);

const requestText = computed(() => prettyJson(currentRow.value?.requestBody));
const responseText = computed(() => prettyJson(currentRow.value?.responseBody));

const [Grid] = useVbenVxeGrid<OperationLogApi.OperationLogItem>({
  formOptions: {
    fieldMappingTime: [['createdAt', ['startTime', 'endTime']]],
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
          const result = await getOperationLogList({
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

function onViewDetail(row: OperationLogApi.OperationLogItem) {
  currentRow.value = row;
  activeTab.value = 'request';
  detailOpen.value = true;
}

function prettyJson(text?: string) {
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
      <Grid :table-title="$t('system.operationLog.list')">
        <template #action="{ row }">
          <Button size="small" @click="onViewDetail(row)">
            {{ $t('system.workflow.instance.detail') }}
          </Button>
        </template>
      </Grid>
    </Page>

    <Modal
      v-model:open="detailOpen"
      :title="$t('system.workflow.instance.detail')"
      :footer="null"
      width="860px"
      destroy-on-close
    >
      <Tabs v-model:activeKey="activeTab">
        <Tabs.TabPane key="request" tab="请求入参">
          <pre class="oplog-pre">{{ requestText }}</pre>
        </Tabs.TabPane>
        <Tabs.TabPane key="response" tab="响应出参">
          <pre class="oplog-pre">{{ responseText }}</pre>
        </Tabs.TabPane>
      </Tabs>
    </Modal>
  </div>
</template>

<style scoped>
.oplog-pre {
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
</style>
