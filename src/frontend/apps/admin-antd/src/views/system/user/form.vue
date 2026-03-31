<script lang="ts" setup>
import { computed, nextTick, onUnmounted, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';

import { Button, Card, message, Modal, Space } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import { getDeptTree } from '#/api/system/dept';
import { fetchFileBlob } from '#/api/system/file';
import { getRoleList } from '#/api/system/role';
import { createUser, getUser, updateUser, updateUserRoles } from '#/api/system/user';
import { getPublishedDefinitions, startWorkflow } from '#/api/system/workflow';
import { $t } from '#/locales';

import { useFormSchema } from './data';

const route = useRoute();
const router = useRouter();

const id = computed(() => route.params.id as string | undefined);
const isCreateMode = computed(() => !id.value);

const submitting = ref(false);
const avatarBlobUrl = ref<string | null>(null);

/** 从部门树中查找部门名称 */
function findDeptName(deptTree: any[], deptId: string): string {
  for (const dept of deptTree) {
    if (dept.id === deptId) {
      return dept.name;
    }
    if (dept.children?.length) {
      const found = findDeptName(dept.children, deptId);
      if (found) return found;
    }
  }
  return '';
}

const [Form, formApi] = useVbenForm({
  layout: 'horizontal',
  labelWidth: 120,
  commonConfig: { colon: true },
  schema: computed(() =>
    useFormSchema((path) => formApi.setFieldValue('avatarUrl', path)),
  ),
  showDefaultActions: false,
  wrapperClass: 'grid-cols-1 sm:grid-cols-2 gap-x-6 gap-y-4',
} as any);

async function loadUser() {
  if (!id.value) return;
  try {
    const data = await getUser(id.value);
    let roleIds: string[] = [];
    if (data.roles?.length) {
      const roleListResult = await getRoleList({
        pageIndex: 1,
        pageSize: 1000,
        countTotal: false,
      });
      const roleMap = new Map(
        roleListResult.items.map((r) => [r.name, r.roleId]),
      );
      roleIds = data.roles
        .map((name: string) => roleMap.get(name))
        .filter((id): id is string => !!id);
    }
    const isDeptManager = data.isDeptManager ?? false;
    const avatarPath = data.avatarUrl ?? '';
    const avatarFileName = avatarPath ? avatarPath.split('/').pop() || $t('system.user.avatarUrl') : '';
    // 部门树选项的 id 为 string，接口可能返回 number，需统一为 string 才能正确回显
    const rawDeptId = data.deptId;
    const deptIdStr =
      rawDeptId != null && rawDeptId !== '' && String(rawDeptId) !== '0'
        ? String(rawDeptId)
        : undefined;
    if (avatarBlobUrl.value) {
      URL.revokeObjectURL(avatarBlobUrl.value);
      avatarBlobUrl.value = null;
    }
    await nextTick();
    formApi.setValues({
      name: data.name,
      email: data.email,
      phone: data.phone ?? '',
      realName: data.realName ?? '',
      status: data.status ?? 1,
      gender: data.gender ?? '',
      birthDate: data.birthDate,
      deptId: deptIdStr,
      deptName: data.deptName ?? '',
      roleIds,
      password: '',
      idCardNumber: data.idCardNumber ?? '',
      address: data.address ?? '',
      education: data.education ?? '',
      graduateSchool: data.graduateSchool ?? '',
      avatarUrl: avatarPath,
      avatarFileList: avatarPath
        ? [
            {
              uid: 'avatar',
              name: avatarFileName || $t('system.user.avatarUrl'),
              url: undefined as string | undefined,
              thumbUrl: undefined as string | undefined,
            },
          ]
        : [],
      notOrderMeal: data.notOrderMeal ?? false,
      wechatGuid: data.wechatGuid ?? '',
      isResigned: data.isResigned ?? false,
      resignedTime: data.resignedTime ?? undefined,
      isDeptManager,
    });
    if (avatarPath) {
      try {
        const blob = await fetchFileBlob(avatarPath);
        const blobUrl = URL.createObjectURL(blob);
        avatarBlobUrl.value = blobUrl;
        formApi.setFieldValue('avatarFileList', [
          {
            uid: 'avatar',
            name: avatarFileName || $t('system.user.avatarUrl'),
            url: blobUrl,
            thumbUrl: blobUrl,
          },
        ]);
      } catch {
        // 预览加载失败时仅保留文件名
      }
    }
  } catch {
    message.error($t('ui.actionMessage.loadFailed'));
  }
}

onUnmounted(() => {
  if (avatarBlobUrl.value) {
    URL.revokeObjectURL(avatarBlobUrl.value);
    avatarBlobUrl.value = null;
  }
});

watch(
  id,
  (v) => {
    if (v) {
      loadUser();
    } else {
      formApi.resetForm();
      formApi.setValues({
        status: 1,
        notOrderMeal: false,
        isResigned: false,
        isDeptManager: false,
        avatarUrl: '',
        avatarFileList: [],
      });
    }
  },
  { immediate: true },
);

function goBack() {
  router.push('/system/user');
}

function resetForm() {
  formApi.resetForm();
  if (id.value) {
    loadUser();
  } else {
    formApi.setValues({
      status: 1,
      notOrderMeal: false,
      isResigned: false,
      isDeptManager: false,
    });
  }
}

async function onSubmit() {
  if (submitting.value) return;
  const { valid } = await formApi.validate();
  if (!valid) return;

  const values = await formApi.getValues();
  if (!id.value) {
    if (!values.password?.trim()) {
      message.error($t('ui.formRules.required', [$t('system.user.password')]));
      return;
    }
  }

  submitting.value = true;
  try {
    let deptName = values.deptName ?? '';
    if (values.deptId && !deptName) {
      const deptTree = await getDeptTree();
      deptName = findDeptName(deptTree, values.deptId);
    }

    const resignedDateStr =
      values.isResigned && !values.resignedTime
        ? new Date().toISOString().slice(0, 10)
        : values.resignedTime || undefined;

    if (id.value) {
      await updateUser(id.value, {
        name: values.name,
        email: values.email,
        phone: values.phone ?? '',
        realName: values.realName ?? '',
        status: values.isResigned ? 0 : values.status ?? 1,
        gender: values.gender ?? '',
        age: 0,
        birthDate: values.birthDate,
        deptId: values.deptId ?? '0',
        deptName,
        isDeptManager: values.isDeptManager ?? false,
        password: values.password ?? '',
        idCardNumber: values.idCardNumber ?? '',
        address: values.address ?? '',
        education: values.education ?? '',
        graduateSchool: values.graduateSchool ?? '',
        avatarUrl: values.avatarFileList?.length ? (values.avatarUrl ?? '') : '',
        notOrderMeal: values.notOrderMeal ?? false,
        wechatGuid: undefined,
        isResigned: values.isResigned ?? false,
        resignedTime: resignedDateStr,
      });
      if (values.roleIds && Array.isArray(values.roleIds)) {
        await updateUserRoles(id.value, values.roleIds);
      }
      message.success($t('ui.actionMessage.updateSuccess'));
    } else {
      await createUser({
        name: values.name,
        email: values.email,
        password: values.password,
        phone: values.phone ?? '',
        realName: values.realName ?? '',
        status: values.isResigned ? 0 : values.status ?? 1,
        gender: values.gender ?? '',
        birthDate: values.birthDate,
        deptId: values.deptId,
        deptName,
        isDeptManager: values.isDeptManager ?? false,
        roleIds: values.roleIds ?? [],
        idCardNumber: values.idCardNumber ?? '',
        address: values.address ?? '',
        education: values.education ?? '',
        graduateSchool: values.graduateSchool ?? '',
        avatarUrl: values.avatarFileList?.length ? (values.avatarUrl ?? '') : '',
        notOrderMeal: values.notOrderMeal ?? false,
        wechatGuid: undefined,
        isResigned: values.isResigned ?? false,
        resignedTime: resignedDateStr,
      });
      message.success($t('ui.actionMessage.createSuccess'));
    }
    goBack();
  } catch {
    // 错误由请求层处理
  } finally {
    submitting.value = false;
  }
}

/** 提交审批（仅新建模式） */
async function onSubmitForApproval() {
  const { valid } = await formApi.validate();
  if (!valid) return;
  const values = await formApi.getValues();
  if (!values.password?.trim()) {
    message.error($t('ui.formRules.required', [$t('system.user.password')]));
    return;
  }

  const definitions = await getPublishedDefinitions();
  const userCreateDef = definitions.find((d) => d.category === 'CreateUser');
  if (!userCreateDef) {
    Modal.warning({
      content: $t('system.workflow.noDefinitionForUser'),
      title: $t('system.workflow.noDefinitionTitle'),
    });
    return;
  }

  let deptName = '';
  if (values.deptId) {
    const deptTree = await getDeptTree();
    deptName = findDeptName(deptTree, values.deptId);
  }

  const variables = JSON.stringify({
    name: values.name,
    email: values.email,
    password: values.password,
    phone: values.phone ?? '',
    realName: values.realName,
    status: values.isResigned ? 0 : values.status ?? 1,
    gender: values.gender ?? '',
    birthDate: values.birthDate,
    deptId: values.deptId ?? '',
    deptName,
    roleIds: values.roleIds ?? [],
    idCardNumber: values.idCardNumber ?? '',
    address: values.address ?? '',
    education: values.education ?? '',
    graduateSchool: values.graduateSchool ?? '',
    avatarUrl: values.avatarFileList?.length ? (values.avatarUrl ?? '') : '',
    notOrderMeal: values.notOrderMeal ?? false,
    wechatGuid: values.wechatGuid ?? '',
    isResigned: values.isResigned ?? false,
    resignedTime: values.resignedTime ?? undefined,
  });

  Modal.confirm({
    content: $t('system.workflow.submitApprovalConfirmContent'),
    title: $t('system.workflow.submitApprovalConfirmTitle'),
    async onOk() {
      await startWorkflow({
        workflowDefinitionId: userCreateDef.id,
        businessKey: values.name,
        businessType: 'CreateUser',
        title: `新增用户申请 - ${values.realName || values.name}`,
        variables,
        remark: '',
      });
      message.success({
        content: $t('system.workflow.submitApprovalSuccess'),
        duration: 4,
      });
      message.info({
        content: $t('system.workflow.submitApprovalSuccessDetail'),
        duration: 5,
      });
      goBack();
    },
  });
}
</script>

<template>
  <Page auto-content-height content-class="flex flex-col">
    <div class="w-full flex-1 min-w-0">
      <div class="border-border border-b pb-5">
        <h2 class="mb-1.5 text-lg font-semibold text-foreground">
          {{ isCreateMode ? $t('common.create', [$t('system.user.name')]) : $t('common.edit', [$t('system.user.name')]) }}
        </h2>
      </div>
      <Card :bordered="true" class="border-border bg-card mt-5">
        <template #title>
          <span class="text-base font-medium">{{ $t('system.user.name') }}</span>
        </template>
        <div class="pb-2">
          <Form />
        </div>
        <div class="flex justify-end gap-3 border-t border-border pt-4">
          <Space>
            <Button v-if="isCreateMode" type="dashed" @click="onSubmitForApproval">
              {{ $t('system.workflow.submitForApproval') }}
            </Button>
            <Button @click="resetForm">{{ $t('common.reset') }}</Button>
            <Button type="primary" :loading="submitting" :disabled="submitting" @click="onSubmit">
              {{ $t('common.confirm') }}
            </Button>
          </Space>
        </div>
      </Card>
    </div>
  </Page>
</template>
