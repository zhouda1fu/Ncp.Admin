<script lang="ts" setup>
import { computed, h, nextTick, onUnmounted, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';
import { ArrowLeft } from '@vben/icons';

import { Alert, Button, Card, message, Modal, Space } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import { getDeptTree } from '#/api/system/dept';
import { fetchFileBlob } from '#/api/system/file';
import { getRoleList } from '#/api/system/role';
import { createUser, getUser, resetUserPassword, updateUser, updateUserRoles } from '#/api/system/user';
import { PermissionCodes } from '#/constants/permission-codes';
import { useAccessStore } from '@vben/stores';
import { getPublishedDefinitions, startWorkflow } from '#/api/system/workflow';
import { $t } from '#/locales';
import { navigateBackToList } from '#/utils/list-return-state';

import { useFormSchema } from './data';

const LIST_PATH = '/system/user';

const route = useRoute();
const router = useRouter();

const id = computed(() => route.params.id as string | undefined);
const isCreateMode = computed(() => !id.value);
const isViewMode = computed(() => route.path.includes('/view'));
const pageTitle = computed(() => {
  if (isCreateMode.value) return $t('common.create', [$t('system.user.name')]);
  return isViewMode.value
    ? $t('common.view', [$t('system.user.name')])
    : $t('common.edit', [$t('system.user.name')]);
});

const submitting = ref(false);
const avatarBlobUrl = ref<string | null>(null);
/** 新建模式下页内展示的随机初始密码（与表单隐藏字段 password 同步） */
const createPasswordPlain = ref('');

const LETTERS = 'ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz';
const DIGITS = '23456789';
const ALNUM = LETTERS + DIGITS;

function generateRandomPassword8(): string {
  const pick = (pool: string, bytes: Uint8Array, i: number) =>
    pool[bytes[i]! % pool.length]!;
  for (let attempt = 0; attempt < 50; attempt++) {
    const bytes = new Uint8Array(8);
    crypto.getRandomValues(bytes);
    let s = '';
    for (let i = 0; i < 8; i++) s += pick(ALNUM, bytes, i);
    if (/[A-Za-z]/.test(s) && /\d/.test(s)) return s;
  }
  return 'Ab3x7nQ9';
}

function assignCreatePassword() {
  const pwd = generateRandomPassword8();
  createPasswordPlain.value = pwd;
  formApi.setFieldValue('password', pwd);
}

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

function findDeptNode(deptTree: any[], deptId: string): any | undefined {
  for (const dept of deptTree) {
    if (String(dept.id) === String(deptId)) {
      return dept;
    }
    if (dept.children?.length) {
      const found = findDeptNode(dept.children, deptId);
      if (found) return found;
    }
  }
  return undefined;
}

async function handleDeptChange(deptId?: string) {
  if (!isCreateMode.value) return;
  if (!deptId) {
    formApi.setFieldValue('setAsDeptResponsibleUser', false);
    formApi.setFieldValue('setAsDefaultDeptResponsibleUser', false);
    return;
  }
  const values = await formApi.getValues();
  if (!values.setAsDeptResponsibleUser) return;
  const deptTree = await getDeptTree();
  const dept = findDeptNode(deptTree, deptId);
  const hasDefaultResponsibleUser = dept?.responsibleUsers?.some((u: any) => u.isDefault) ?? false;
  formApi.setFieldValue('setAsDefaultDeptResponsibleUser', !hasDefaultResponsibleUser);
}

async function handleSetAsDeptResponsibleUserChange(checked: boolean) {
  if (!checked) {
    formApi.setFieldValue('setAsDefaultDeptResponsibleUser', false);
    return;
  }
  const values = await formApi.getValues();
  if (values.deptId) {
    await handleDeptChange(values.deptId);
  }
}

const accessStore = useAccessStore();

function canResetPassword() {
  return accessStore.accessCodes?.includes(PermissionCodes.UserResetPassword) ?? false;
}

const [Form, formApi] = useVbenForm({
  layout: 'horizontal',
  labelWidth: 120,
  commonConfig: { colon: true },
  schema: computed(() =>
    useFormSchema(
      isCreateMode.value ? 'create' : 'edit',
      (path) => formApi.setFieldValue('avatarUrl', path),
      handleDeptChange,
      handleSetAsDeptResponsibleUserChange,
      canResetPassword(),
    ),
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
      notAttendanceRequired: data.attendanceRequired === false,
      wechatGuid: data.wechatGuid ?? '',
      isResigned: data.isResigned ?? false,
      resignedTime: data.resignedTime ?? undefined,
      setAsDeptResponsibleUser: data.setAsDeptResponsibleUser ?? false,
      setAsDefaultDeptResponsibleUser:
        (data.setAsDeptResponsibleUser ?? false) && (data.setAsDefaultDeptResponsibleUser ?? false),
      resetPassword: false,
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
  async (v) => {
    if (v) {
      loadUser();
    } else {
      formApi.resetForm();
      formApi.setValues({
        status: 1,
        notOrderMeal: false,
        notAttendanceRequired: false,
        isResigned: false,
        avatarUrl: '',
        avatarFileList: [],
        setAsDeptResponsibleUser: false,
        setAsDefaultDeptResponsibleUser: false,
      });
      await nextTick();
      assignCreatePassword();
    }
  },
  { immediate: true },
);

function goBack(options?: { reload?: boolean }) {
  void navigateBackToList(router, route, [LIST_PATH], LIST_PATH, options);
}

function resetForm() {
  if (isViewMode.value) return;
  formApi.resetForm();
  if (id.value) {
    loadUser();
  } else {
    formApi.setValues({
      status: 1,
      notOrderMeal: false,
      notAttendanceRequired: false,
      isResigned: false,
    });
    void nextTick(() => assignCreatePassword());
  }
}

async function onSubmit() {
  if (isViewMode.value) return;
  if (submitting.value) return;
  const { valid } = await formApi.validate();
  if (!valid) return;

  const values = await formApi.getValues();
  if (!id.value && !values.password?.trim()) {
    message.error($t('ui.formRules.required', [$t('system.user.password')]));
    return;
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
      const resetPwd = !!values.resetPassword;
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
        idCardNumber: values.idCardNumber ?? '',
        address: values.address ?? '',
        education: values.education ?? '',
        graduateSchool: values.graduateSchool ?? '',
        avatarUrl: values.avatarFileList?.length ? (values.avatarUrl ?? '') : '',
        notOrderMeal: values.notOrderMeal ?? false,
        attendanceRequired: !(values.notAttendanceRequired ?? false),
        wechatGuid: undefined,
        isResigned: values.isResigned ?? false,
        resignedTime: resignedDateStr,
        setAsDeptResponsibleUser: values.setAsDeptResponsibleUser ?? false,
        setAsDefaultDeptResponsibleUser:
          (values.setAsDeptResponsibleUser ?? false) && (values.setAsDefaultDeptResponsibleUser ?? false),
      });
      if (values.roleIds && Array.isArray(values.roleIds)) {
        await updateUserRoles(id.value, values.roleIds);
      }
      if (resetPwd) {
        await resetUserPassword(id.value);
        Modal.info({
          title: $t('system.user.passwordResetSavedTitle'),
          content: h('div', { class: 'mt-2 space-y-2' }, [
            h('p', { class: 'text-muted-foreground text-sm' }, $t('system.user.passwordResetSavedHint')),
          ]),
          okText: $t('system.user.closeModal'),
          maskClosable: false,
          onOk() {
            goBack({ reload: true });
          },
        });
      } else {
        message.success($t('ui.actionMessage.updateSuccess'));
        goBack({ reload: true });
      }
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
        roleIds: values.roleIds ?? [],
        idCardNumber: values.idCardNumber ?? '',
        address: values.address ?? '',
        education: values.education ?? '',
        graduateSchool: values.graduateSchool ?? '',
        avatarUrl: values.avatarFileList?.length ? (values.avatarUrl ?? '') : '',
        notOrderMeal: values.notOrderMeal ?? false,
        attendanceRequired: !(values.notAttendanceRequired ?? false),
        wechatGuid: undefined,
        isResigned: values.isResigned ?? false,
        resignedTime: resignedDateStr,
        setAsDeptResponsibleUser: values.setAsDeptResponsibleUser ?? false,
        setAsDefaultDeptResponsibleUser:
          (values.setAsDeptResponsibleUser ?? false) && (values.setAsDefaultDeptResponsibleUser ?? false),
      });
      message.success($t('ui.actionMessage.createSuccess'));
      goBack({ reload: true });
    }
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
    attendanceRequired: !(values.notAttendanceRequired ?? false),
    wechatGuid: values.wechatGuid ?? '',
    isResigned: values.isResigned ?? false,
    resignedTime: values.resignedTime ?? undefined,
    setAsDeptResponsibleUser: values.setAsDeptResponsibleUser ?? false,
    setAsDefaultDeptResponsibleUser:
      (values.setAsDeptResponsibleUser ?? false) && (values.setAsDefaultDeptResponsibleUser ?? false),
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
      goBack({ reload: true });
    },
  });
}
</script>

<template>
  <Page auto-content-height content-class="flex flex-col">
    <div class="w-full flex-1 min-w-0">
      <div class="mb-4 flex items-center gap-2">
        <Button class="inline-flex items-center gap-1" @click="() => goBack()">
          <ArrowLeft class="size-4 shrink-0" />
          {{ $t('common.back') }}
        </Button>
      </div>
      <div class="border-border border-b pb-5">
        <h2 class="mb-1.5 text-lg font-semibold text-foreground">
          {{ pageTitle }}
        </h2>
      </div>
      <Card :bordered="true" class="border-border bg-card mt-5">
        <template #title>
          <span class="text-base font-medium">{{ $t('system.user.name') }}</span>
        </template>
        <Alert
          v-if="isCreateMode"
          class="mb-4"
          type="info"
          show-icon
        >
          <template #message>
            <span>{{ $t('system.user.initialPasswordAlertTitle') }}</span>
          </template>
          <template #description>
            <div class="flex flex-wrap items-center gap-2">
              <code class="rounded bg-muted px-2 py-1 font-mono text-sm select-all">{{ createPasswordPlain }}</code>
              <Button size="small" type="link" class="p-0 h-auto" @click="assignCreatePassword">
                {{ $t('system.user.regenerateInitialPassword') }}
              </Button>
            </div>
            <p class="mt-2 text-xs text-muted-foreground">{{ $t('system.user.initialPasswordAlertDesc') }}</p>
          </template>
        </Alert>
        <div class="pb-2">
          <Form :disabled="isViewMode" />
        </div>
        <div class="flex justify-end gap-3 border-t border-border pt-4">
          <Space>
            <Button
              v-if="isCreateMode"
              type="dashed"
              @click="onSubmitForApproval"
            >
              {{ $t('system.workflow.submitForApproval') }}
            </Button>
            <Button v-if="!isViewMode" @click="resetForm">
              {{ $t('common.reset') }}
            </Button>
            <Button
              v-if="!isViewMode"
              type="primary"
              :loading="submitting"
              :disabled="submitting"
              @click="onSubmit"
            >
              {{ $t('common.confirm') }}
            </Button>
          </Space>
        </div>
      </Card>
    </div>
  </Page>
</template>
