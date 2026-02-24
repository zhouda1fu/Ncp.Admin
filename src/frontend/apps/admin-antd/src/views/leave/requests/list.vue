<script lang="ts" setup>
import type { Recordable } from '@vben/types';
import type { OnActionClickParams } from '#/adapter/vxe-table';
import type { LeaveApi } from '#/api/system/leave';

import { Page, useVbenDrawer, useVbenModal } from '@vben/common-ui';
import { Plus } from '@vben/icons';

import { Button, message } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import {
  cancelLeaveRequest,
  getLeaveRequestList,
  submitLeaveRequest,
} from '#/api/system/leave';
import { $t } from '#/locales';

import { useColumns, useGridFormSchema } from './data';
import Form from './modules/form.vue';
import SubmitModal from './modules/submit-modal.vue';

const [FormDrawer, formDrawerApi] = useVbenDrawer({
  connectedComponent: Form,
  destroyOnClose: true,
});

const [SubmitModalRef, submitModalApi] = useVbenModal({
  connectedComponent: SubmitModal,
});

const [Grid, gridApi] = useVbenVxeGrid<LeaveApi.LeaveRequestItem>({
  formOptions: {
    schema: useGridFormSchema(),
    submitOnChange: true,
  },
  gridOptions: {
    columns: useColumns(onActionClick),
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
            status: formValues.status,
            leaveType: formValues.leaveType,
          };
          if (formValues.startDateFrom) params.startDateFrom = formValues.startDateFrom;
          if (formValues.startDateTo) params.startDateTo = formValues.startDateTo;
          const result = await getLeaveRequestList(params);
          return {
            items: result.items ?? [],
            total: result.total ?? 0,
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

function onActionClick(e: OnActionClickParams<LeaveApi.LeaveRequestItem>) {
  switch (e.code) {
    case 'edit':
      onEdit(e.row);
      break;
    case 'submit':
      onSubmit(e.row);
      break;
    case 'cancel':
      onCancel(e.row);
      break;
  }
}

function onEdit(row: LeaveApi.LeaveRequestItem) {
  formDrawerApi.setData(row).open();
}

function onSubmit(row: LeaveApi.LeaveRequestItem) {
  submitModalApi.setData({
    leaveRequestId: row.id,
    remark: '',
    onSuccess: onRefresh,
  }).open();
}

function onCancel(row: LeaveApi.LeaveRequestItem) {
  const hideLoading = message.loading({
    content: $t('leave.request.cancel') + '...',
    duration: 0,
    key: 'leave_cancel',
  });
  cancelLeaveRequest(row.id)
    .then(() => {
      message.success({ content: $t('common.success'), key: 'leave_cancel' });
      onRefresh();
    })
    .catch(() => {
      hideLoading();
    });
}

function onRefresh() {
  gridApi.query();
}

function onCreate() {
  formDrawerApi.setData({}).open();
}
</script>

<template>
  <Page auto-content-height>
    <FormDrawer @success="onRefresh" />
    <SubmitModalRef @success="onRefresh" />
    <Grid :table-title="$t('leave.request.list')">
      <template #toolbar-tools>
        <Button type="primary" class="inline-flex items-center gap-1" @click="onCreate">
          <Plus class="size-5 shrink-0" />
          {{ $t('ui.actionTitle.create', [$t('leave.request.name')]) }}
        </Button>
      </template>
    </Grid>
  </Page>
</template>
