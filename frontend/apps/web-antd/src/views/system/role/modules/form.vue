<script lang="ts" setup>
import type { SystemRoleApi } from '#/api/system/role';

import { computed, nextTick, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { useVbenForm } from '#/adapter/form';
import { createRole, updateRole } from '#/api/system/role';
import { $t } from '#/locales';

import { useFormSchema } from '../data';

const emits = defineEmits(['success']);

const formData = ref<SystemRoleApi.SystemRole>();

const [Form, formApi] = useVbenForm({
  schema: useFormSchema(),
  showDefaultActions: false,
});

const id = ref<string>();
const [Drawer, drawerApi] = useVbenDrawer({
  async onConfirm() {
    const { valid } = await formApi.validate();
    if (!valid) return;
    const values = await formApi.getValues();
    drawerApi.lock();
    try {
      if (id.value) {
        await updateRole(id.value, {
          name: values.name,
          description: values.description || '',
          permissionCodes: values.permissionCodes || [],
        });
      } else {
        await createRole({
          name: values.name,
          description: values.description || '',
          permissionCodes: values.permissionCodes || [],
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

      // Wait for Vue to flush DOM updates (form fields mounted)
      await nextTick();
      if (data && data.roleId) {
        formApi.setValues({
          name: data.name,
          description: data.description || '',
          isActive: data.isActive,
          permissionCodes: data.permissionCodes || [],
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
  <Drawer :title="getDrawerTitle">
    <Form />
  </Drawer>
</template>
