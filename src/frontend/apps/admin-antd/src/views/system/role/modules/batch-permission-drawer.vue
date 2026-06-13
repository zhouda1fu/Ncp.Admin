<script lang="ts" setup>
import type { PermissionTreeNode } from '#/utils/permission-tree';
import type { SystemRoleApi } from '#/api/system/role';

import { computed, nextTick, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { Button, message, RadioGroup, Select, Spin } from 'ant-design-vue';

import { batchUpdateRolePermissions, getRoleList } from '#/api/system/role';
import { stripSyntheticPermissionTreeKeys } from '#/constants/module-menu-categories';
import { $t } from '#/locales';
import { buildPermissionTree } from '#/utils/permission-tree';

import PermissionCodesTree from './permission-codes-tree.vue';

const emits = defineEmits(['success']);

const permissions = ref<PermissionTreeNode[]>([]);
const roleOptions = ref<{ label: string; value: string }[]>([]);
const selectedRoleIds = ref<string[]>([]);
const selectedPermissionCodes = ref<string[]>([]);
const operation = ref<SystemRoleApi.RolePermissionBatchOperation>(0);

const operationOptions = computed(() => [
  { label: $t('system.role.batchPermissionAdd'), value: 0 },
  { label: $t('system.role.batchPermissionRemove'), value: 1 },
]);

const selectedRoleCount = computed(() => selectedRoleIds.value.length);
const selectedPermissionCount = computed(() => selectedPermissionCodes.value.length);

async function loadRoleOptions() {
  const pageSize = 500;
  const roles: SystemRoleApi.SystemRole[] = [];
  let pageIndex = 1;
  let total = Number.POSITIVE_INFINITY;

  while (roles.length < total) {
    const res = await getRoleList({ pageIndex, pageSize, countTotal: true } as any);
    const items = res.items ?? [];
    total = res.total ?? items.length;
    roles.push(...items);
    if (items.length < pageSize) break;
    pageIndex += 1;
  }

  roleOptions.value = roles.map((role) => ({
    label: role.name ?? String(role.roleId),
    value: String(role.roleId),
  }));
}

function selectAllRoles() {
  selectedRoleIds.value = roleOptions.value.map((option) => option.value);
}

function clearRoles() {
  selectedRoleIds.value = [];
}

function filterSelectOption(input: string, option?: { label?: unknown; value?: unknown }) {
  return String(option?.label ?? option?.value ?? '')
    .toLowerCase()
    .includes(input.toLowerCase());
}

function updateRoleIds(value: unknown) {
  selectedRoleIds.value = Array.isArray(value) ? value.map((item) => String(item)) : [];
}

function roleMaxTagPlaceholder(omitted: number) {
  const total = selectedRoleIds.value.length;
  if (total <= 0) return '';
  if (omitted <= 0) return $t('system.role.batchPermissionSelectedRoles', { count: total });
  return $t('system.role.batchPermissionSelectedRoles', { count: total });
}

function clearPermissions() {
  selectedPermissionCodes.value = [];
}

const [Drawer, drawerApi] = useVbenDrawer({
  async onConfirm() {
    const roleIds = selectedRoleIds.value.map((roleId) => String(roleId));
    if (roleIds.length === 0) {
      message.warning($t('system.role.batchPermissionRoleRequired'));
      return;
    }

    const permissionCodes = stripSyntheticPermissionTreeKeys(selectedPermissionCodes.value);
    if (permissionCodes.length === 0) {
      message.warning($t('system.role.batchPermissionCodesRequired'));
      return;
    }

    drawerApi.lock();
    try {
      await batchUpdateRolePermissions({
        roleIds,
        operation: operation.value,
        permissionCodes,
      });
      message.success(
        operation.value === 0
          ? $t('system.role.batchPermissionAddSuccess', {
              permissions: permissionCodes.length,
              roles: roleIds.length,
            })
          : $t('system.role.batchPermissionRemoveSuccess', {
              permissions: permissionCodes.length,
              roles: roleIds.length,
            }),
      );
      emits('success');
      drawerApi.close();
    } finally {
      drawerApi.unlock();
    }
  },

  async onOpenChange(isOpen) {
    if (!isOpen) return;

    const data = drawerApi.getData<{ roles?: SystemRoleApi.SystemRole[] }>();
    selectedRoleIds.value = (data?.roles ?? []).map((role) => String(role.roleId));
    selectedPermissionCodes.value = [];
    operation.value = 0;

    if (permissions.value.length === 0) {
      permissions.value = buildPermissionTree();
    }

    await loadRoleOptions();
    await nextTick();
  },
});
</script>

<template>
  <Drawer :title="$t('system.role.batchPermissionTitle')" class="!w-[820px] max-w-[96vw]">
    <div class="flex flex-col gap-5">
      <section class="flex flex-col gap-2">
        <div class="text-sm font-medium leading-6">
          {{ $t('system.role.batchPermissionRoles') }}
        </div>
        <div class="flex flex-wrap items-center gap-3">
          <Button
            type="link"
            size="small"
            class="!h-auto !px-0"
            :disabled="roleOptions.length === 0"
            @click="selectAllRoles"
          >
            {{ $t('system.role.batchPermissionSelectAllRoles') }}
          </Button>
          <Button
            type="link"
            size="small"
            class="!h-auto !px-0"
            :disabled="selectedRoleIds.length === 0"
            @click="clearRoles"
          >
            {{ $t('system.role.batchPermissionClearRoles') }}
          </Button>
          <span v-if="selectedRoleCount > 0" class="text-muted-foreground text-xs">
            {{ $t('system.role.batchPermissionSelectedRoles', { count: selectedRoleCount }) }}
          </span>
        </div>
        <Select
          allow-clear
          class="w-full"
          :filter-option="filterSelectOption"
          :max-tag-count="0"
          :max-tag-placeholder="roleMaxTagPlaceholder"
          mode="multiple"
          :options="roleOptions"
          :placeholder="$t('system.role.batchPermissionRolePlaceholder')"
          show-search
          virtual
          :value="selectedRoleIds"
          @update:value="updateRoleIds"
        />
      </section>

      <section class="flex flex-col gap-2">
        <div class="text-sm font-medium leading-6">
          {{ $t('system.role.batchPermissionOperation') }}
        </div>
        <RadioGroup
          v-model:value="operation"
          button-style="solid"
          option-type="button"
          :options="operationOptions"
        />
      </section>

      <section class="flex flex-col gap-2">
        <div class="flex flex-wrap items-center justify-between gap-2">
          <div class="text-sm font-medium leading-6">
            {{ $t('system.role.batchPermissionPermissions') }}
          </div>
          <div class="flex items-center gap-2">
            <span class="text-muted-foreground text-xs">
              {{ $t('system.role.selectedPermissionCount', [selectedPermissionCount]) }}
            </span>
            <Button
              size="small"
              :disabled="selectedPermissionCodes.length === 0"
              @click="clearPermissions"
            >
              {{ $t('system.role.batchPermissionClearPermissions') }}
            </Button>
          </div>
        </div>
        <Spin :spinning="false" wrapper-class-name="w-full">
          <PermissionCodesTree
            :permissions="permissions"
            :model-value="selectedPermissionCodes"
            @update:model-value="selectedPermissionCodes = $event"
          />
        </Spin>
      </section>
    </div>
  </Drawer>
</template>
