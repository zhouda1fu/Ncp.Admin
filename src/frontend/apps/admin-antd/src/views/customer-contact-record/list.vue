<script lang="ts" setup>
import type { Recordable } from '@vben/types';
import type { OnActionClickParams } from '#/adapter/vxe-table';

import { computed } from 'vue';
import { useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';
import { useAccessStore } from '@vben/stores';
import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { Plus } from '@vben/icons';
import { Button } from 'ant-design-vue';

import { $t } from '#/locales';

import { getCustomerContactRecordList } from '#/api/system/customer-contact-record';
import type { CustomerContactRecordApi } from '#/api/system/customer-contact-record';
import { PermissionCodes } from '#/constants/permission-codes';
import {
  formatCustomerContactRecordStatus,
  formatCustomerContactRecordType,
} from '#/utils/customer-contact-record-display';

const router = useRouter();
const accessStore = useAccessStore();

const canView = computed(
  () =>
    accessStore.accessCodes?.includes(PermissionCodes.CustomerContactRecordView) ??
    false,
);
const canCreate = computed(
  () =>
    accessStore.accessCodes?.includes(PermissionCodes.CustomerContactRecordCreate) ??
    false,
);
const canEdit = computed(
  () =>
    accessStore.accessCodes?.includes(PermissionCodes.CustomerContactRecordEdit) ?? false,
);

function formatDateTime(v?: string) {
  if (!v) return '-';
  const d = new Date(v);
  return Number.isNaN(d.getTime()) ? v : d.toLocaleString('zh-CN');
}

function toEdit(record: CustomerContactRecordApi.ListItem) {
  // 联系记录编辑入口（同一个页面同时支持查看/编辑）
  router.push(`/customer/${record.customerId}/contact-records/${record.id}/edit`);
}

function onActionClick(e: OnActionClickParams<CustomerContactRecordApi.ListItem>) {
  if (e.code === 'view' || e.code === 'edit') toEdit(e.row);
}

const formSchema = [
  {
    component: 'Input',
    componentProps: { class: 'w-full' },
    fieldName: 'keyword',
    label: '关键字',
  },
  {
    component: 'Select',
    componentProps: {
      allowClear: true,
      class: 'w-full',
      options: [
        { label: '电话', value: 1 },
        { label: '出差', value: 2 },
        { label: '微信', value: 3 },
        { label: '其他', value: 4 },
      ],
    },
    fieldName: 'recordTypeId',
    label: '类型',
  },
  {
    component: 'Select',
    componentProps: {
      allowClear: true,
      class: 'w-full',
      options: [
        { label: '待选择', value: 0 },
        { label: '有效联系', value: 1 },
        { label: '无效联系', value: 2 },
      ],
    },
    fieldName: 'statusId',
    label: '状态',
  },
];

const [Grid] = useVbenVxeGrid<CustomerContactRecordApi.ListItem>({
  formOptions: {
    schema: formSchema as any,
    submitOnChange: true,
  },
  gridOptions: {
    columns: [
      {
        field: 'recordType',
        title: '类型',
        width: 100,
        formatter: ({ row }: { row: CustomerContactRecordApi.ListItem }) =>
          formatCustomerContactRecordType(row.recordType),
      },
      { field: 'regionName', title: '所在地区', width: 160 },
      { field: 'customerName', title: '客户名称', width: 220 },
      {
        field: 'industryNames',
        title: '所属行业',
        width: 160,
        formatter: ({ row }: { row: CustomerContactRecordApi.ListItem }) =>
          (row.industryNames ?? []).join('、') || '-',
      },
      { field: 'ownerName', title: '负责人', width: 120 },
      {
        field: 'status',
        title: '状态',
        width: 100,
        formatter: ({ row }: { row: CustomerContactRecordApi.ListItem }) =>
          formatCustomerContactRecordStatus(row.status),
      },
      {
        field: 'recordAt',
        title: '拜访日期',
        width: 170,
        formatter: ({ row }: { row: CustomerContactRecordApi.ListItem }) =>
          formatDateTime(row.recordAt),
      },
      {
        field: 'nextVisitAt',
        title: '下次拜访',
        width: 170,
        formatter: ({ row }: { row: CustomerContactRecordApi.ListItem }) =>
          formatDateTime(row.nextVisitAt),
      },
      {
        align: 'right',
        cellRender: {
          attrs: {
            nameField: 'customerName',
            nameTitle: '客户名称',
            onClick: onActionClick,
          },
          name: 'CellOperation',
          options: [
            {
              code: 'view',
              text: $t('customer.view'),
              show: (_row: CustomerContactRecordApi.ListItem) => canView.value,
            },
            {
              code: 'edit',
              text: $t('customer.edit'),
              show: (_row: CustomerContactRecordApi.ListItem) => canEdit.value,
            },
          ],
        },
        field: 'operation',
        fixed: 'right',
        headerAlign: 'center',
        showOverflow: false,
        title: '操作',
        width: 220,
      },
      { field: '_flex', title: '' },
    ],
    height: 'auto',
    keepSource: true,
    proxyConfig: {
      ajax: {
        query: async (
          { page }: { page: { currentPage: number; pageSize: number } },
          formValues: Recordable<any>,
        ) => {
          if (!canView.value)
            return {
              items: [],
              total: 0,
            };

          const params: Recordable<any> = {
            pageIndex: page.currentPage,
            pageSize: page.pageSize,
            keyword: formValues.keyword?.trim() || undefined,
            recordTypeId: formValues.recordTypeId,
            statusId: formValues.statusId,
          };

          const result = await getCustomerContactRecordList(params);
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

function onCreate() {
  // 这里需要先选择客户，因此先跳转客户列表选择目标客户
  router.push('/customer/list');
}
</script>

<template>
  <Page auto-content-height>
    <Grid :table-title="'客户联络'">
      <template #toolbar-tools>
        <Button
          v-if="canCreate"
          type="primary"
          class="inline-flex items-center gap-1"
          @click="onCreate"
        >
          <Plus class="size-5 shrink-0" />
          {{ $t('customer.addRecordTitle') }}
        </Button>
      </template>
    </Grid>
  </Page>
</template>

