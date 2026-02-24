<script lang="ts" setup>
import type { Recordable } from '@vben/types';
import type { AttendanceApi } from '#/api/system/attendance';

import { Page, useVbenModal } from '@vben/common-ui';

import { Button, message } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { checkIn, checkOut, getAttendanceRecordList } from '#/api/system/attendance';
import { $t } from '#/locales';

import { useColumns, useGridFormSchema } from './data';

const [Grid, gridApi] = useVbenVxeGrid<AttendanceApi.AttendanceRecordItem>({
  formOptions: {
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
          const params: Recordable<any> = {
            pageIndex: page.currentPage,
            pageSize: page.pageSize,
            userId: formValues.userId,
            dateFrom: formValues.dateFrom,
            dateTo: formValues.dateTo,
          };
          const result = await getAttendanceRecordList(params);
          return {
            items: result.items ?? [],
            total: result.total ?? 0,
          };
        },
      },
    },
    rowConfig: { keyField: 'id' },
    toolbarConfig: {
      custom: true,
      export: false,
      refresh: true,
      search: true,
      zoom: true,
    },
  },
});

function onRefresh() {
  gridApi.query();
}

async function onCheckIn() {
  try {
    await checkIn({ source: 2 });
    message.success($t('common.success'));
    onRefresh();
  } catch {
    // error handled by request
  }
}

async function onCheckOut() {
  try {
    await checkOut();
    message.success($t('common.success'));
    onRefresh();
  } catch {
    // error handled by request
  }
}
</script>

<template>
  <Page auto-content-height>
    <Grid :table-title="$t('attendance.record.list')">
      <template #toolbar-tools>
        <Button class="mr-2" @click="onCheckIn">
          {{ $t('attendance.record.checkIn') }}
        </Button>
        <Button @click="onCheckOut">
          {{ $t('attendance.record.checkOut') }}
        </Button>
      </template>
    </Grid>
  </Page>
</template>
