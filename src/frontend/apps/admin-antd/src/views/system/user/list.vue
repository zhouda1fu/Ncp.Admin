<script lang="ts" setup>
import type { Recordable } from '@vben/types';

import type { OnActionClickParams } from '#/adapter/vxe-table';
import type { SystemUserApi } from '#/api/system/user';

import { Page } from '@vben/common-ui';
import { IconifyIcon, Plus } from '@vben/icons';

import { useAccessStore } from '@vben/stores';

import { Button, message, Modal, Tag } from 'ant-design-vue';
import { computed, onMounted, ref, nextTick } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import {
  deleteUser,
  downloadUserImportTemplate,
  exportUsersExcel,
  getUserColumnFacets,
  getUserList,
  importUsersExcel,
  updateUser,
} from '#/api/system/user';
import { PermissionCodes } from '#/constants/permission-codes';
import { useListReturnState } from '#/composables/use-list-return-state';
import {
  collectRouteStringParams,
  parseBooleanQuery,
  parseNumberQuery,
  readStringQuery,
} from '#/utils/list-return-state';
import { $t } from '#/locales';

import { useColumns, useGridFormSchema } from './data';
import ColumnFilterModal from './modules/column-filter-modal.vue';
import UserChangeHistoryModal from './modules/user-change-history-modal.vue';

const LIST_PATH = '/system/user';
const SEARCH_KEYS = ['keyword', 'status'] as const;

const router = useRouter();
const route = useRoute();
const accessStore = useAccessStore();
const hasPermission = (code: string) =>
  accessStore.accessCodes?.includes(code) ?? false;
const canEditUser = () => hasPermission(PermissionCodes.UserEdit);
const canDeleteUser = () => hasPermission(PermissionCodes.UserDelete);
const canViewUser = () => hasPermission(PermissionCodes.UserView);
const canExportUsers = () =>
  accessStore.accessCodes?.includes(PermissionCodes.UserExport) ?? false;
const canImportUsers = () =>
  accessStore.accessCodes?.includes(PermissionCodes.UserImport) ?? false;
const canViewUserChangeHistory = computed(
  () => accessStore.accessCodes?.includes(PermissionCodes.UserChangeHistoryView) ?? false,
);

/** 与表单页 return 参数配合：恢复列表当前页/每页条数及搜索条件 */
function restoreHeaderFacetsFromRoute() {
  const deptNames = readStringQuery(route.query, 'filterDeptNames');
  const roleNames = readStringQuery(route.query, 'filterRoleNames');
  if (deptNames) {
    headerFacetSelections.value.deptName = deptNames.split('|').filter(Boolean);
  }
  if (roleNames) {
    headerFacetSelections.value.roles = roleNames.split('|').filter(Boolean);
  }
}

const HEADER_FACET_CONFIG: Record<string, { queryParam: string; facetColumn: string }> = {
  deptName: { queryParam: 'filterDeptNames', facetColumn: 'DeptName' },
  roles: { queryParam: 'filterRoleNames', facetColumn: 'RoleName' },
};

function isHeaderFacetField(field: string | undefined): field is 'deptName' | 'roles' {
  return field === 'deptName' || field === 'roles';
}

const headerFacetSelections = ref<Record<string, string[]>>({});
restoreHeaderFacetsFromRoute();
const columnFilterOpen = ref(false);
const columnFacetLoading = ref(false);
const columnFacetRaw = ref<SystemUserApi.UserColumnFacet[]>([]);
const activeFacetGridField = ref<string | null>(null);
const activeFacetColumnTitle = ref('');

function buildListFilterParams(excludeGridField?: string): Recordable<any> {
  const params: Recordable<any> = {};
  for (const [field, meta] of Object.entries(HEADER_FACET_CONFIG)) {
    if (field === excludeGridField) continue;
    const selected = headerFacetSelections.value[field];
    if (!selected?.length) continue;
    params[meta.queryParam] = selected;
  }
  return params;
}

function headerFacetHasSelection(field: string | undefined): boolean {
  if (!field) return false;
  return (headerFacetSelections.value[field]?.length ?? 0) > 0;
}

const columnFilterOptions = computed(() => {
  const field = activeFacetGridField.value;
  if (!field) return [];
  return columnFacetRaw.value.map((x) => ({
    value: x.value,
    label: `${x.displayLabel ?? x.value}(${x.count})`,
  }));
});

const columnFilterModalTitle = computed(() => {
  const t = activeFacetColumnTitle.value;
  if (!t) return $t('system.user.columnFilterTitle');
  return `${$t('system.user.columnFilterTitlePrefix')}-${t}`;
});

const activeColumnAppliedValues = computed(() => {
  const f = activeFacetGridField.value;
  if (!f) return [];
  return headerFacetSelections.value[f] ?? [];
});

const importFileInputRef = ref<HTMLInputElement | null>(null);

function buildFacetReturnFields(): Record<string, string> {
  const extra: Record<string, string> = {};
  const depts = headerFacetSelections.value.deptName;
  if (depts?.length) extra.filterDeptNames = depts.join('|');
  const roles = headerFacetSelections.value.roles;
  if (roles?.length) extra.filterRoleNames = roles.join('|');
  return extra;
}

const {
  shouldDeferGridAutoLoad,
  trackPage,
  buildReturnQuery,
  restoreOnMount,
  clearRestoreKey,
  pagerConfig,
} = useListReturnState({
  route,
  listPath: LIST_PATH,
  searchKeys: SEARCH_KEYS,
  extraRestoreQueryKeys: ['filterDeptNames', 'filterRoleNames', 'isResigned'],
  onBeforeRestoreFromRoute: restoreHeaderFacetsFromRoute,
  getExtraReturnFields: buildFacetReturnFields,
  parseRouteToSearchValues: (query) => {
    const searchValues: Record<string, unknown> = collectRouteStringParams(query, SEARCH_KEYS);
    const statusRaw = readStringQuery(query, 'status');
    const statusNum = parseNumberQuery(statusRaw);
    if (statusNum !== undefined) searchValues.status = statusNum;
    else if (statusRaw !== undefined) searchValues.status = statusRaw;
    const isResignedRaw = readStringQuery(query, 'isResigned');
    if (isResignedRaw !== undefined) {
      const isResigned = parseBooleanQuery(isResignedRaw);
      if (isResigned !== undefined) searchValues.isResigned = isResigned;
    } else {
      searchValues.isResigned = false;
    }
    return searchValues;
  },
});

async function buildUserListReturnQuery(): Promise<Record<string, string>> {
  const formValues = (await gridApi.formApi?.getValues?.()) ?? {};
  const base = await buildReturnQuery(gridApi);
  if (formValues.isResigned === true || formValues.isResigned === false) {
    return { ...base, isResigned: String(formValues.isResigned) };
  }
  return base;
}

const [Grid, gridApi] = useVbenVxeGrid<SystemUserApi.SystemUser>({
  formOptions: {
    schema: useGridFormSchema(),
    submitOnChange: true,
  },
  gridEvents: {
    'cell-dblclick': ({ column, row }: any) => {
      if (column?.type === 'checkbox' || column?.field === 'operation') return;
      onRowDblclick(row);
    },
  } as any,
  gridOptions: {
    columns: useColumns(onActionClick, onStatusChange, onResignedChange, {
      canDelete: canDeleteUser,
      canEdit: canEditUser,
    }),
    rowClassName: () =>
      canEditUser() || canViewUser() ? 'system-user-row-clickable' : '',
    height: 'auto',
    keepSource: true,
    pagerConfig: {
      currentPage: pagerConfig.currentPage,
      pageSize: pagerConfig.pageSize,
    },
    proxyConfig: {
      autoLoad: !shouldDeferGridAutoLoad,
      ajax: {
        query: async ({ page }: { page: { currentPage: number; pageSize: number } }, formValues: Recordable<any>) => {
          trackPage(page);
          const result = await getUserList({
            pageIndex: page.currentPage, // 后端期望 pageIndex（从1开始），而不是 page
            pageSize: page.pageSize,
            countTotal: true, // 需要总数用于分页显示
            ...formValues,
            ...buildListFilterParams(),
          });
          // vxe-table 根据全局配置 response: { result: 'items', total: 'total' } 读取数据
          return {
            items: result.items,
            total: result.total,
          };
        },
      },
    },
    checkboxConfig: { highlight: true },
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

async function openColumnFilterModal(column: { field?: string; title?: unknown }) {
  const field = column.field;
  if (!isHeaderFacetField(field)) return;
  activeFacetGridField.value = field;
  activeFacetColumnTitle.value = String(column.title ?? '');
  columnFilterOpen.value = true;
  columnFacetLoading.value = true;
  columnFacetRaw.value = [];
  const cfg = HEADER_FACET_CONFIG[field];
  if (!cfg) return;
  try {
    const formValues = (await gridApi.formApi?.getValues?.()) ?? {};
    const list = await getUserColumnFacets(
      { ...formValues, ...buildListFilterParams(field) },
      cfg.facetColumn,
    );
    columnFacetRaw.value = Array.isArray(list) ? list : [];
  } catch {
    columnFacetRaw.value = [];
  } finally {
    columnFacetLoading.value = false;
  }
}

function onColumnFilterApply(selected: string[]) {
  const field = activeFacetGridField.value;
  if (field) {
    headerFacetSelections.value = {
      ...headerFacetSelections.value,
      [field]: selected,
    };
  }
  gridApi.query();
}

function onColumnFilterClear() {
  const field = activeFacetGridField.value;
  if (!field) return;
  const next = { ...headerFacetSelections.value };
  delete next[field];
  headerFacetSelections.value = next;
  gridApi.query();
}

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
async function onStatusChange(newStatus: 0 | 1, row: SystemUserApi.SystemUser) {
  const status: Recordable<string> = {
    0: '禁用',
    1: '启用',
  };
  try {
    await confirm(
      `你要将${row.name}的状态切换为 【${status[newStatus.toString()]}】 吗？`,
      `切换状态`,
    );
    await updateUser(
      row.userId,
      buildUserUpdatePayload(row, {
        status: newStatus,
      }),
    );
    onRefresh();
    return true;
  } catch {
    return false;
  }
}

async function onResignedChange(
  isResigned: boolean,
  row: SystemUserApi.SystemUser,
) {
  try {
    await confirm(
      `你要将${row.name}切换为【${isResigned ? '已离职' : '在职'}】吗？`,
      '切换离职状态',
    );
    await updateUser(
      row.userId,
      buildUserUpdatePayload(row, {
        isResigned,
        resignedTime: isResigned
          ? (row.resignedTime ?? new Date().toISOString())
          : undefined,
        status: isResigned ? 0 : row.status,
      }),
    );
    onRefresh();
    return true;
  } catch {
    return false;
  }
}

function buildUserUpdatePayload(
  row: SystemUserApi.SystemUser,
  patch: Partial<Parameters<typeof updateUser>[1]> = {},
): Parameters<typeof updateUser>[1] {
  return {
    name: row.name,
    email: row.email,
    phone: row.phone || '',
    realName: row.realName || '',
    status: row.status,
    gender: row.gender || '',
    age: row.age || 0,
    birthDate: row.birthDate,
    deptId: row.deptId || '0',
    deptName: row.deptName || '',
    password: '',
    idCardNumber: row.idCardNumber || '',
    address: row.address || '',
    education: row.education || '',
    graduateSchool: row.graduateSchool || '',
    avatarUrl: row.avatarUrl || '',
    notOrderMeal: row.notOrderMeal ?? false,
    wechatGuid: row.wechatGuid || '',
    isResigned: row.isResigned ?? false,
    resignedTime: row.resignedTime || undefined,
    ...patch,
  };
}

function onEdit(row: SystemUserApi.SystemUser) {
  clearRestoreKey();
  void buildUserListReturnQuery().then((query) => {
    void router.push({
      path: `/system/user/${row.userId}/edit`,
      query,
    });
  });
}

function onView(row: SystemUserApi.SystemUser) {
  clearRestoreKey();
  void buildUserListReturnQuery().then((query) => {
    void router.push({
      path: `/system/user/${row.userId}/view`,
      query,
    });
  });
}

function onRowDblclick(row: SystemUserApi.SystemUser) {
  if (canEditUser()) {
    onEdit(row);
    return;
  }
  if (canViewUser()) {
    onView(row);
  }
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
  void buildUserListReturnQuery().then((query) => {
    void router.push({
      path: '/system/user/create',
      query,
    });
  });
}

async function onExportExcel() {
  try {
    const formValues = (await gridApi.formApi?.getValues?.()) ?? {};
    await exportUsersExcel({
      keyword: formValues.keyword,
      status: formValues.status,
      isResigned: formValues.isResigned,
      ...buildListFilterParams(),
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

function onClearHeaderFilters() {
  headerFacetSelections.value = {};
  columnFilterOpen.value = false;
  void nextTick(() => {
    gridApi.query();
  });
}

const userChangeHistoryModalOpen = ref(false);
const userChangeHistoryRow = ref<SystemUserApi.SystemUser | null>(null);

function getSelectedUserRows(): SystemUserApi.SystemUser[] {
  return (
    (((gridApi as any)?.grid?.getCheckboxRecords?.() ?? []) as SystemUserApi.SystemUser[]) ?? []
  );
}

function openUserChangeHistory() {
  const rows = getSelectedUserRows();
  if (rows.length !== 1) {
    message.warning($t('system.user.changeHistorySelectOne'));
    return;
  }
  userChangeHistoryRow.value = rows[0]!;
  userChangeHistoryModalOpen.value = true;
}

onMounted(async () => {
  await restoreOnMount(gridApi);
});
</script>
<template>
  <Page auto-content-height>
    <Grid :table-title="$t('system.user.list')">
      <template #columnFilterHeader="{ column }">
        <div
          v-if="isHeaderFacetField(column.field)"
          class="inline-flex w-full min-w-0 items-center justify-center gap-0.5"
        >
          <span class="truncate">{{ column.title }}</span>
          <button
            type="button"
            class="inline-flex shrink-0 items-center rounded p-0.5 hover:bg-accent"
            :class="{ 'text-primary': headerFacetHasSelection(column.field) }"
            @click.stop="openColumnFilterModal(column)"
          >
            <IconifyIcon icon="mdi:filter-variant" class="size-4" />
          </button>
        </div>
        <span v-else>{{ column.title }}</span>
      </template>
      <template #userListUserName="{ row }">
        <div class="inline-flex flex-wrap items-center gap-1.5">
          <span>{{ (row as SystemUserApi.SystemUser).name }}</span>
          <Tag
            v-if="(row as SystemUserApi.SystemUser).isResigned"
            color="default"
            class="m-0 shrink-0 rounded px-1.5 py-0 text-xs leading-5"
          >
            {{ $t('system.user.employmentResigned') }}
          </Tag>
        </div>
      </template>
      <template #toolbar-tools>
        <Button size="small" class="mr-1" @click="onClearHeaderFilters">
          {{ $t('system.user.clearHeaderFilters') }}
        </Button>
        <Button
          v-if="canViewUserChangeHistory"
          class="inline-flex items-center gap-1"
          @click="openUserChangeHistory"
        >
          {{ $t('system.user.changeHistoryButton') }}
        </Button>
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
    <ColumnFilterModal
      v-model:open="columnFilterOpen"
      :modal-title="columnFilterModalTitle"
      :loading="columnFacetLoading"
      :options="columnFilterOptions"
      :applied-values="activeColumnAppliedValues"
      @apply="onColumnFilterApply"
      @clear="onColumnFilterClear"
    />
    <UserChangeHistoryModal v-model:open="userChangeHistoryModalOpen" :row="userChangeHistoryRow" />
  </Page>
</template>

<style scoped>
:deep(.vxe-body--row.system-user-row-clickable),
:deep(.vxe-body--row.system-user-row-clickable .vxe-body--column),
:deep(.vxe-body--row.system-user-row-clickable .vxe-cell),
:deep(.vxe-body--row.system-user-row-clickable .vxe-cell *) {
  cursor: pointer;
}
</style>
