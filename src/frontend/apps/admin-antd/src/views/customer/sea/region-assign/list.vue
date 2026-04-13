<script lang="ts" setup>
import type { OnActionClickParams } from '#/adapter/vxe-table';

import { ref } from 'vue';
import { Page } from '@vben/common-ui';

import { Select } from 'ant-design-vue';

import { $t } from '#/locales';

import { getAssignUsers } from '#/api/system/customer-sea-region';
import type { CustomerSeaRegionAssignApi } from '#/api/system/customer-sea-region';

import AssignDrawer from './modules/assign-drawer.vue';
import { useVbenVxeGrid } from '#/adapter/vxe-table';

const assignDrawerRef = ref<InstanceType<typeof AssignDrawer> | null>(null);

const deptOptions = [{ label: '营销中心', value: 'marketing-center' }];
const selectedDept = ref('marketing-center');

function onRefresh() {
  gridApi.query();
}

function onActionClick(e: OnActionClickParams<CustomerSeaRegionAssignApi.AssignUserListItem>) {
  if (e.code === 'assign') {
    assignDrawerRef.value?.open(e.row.userId);
  }
}

const [Grid, gridApi] = useVbenVxeGrid<CustomerSeaRegionAssignApi.AssignUserListItem>({
  gridOptions: {
    columns: [
      { field: 'deptName', title: '部门', width: 160 },
      {
        field: 'roleNames',
        title: '角色',
        width: 220,
        formatter: ({ row }: { row: CustomerSeaRegionAssignApi.AssignUserListItem }) =>
          row.roleNames?.join('、') || '-',
      },
      { field: 'name', title: '姓名', width: 140 },
      {
        align: 'center',
        cellRender: {
          attrs: {
            onClick: onActionClick,
            nameField: 'name',
            nameTitle: '姓名',
          },
          name: 'CellOperation',
          options: [{ code: 'assign', text: '分配片区' }],
        },
        field: 'operation',
        fixed: 'right',
        headerAlign: 'center',
        showOverflow: false,
        title: '操作',
        width: 200,
      },
      // 弹性列：保证表格铺满整行，避免右侧大空白
      { field: '_flex', title: '' },
    ],
    height: 'auto',
    keepSource: true,
    proxyConfig: {
      ajax: {
        query: async ({ page }: { page: { currentPage: number; pageSize: number } }) => {
          const result = await getAssignUsers({
            pageIndex: page.currentPage,
            pageSize: page.pageSize,
          });
          return { items: result.items ?? [], total: result.total ?? 0 };
        },
      },
    },
    rowConfig: { keyField: 'userId' },
    toolbarConfig: { custom: true, export: false, refresh: false, search: false, zoom: true },
  },
});
</script>

<template>
  <Page auto-content-height>
    <AssignDrawer ref="assignDrawerRef" @success="onRefresh" />
    <div class="mb-4 flex items-center gap-3 mx-4">
      <div class="text-sm text-muted-foreground">部门：</div>
      <Select
        v-model:value="selectedDept"
        :options="deptOptions"
        disabled
        style="min-width: 240px"
      />
    </div>
    <Grid :table-title="$t('customer.seaRegionAssign')" />
  </Page>
</template>

