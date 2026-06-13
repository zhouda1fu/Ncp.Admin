<script lang="ts" setup>
import type { PermissionTreeNode } from '#/utils/permission-tree';
import type { SystemRoleApi } from '#/api/system/role';

import { computed, nextTick, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { Spin, Select, message } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import { createRole, updateRole, getRole, getRoleList } from '#/api/system/role';
import { buildPermissionTree } from '#/utils/permission-tree';
import { stripSyntheticPermissionTreeKeys } from '#/constants/module-menu-categories';
import { $t } from '#/locales';

import { useFormSchema } from '../data';
import PermissionCodesTree from './permission-codes-tree.vue';

const emits = defineEmits(['success']);

const formData = ref<SystemRoleApi.SystemRole>();

const [Form, formApi] = useVbenForm({
  schema: useFormSchema(),
  showDefaultActions: false,
});

const permissions = ref<PermissionTreeNode[]>([]);

/** 复制授权：可选角色（排除当前编辑中的角色） */
const copyRoleOptions = ref<{ label: string; value: string }[]>([]);
const copyFromRoleId = ref<string | undefined>();

async function loadCopyRoleOptions() {
  try {
    const res = await getRoleList({
      pageIndex: 1,
      pageSize: 500,
      countTotal: true,
    });
    const items = res.items ?? [];
    const currentId = id.value ? String(id.value) : '';
    copyRoleOptions.value = items
      .filter((r) => String(r.roleId) !== currentId)
      .map((r) => ({ label: r.name, value: String(r.roleId) }));
  } catch {
    copyRoleOptions.value = [];
  }
}

async function onCopyPermissionRoleChange(roleId: unknown) {
  const roleIdValue = String(roleId ?? '').trim();
  if (!roleIdValue) return;
  try {
    const detail = await getRole(roleIdValue);
    const raw = detail.permissionCodes;
    const codes = Array.isArray(raw) ? [...raw] : [];
    await formApi.setValues({ permissionCodes: codes });
    message.success($t('system.role.copyPermissionsSuccess'));
  } catch {
    message.error($t('system.role.copyPermissionsFailed'));
  } finally {
    copyFromRoleId.value = undefined;
  }
}

const id = ref<string>();
const [Drawer, drawerApi] = useVbenDrawer({
  async onConfirm() {
    const { valid } = await formApi.validate();
    if (!valid) return;
    const values = await formApi.getValues();
    drawerApi.lock();
    try {
      if (id.value) {
        if ((values.dataScope ?? 0) !== 4) {
          await formApi.setValues({ customDeptIds: [] });
        }
        await updateRole(id.value, {
          name: values.name,
          description: values.description || '',
          dataScope: values.dataScope ?? 0,
          customDeptIds:
            (values.dataScope ?? 0) === 4 ? (values.customDeptIds ?? []) : [],
          permissionCodes: stripSyntheticPermissionTreeKeys(values.permissionCodes || []),
        });
      } else {
        if ((values.dataScope ?? 0) !== 4) {
          await formApi.setValues({ customDeptIds: [] });
        }
        await createRole({
          name: values.name,
          description: values.description || '',
          dataScope: values.dataScope ?? 0,
          customDeptIds:
            (values.dataScope ?? 0) === 4 ? (values.customDeptIds ?? []) : [],
          permissionCodes: stripSyntheticPermissionTreeKeys(values.permissionCodes || []),
        });
      }
      emits('success');
      drawerApi.close();
    } catch {
      drawerApi.unlock();
    }
  },

  async onOpenChange(isOpen) {
    if (isOpen) {
      const data = drawerApi.getData<SystemRoleApi.SystemRole>();
      formApi.resetForm();

      if (data && data.roleId) {
        formData.value = data;
        id.value = data.roleId;
      } else {
        id.value = undefined;
        formData.value = undefined;
      }

      if (permissions.value.length === 0) {
        permissions.value = buildPermissionTree();
      }

      copyFromRoleId.value = undefined;
      await loadCopyRoleOptions();

      await nextTick();
      if (data && data.roleId) {
        const detail = await getRole(String(data.roleId));
        const permissionCodes = detail.permissionCodes || [];
        formApi.setValues({
          name: detail.name,
          description: detail.description || '',
          isActive: detail.isActive,
          dataScope: detail.dataScope ?? 0,
          customDeptIds: detail.customDeptIds ?? [],
          permissionCodes,
        });
      }
    }
  },
});

const getDrawerTitle = computed(() => {
  return formData.value?.roleId
    ? $t('common.edit', [$t('system.role.name')])
    : $t('common.create', [$t('system.role.name')]);
});
</script>
<template>
  <Drawer :title="getDrawerTitle" class="!w-[760px] max-w-[96vw]" destroyOnClose>
    <Form>
      <template #permissionCodes="slotProps">
        <Spin :spinning="false" wrapper-class-name="w-full">
          <div class="mb-4 flex w-full max-w-full flex-col gap-1">
            <span class="text-muted-foreground text-sm leading-snug">
              {{ $t('system.role.copyPermissionsHint') }}
            </span>
            <Select
              v-model:value="copyFromRoleId"
              allow-clear
              show-search
              option-filter-prop="label"
              class="max-w-md w-full"
              :options="copyRoleOptions"
              :placeholder="$t('system.role.copyPermissionsPlaceholder')"
              @change="onCopyPermissionRoleChange"
            />
          </div>
          <PermissionCodesTree
            :permissions="permissions"
            :model-value="slotProps.componentField?.modelValue"
            @update:model-value="slotProps.componentField?.['onUpdate:modelValue']"
          />
        </Spin>
      </template>
    </Form>
  </Drawer>
</template>
<style lang="css" scoped>
:deep(.ant-tree-title) {
  .tree-actions {
    display: none;
    margin-left: 20px;
  }
}

:deep(.ant-tree-title:hover) {
  .tree-actions {
    display: flex;
    flex: auto;
    justify-content: flex-end;
    margin-left: 20px;
  }
}
</style>
