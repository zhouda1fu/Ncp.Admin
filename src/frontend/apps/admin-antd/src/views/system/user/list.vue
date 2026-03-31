<script lang="ts" setup>
import type { Recordable } from '@vben/types';

import type { OnActionClickParams } from '#/adapter/vxe-table';
import type { SystemUserApi } from '#/api/system/user';

import { Page } from '@vben/common-ui';
import { IconifyIcon, Plus } from '@vben/icons';

import { useAccessStore } from '@vben/stores';

import { Button, message, Modal } from 'ant-design-vue';
import { ref } from 'vue';
import { useRouter } from 'vue-router';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import {
  deleteUser,
  downloadUserImportTemplate,
  exportUsersExcel,
  getUserList,
  importUsersExcel,
  updateUser,
} from '#/api/system/user';
import { PermissionCodes } from '#/constants/permission-codes';
import { $t } from '#/locales';

import { useColumns, useGridFormSchema } from './data';

const router = useRouter();
const accessStore = useAccessStore();
const canExportUsers = () => accessStore.accessCodes?.includes(PermissionCodes.UserExport) ?? false;
const canImportUsers = () => accessStore.accessCodes?.includes(PermissionCodes.UserImport) ?? false;

const importFileInputRef = ref<HTMLInputElement | null>(null);
const [Grid, gridApi] = useVbenVxeGrid<SystemUserApi.SystemUser>({
  formOptions: {
    schema: useGridFormSchema(),
    submitOnChange: true,
  },
  gridOptions: {
    columns: useColumns(onActionClick, onStatusChange),
    height: 'auto',
    keepSource: true,
    proxyConfig: {
      ajax: {
        query: async ({ page }: { page: { currentPage: number; pageSize: number } }, formValues: Recordable<any>) => {
          const result = await getUserList({
            pageIndex: page.currentPage, // 后端期望 pageIndex（从1开始），而不是 page
            pageSize: page.pageSize,
            countTotal: true, // 需要总数用于分页显示
            ...formValues,
          });
          // vxe-table 根据全局配置 response: { result: 'items', total: 'total' } 读取数据
          return {
            items: result.items,
            total: result.total,
          };
        },
      },
    },
    rowConfig: {
      keyField: 'userId',
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

function onActionClick(e: OnActionClickParams<SystemUserApi.SystemUser>) {
  switch (e.code) {
    case 'delete': {
      onDelete(e.row);
      break;
    }
    case 'edit': {
      onEdit(e.row);
      break;
    }
  }
}

/**
 * 将Antd的Modal.confirm封装为promise，方便在异步函数中调用。
 * @param content 提示内容
 * @param title 提示标题
 */
function confirm(content: string, title: string) {
  return new Promise((resolve, reject) => {
    Modal.confirm({
      content,
      onCancel() {
        reject(new Error('已取消'));
      },
      onOk() {
        resolve(true);
      },
      title,
    });
  });
}

/**
 * 状态开关即将改变
 * @param newStatus 期望改变的状态值（0或1）
 * @param row 行数据
 * @returns 返回false则中止改变，返回其他值（undefined、true）则允许改变
 */
async function onStatusChange(
  newStatus: 0 | 1,
  row: SystemUserApi.SystemUser,
) {
  const status: Recordable<string> = {
    0: '禁用',
    1: '启用',
  };
  try {
    await confirm(
      `你要将${row.name}的状态切换为 【${status[newStatus.toString()]}】 吗？`,
      `切换状态`,
    );
    // 通过更新用户信息来切换状态，保留离职状态
    await updateUser(row.userId, {
      name: row.name,
      email: row.email,
      phone: row.phone || '',
      realName: row.realName || '',
      status: newStatus,
      gender: row.gender || '',
      age: row.age || 0,
      birthDate: row.birthDate,
      deptId: row.deptId || '0',
      deptName: row.deptName || '',
      password: '', // 不更新密码
      isResigned: row.isResigned ?? false,
      resignedTime: row.resignedTime || undefined,
    });
    // 刷新列表以获取最新状态
    onRefresh();
    return true;
  } catch {
    return false;
  }
}

function onEdit(row: SystemUserApi.SystemUser) {
  router.push(`/system/user/${row.userId}/edit`);
}

function onDelete(row: SystemUserApi.SystemUser) {
  const hideLoading = message.loading({
    content: $t('ui.actionMessage.deleting', [row.name]),
    duration: 0,
    key: 'action_process_msg',
  });
  deleteUser(row.userId)
    .then(() => {
      message.success({
        content: $t('ui.actionMessage.deleteSuccess', [row.name]),
        key: 'action_process_msg',
      });
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
  router.push('/system/user/create');
}

async function onExportExcel() {
  try {
    const formValues = (await gridApi.formApi?.getValues?.()) ?? {};
    await exportUsersExcel({
      keyword: formValues.keyword,
      status: formValues.status,
      isResigned: formValues.isResigned,
    });
    message.success($t('system.user.exportSuccess'));
  } catch {
    /* 错误已由拦截器提示 */
  }
}

async function onDownloadTemplate() {
  try {
    await downloadUserImportTemplate();
    message.success($t('system.user.exportSuccess'));
  } catch {
    /* 错误已由拦截器提示 */
  }
}

function onPickImportFile() {
  importFileInputRef.value?.click();
}

async function onImportFileChange(e: Event) {
  const input = e.target as HTMLInputElement;
  const file = input.files?.[0];
  input.value = '';
  if (!file) return;
  try {
    const result = await importUsersExcel(file);
    const failCount = result.errors?.length ?? 0;
    if (failCount === 0) {
      message.success($t('system.user.importSuccess', [String(result.successCount)]));
    } else {
      const detail = (result.errors ?? [])
        .map((x) => `第 ${x.rowNumber} 行：${x.message}`)
        .join('\n');
      Modal.warning({
        title: $t('system.user.importPartial', [String(result.successCount), String(failCount)]),
        content: `${$t('system.user.importErrorsTitle')}\n${detail}`,
        width: 560,
      });
    }
    gridApi.query();
  } catch {
    /* 错误已由拦截器提示 */
  }
}
</script>
<template>
  <Page auto-content-height>
    <Grid :table-title="$t('system.user.list')">
      <template #toolbar-tools>
        <Button
          v-if="canExportUsers()"
          class="inline-flex items-center gap-1"
          @click="onExportExcel"
        >
          <IconifyIcon icon="mdi:tray-arrow-down" class="size-5 shrink-0" />
          {{ $t('system.user.exportExcel') }}
        </Button>
        <Button
          v-if="canImportUsers()"
          class="inline-flex items-center gap-1"
          @click="onDownloadTemplate"
        >
          <IconifyIcon icon="mdi:file-download-outline" class="size-5 shrink-0" />
          {{ $t('system.user.downloadImportTemplate') }}
        </Button>
        <Button
          v-if="canImportUsers()"
          class="inline-flex items-center gap-1"
          @click="onPickImportFile"
        >
          <IconifyIcon icon="mdi:upload" class="size-5 shrink-0" />
          {{ $t('system.user.importExcel') }}
        </Button>
        <input
          ref="importFileInputRef"
          type="file"
          accept=".xlsx,.xlsm"
          class="hidden"
          @change="onImportFileChange"
        />
        <Button type="primary" class="inline-flex items-center gap-1" @click="onCreate">
          <Plus class="size-5 shrink-0" />
          {{ $t('ui.actionTitle.create', [$t('system.user.name')]) }}
        </Button>
      </template>
    </Grid>
  </Page>
</template>
