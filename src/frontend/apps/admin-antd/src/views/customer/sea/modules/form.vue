<script lang="ts" setup>
import type { RegionApi } from '#/api/system/region';

import { computed, h, onBeforeUnmount, onMounted, ref, watch } from 'vue';

import { useAccessStore } from '@vben/stores';
import { useVbenDrawer } from '@vben/common-ui';

import { Alert, Button, Cascader, Form, FormItem, Input, Modal, Select } from 'ant-design-vue';

import type { RegionCascaderOption } from '../../data';

function filterOption(input: string, option: unknown) {
  const o = option as { label?: string } | undefined;
  const label = o?.label ?? '';
  return label.toLowerCase().includes((input ?? '').toLowerCase());
}

import type { CustomerApi } from '#/api/system/customer';
import {
  checkSeaCustomerDuplicateContacts,
  createSeaCustomer,
  updateSeaCustomer,
  updateSeaCustomerConsultation,
} from '#/api/system/customer';
import { getCustomerSourceList } from '#/api/system/customerSource';
import { PermissionCodes } from '#/constants/permission-codes';
import { getRegionList } from '#/api/system/region';
import { $t } from '#/locales';

const emit = defineEmits(['success']);

const accessStore = useAccessStore();

const regionList = ref<RegionApi.RegionItem[]>([]);
const customerSourceOptions = ref<{ label: string; value: string }[]>([]);
const editId = ref<string | null>(null);

const formState = ref({
  customerSourceId: '',
  fullName: '',
  mainContactName: '',
  mainContactPhone: '',
  contactQq: '',
  contactWechat: '',
  /** 电话区域：省市区级联，与新建客户所在区域一致 */
  phoneRegionCodes: undefined as string[] | undefined,
  /** 项目区域：省市区级联，与新建客户所在区域一致 */
  projectRegionCodes: undefined as string[] | undefined,
  consultationContent: '',
});

/** 省级：与新建客户所在区域一致（level=2 的省份 + 港澳台） */
const provinceFilter = (r: RegionApi.RegionItem) =>
  r.level === 2 || (Number(r.parentId) === 9 && r.level === 1);

/** 构建省市区级联树（与新建客户所在区域一致） */
const regionTreeOptions = computed<RegionCascaderOption[]>(() => {
  const list = regionList.value;
  const provinces = list.filter(provinceFilter);
  return provinces.map((p) => {
    const children = list
      .filter((r) => String(r.parentId) === String(p.id))
      .map((c) => {
        const districtChildren = list
          .filter((r) => String(r.parentId) === String(c.id))
          .map((d) => ({ label: d.name, value: String(d.id) }));
        return {
          label: c.name,
          value: String(c.id),
          children: districtChildren.length > 0 ? districtChildren : undefined,
        };
      });
    return {
      label: p.name,
      value: String(p.id),
      children: children.length > 0 ? children : undefined,
    };
  });
});

/** 区域列表是否已加载（用于 Cascader 回显） */
const regionOptionsReady = computed(() => regionList.value.length > 0);

/** 区域列表加载完成后，若处于编辑状态则回填省市区数组 */
watch(
  () => regionList.value.length,
  (len) => {
    if (len > 0 && editId.value) {
      const data = drawerApi.getData<CustomerApi.CustomerDetail & { id?: string }>();
      if (!data?.id) return;
      const toCode = (v: unknown) =>
        v != null && String(v).trim() !== '' ? String(v) : undefined;
      const phoneCodes = [
        toCode(data.phoneProvinceCode),
        toCode(data.phoneCityCode),
        toCode(data.phoneDistrictCode),
      ].filter(Boolean) as string[];
      const projectCodes = [
        toCode(data.provinceCode),
        toCode(data.cityCode),
        toCode(data.districtCode),
      ].filter(Boolean) as string[];
      formState.value = {
        ...formState.value,
        phoneRegionCodes: phoneCodes.length > 0 ? phoneCodes : undefined,
        projectRegionCodes: projectCodes.length > 0 ? projectCodes : undefined,
      };
    }
  },
);

const formRules = computed(() => {
  const requiredPhoneOrProjectRegion = $t(
    'ui.formRules.required',
    [$t('customer.phoneRegion') + '或' + $t('customer.projectRegion')],
  ) as string;

  return {
    customerSourceId: [{ required: true, message: $t('ui.formRules.required', [$t('customer.customerSource')]) }],
    phoneRegionCodes: [
      {
        validator: (_: unknown, value: string[] | undefined) => {
          const hasPhone = value != null && value.length > 0;
          const hasProject =
            formState.value.projectRegionCodes != null && formState.value.projectRegionCodes.length > 0;

          if (hasPhone || hasProject) return Promise.resolve();
          return Promise.reject(new Error(requiredPhoneOrProjectRegion));
        },
      },
    ],
    projectRegionCodes: [
      {
        validator: (_: unknown, value: string[] | undefined) => {
          // 仅在用户显式填了该项但值为空时才报错；是否缺少“二选一”由 phoneRegionCodes 的跨字段校验兜底。
          if (value == null || value.length > 0) return Promise.resolve();
          return Promise.reject(new Error(requiredPhoneOrProjectRegion));
        },
      },
    ],
    consultationContent: [
      { required: true, message: $t('ui.formRules.required', [$t('customer.consultationContent')]) },
    ],
  };
});

// Ant Design Vue Form 实例（含 validate / resetFields），类型用 any 避免与 ant-design-vue 类型不一致
const formRef = ref<any>(null);

/** 新建公海：联系方式实时查重（防抖），编辑模式不查（避免与自身命中） */
const contactDupItems = ref<
  Array<{
    customerId: string;
    customerName: string;
    customerSourceName: string;
    ownerName: string;
    duplicatePhones: string[];
    duplicateQqs: string[];
    duplicateWechats: string[];
  }>
>([]);

function formatDupMeta(source: string | undefined, owner: string | undefined) {
  const dash = '—';
  return {
    sourceLine: `${$t('customer.customerSource')}: ${source?.trim() ? source : dash}`,
    ownerLine: `${$t('customer.ownerId')}: ${owner?.trim() ? owner : dash}`,
  };
}
const contactDupLoading = ref(false);
const contactDupCheckError = ref('');
let contactDupCheckSeq = 0;
let contactDupDebounceTimer: ReturnType<typeof setTimeout> | undefined;

function hasSeaContactInput() {
  const v = formState.value;
  return [v.mainContactPhone, v.contactQq, v.contactWechat].some((s) => String(s ?? '').trim() !== '');
}

function clearContactDupState() {
  clearTimeout(contactDupDebounceTimer);
  contactDupDebounceTimer = undefined;
  contactDupCheckSeq += 1;
  contactDupItems.value = [];
  contactDupLoading.value = false;
  contactDupCheckError.value = '';
}

async function runSeaContactDuplicateCheck() {
  if (editId.value) return;
  contactDupCheckSeq += 1;
  const seq = contactDupCheckSeq;
  if (!hasSeaContactInput()) {
    contactDupItems.value = [];
    contactDupLoading.value = false;
    contactDupCheckError.value = '';
    return;
  }
  contactDupLoading.value = true;
  contactDupCheckError.value = '';
  try {
    const v = formState.value;
    const res = await checkSeaCustomerDuplicateContacts({
      mainContactPhone: v.mainContactPhone ?? '',
      contactQq: v.contactQq ?? '',
      contactWechat: v.contactWechat ?? '',
    });
    if (seq !== contactDupCheckSeq) return;
    contactDupItems.value = res?.items ?? [];
  } catch {
    if (seq !== contactDupCheckSeq) return;
    contactDupCheckError.value = $t('customer.contactDuplicateCheckFailed') as string;
    contactDupItems.value = [];
  } finally {
    if (seq === contactDupCheckSeq) contactDupLoading.value = false;
  }
}

watch(
  () => editId.value,
  (id) => {
    if (id) clearContactDupState();
  },
);

watch(
  () => [
    formState.value.mainContactPhone,
    formState.value.contactQq,
    formState.value.contactWechat,
  ],
  () => {
    if (editId.value) return;
    clearTimeout(contactDupDebounceTimer);
    contactDupDebounceTimer = setTimeout(() => {
      void runSeaContactDuplicateCheck();
    }, 450);
  },
);

const contactDupLines = computed(() =>
  contactDupItems.value.map((x, i) => {
    const bits: string[] = [];
    if (x.duplicatePhones?.length)
      bits.push(`${$t('customer.mainContactPhone')}: ${x.duplicatePhones.join('、')}`);
    if (x.duplicateQqs?.length) bits.push(`${$t('customer.contactQq')}: ${x.duplicateQqs.join('、')}`);
    if (x.duplicateWechats?.length)
      bits.push(`${$t('customer.contactWechat')}: ${x.duplicateWechats.join('、')}`);
    const meta = formatDupMeta(x.customerSourceName, x.ownerName);
    return {
      key: `${x.customerId}-${i}`,
      title: x.customerName || x.customerId,
      sourceLine: meta.sourceLine,
      ownerLine: meta.ownerLine,
      detail: bits.join('；'),
    };
  }),
);

onBeforeUnmount(() => {
  clearContactDupState();
});

onMounted(() => {
  Promise.all([
    getRegionList().then((list) => {
      regionList.value = list;
    }),
    getCustomerSourceList({ scene: 'sea' }).then((list) => {
      customerSourceOptions.value = list.map((x) => ({ label: x.name, value: x.id }));
    }),
  ]);
});

const [Drawer, drawerApi] = useVbenDrawer({
  async onConfirm() {
    try {
      await formRef.value?.validate?.();
    } catch {
      return;
    }
    const v = formState.value;
    const getRegionName = (code: string | undefined) =>
      code ? regionList.value.find((r) => String(r.id) === String(code))?.name ?? '' : '';
    const customerSourceName =
      customerSourceOptions.value.find((o) => o.value === v.customerSourceId)?.label ?? '';
    const phoneCodes = (v.phoneRegionCodes ?? []) as string[];
    const projectCodes = (v.projectRegionCodes ?? []) as string[];
    const payload = {
      customerSourceId: v.customerSourceId,
      customerSourceName,
      mainContactName: v.mainContactName ?? '',
      mainContactPhone: v.mainContactPhone ?? '',
      contactQq: v.contactQq ?? '',
      contactWechat: v.contactWechat ?? '',
      phoneProvinceCode: phoneCodes[0] ?? '',
      phoneCityCode: phoneCodes[1] ?? '',
      phoneDistrictCode: phoneCodes[2] ?? '',
      phoneProvinceName: getRegionName(phoneCodes[0]),
      phoneCityName: getRegionName(phoneCodes[1]),
      phoneDistrictName: getRegionName(phoneCodes[2]),
      provinceCode: projectCodes[0] ?? '',
      cityCode: projectCodes[1] ?? '',
      districtCode: projectCodes[2] ?? '',
      provinceName: getRegionName(projectCodes[0]),
      cityName: getRegionName(projectCodes[1]),
      districtName: getRegionName(projectCodes[2]),
      consultationContent: v.consultationContent ?? '',
    };
    drawerApi.lock();
    try {
      if (editId.value) {
        const { consultationContent, ...seaPayload } = payload;
        await updateSeaCustomer(editId.value, seaPayload);
        if (accessStore.accessCodes?.includes(PermissionCodes.CustomerSeaConsultationEdit)) {
          await updateSeaCustomerConsultation(editId.value, consultationContent ?? '');
        }
      } else {
        const dupRes = await checkSeaCustomerDuplicateContacts({
          mainContactPhone: payload.mainContactPhone,
          contactQq: payload.contactQq,
          contactWechat: payload.contactWechat,
        });
        const dupItems = dupRes?.items ?? [];

        if (dupItems.length > 0) {
          drawerApi.lock(false);
          const content = h(
            'div',
            { style: { maxHeight: '50vh', overflow: 'auto' } },
            dupItems.map((x) => {
              const meta = formatDupMeta(x.customerSourceName, x.ownerName);
              const rows: Array<{ label: string; values: string[] }> = [
                { label: $t('customer.mainContactPhone') as string, values: x.duplicatePhones ?? [] },
                { label: $t('customer.contactQq') as string, values: x.duplicateQqs ?? [] },
                { label: $t('customer.contactWechat') as string, values: x.duplicateWechats ?? [] },
              ].filter((r) => (r.values?.length ?? 0) > 0);

              return h('div', { style: { marginBottom: '12px' } }, [
                h('div', { style: { fontWeight: 600, marginBottom: '4px' } }, x.customerName || x.customerId),
                h('div', { style: { marginBottom: '2px', opacity: 0.85 } }, meta.sourceLine),
                h('div', { style: { marginBottom: '4px', opacity: 0.85 } }, meta.ownerLine),
                ...rows.map((r) =>
                  h('div', { style: { marginBottom: '2px' } }, `${r.label}：${r.values.join('、')}`),
                ),
              ]);
            }),
          );

          const shouldContinue = await new Promise<boolean>((resolve) => {
            Modal.confirm({
              title: '联系方式重复提醒',
              content,
              okText: '继续录入',
              cancelText: '放弃录入',
              onOk: () => resolve(true),
              onCancel: () => resolve(false),
            });
          });

          if (!shouldContinue) {
            drawerApi.close();
            editId.value = null;
            return;
          }

          drawerApi.lock(true);
        }

        await createSeaCustomer(payload);
      }
      drawerApi.close();
      editId.value = null;
      emit('success');
    } finally {
      drawerApi.lock(false);
    }
  },
  onOpenChange(isOpen) {
    if (!isOpen) {
      clearContactDupState();
      return;
    }
    if (isOpen) {
      const data = drawerApi.getData<CustomerApi.CustomerDetail & { id?: string }>();
      const toCode = (v: unknown) =>
        v != null && String(v).trim() !== '' ? String(v) : undefined;
      if (data?.id) {
        editId.value = data.id;
        const phoneCodes = [
          toCode(data.phoneProvinceCode),
          toCode(data.phoneCityCode),
          toCode(data.phoneDistrictCode),
        ].filter(Boolean) as string[];
        const projectCodes = [
          toCode(data.provinceCode),
          toCode(data.cityCode),
          toCode(data.districtCode),
        ].filter(Boolean) as string[];
        formState.value = {
          customerSourceId: data.customerSourceId ?? '',
          fullName: data.fullName ?? '',
          mainContactName: data.mainContactName ?? '',
          mainContactPhone: data.mainContactPhone ?? '',
          contactQq: data.contactQq ?? '',
          contactWechat: data.contactWechat ?? '',
          phoneRegionCodes: phoneCodes.length > 0 ? phoneCodes : undefined,
          projectRegionCodes: projectCodes.length > 0 ? projectCodes : undefined,
          consultationContent: data.consultationContent ?? '',
        };
      } else {
        editId.value = null;
        formState.value = {
          customerSourceId: '',
          fullName: '',
          mainContactName: '',
          mainContactPhone: '',
          contactQq: '',
          contactWechat: '',
          phoneRegionCodes: undefined,
          projectRegionCodes: undefined,
          consultationContent: '',
        };
      }
    }
  },
});

defineExpose({
  open: () => drawerApi.open(),
  close: () => drawerApi.close(),
});
</script>

<template>
  <Drawer :title="editId ? $t('customer.edit') : $t('customer.seaCreate')">
    <Form
      ref="formRef"
      :model="formState"
      :rules="formRules"
      layout="vertical"
      class="mx-4"
    >
      <FormItem :label="$t('customer.customerSource')" name="customerSourceId" required>
        <Select
          v-model:value="formState.customerSourceId"
          :options="customerSourceOptions"
          :placeholder="$t('ui.formRules.required', [$t('customer.customerSource')])"
          class="w-full"
          show-search
          :filter-option="filterOption"
        />
      </FormItem>
      <FormItem :label="$t('customer.mainContactName')" name="mainContactName">
        <Input v-model:value="formState.mainContactName" />
      </FormItem>
      <FormItem :label="$t('customer.mainContactPhone')" name="mainContactPhone">
        <Input v-model:value="formState.mainContactPhone" />
      </FormItem>
      <FormItem :label="$t('customer.contactQq')" name="contactQq">
        <Input v-model:value="formState.contactQq" />
      </FormItem>
      <FormItem :label="$t('customer.contactWechat')" name="contactWechat">
        <Input v-model:value="formState.contactWechat" />
      </FormItem>
      <div v-if="!editId" class="mb-4">
        <Alert
          v-if="contactDupCheckError"
          type="error"
          show-icon
          :message="contactDupCheckError"
          class="mb-0"
        />
        <Alert
          v-else-if="contactDupLoading && hasSeaContactInput()"
          type="info"
          show-icon
          :message="$t('customer.contactDuplicateChecking')"
          class="mb-0"
        />
        <Alert
          v-else-if="contactDupItems.length > 0"
          type="warning"
          show-icon
          class="mb-0"
        >
          <template #message>{{ $t('customer.contactDuplicateWarnTitle') }}</template>
          <template #description>
            <div class="mt-1 space-y-2 text-sm">
              <div v-for="row in contactDupLines" :key="row.key">
                <span class="font-medium">{{ row.title }}</span>
                <div class="mt-0.5 text-xs opacity-80">{{ row.sourceLine }}</div>
                <div class="text-xs opacity-80">{{ row.ownerLine }}</div>
                <div v-if="row.detail" class="mt-0.5 opacity-80">{{ row.detail }}</div>
              </div>
              <div class="opacity-70">{{ $t('customer.contactDuplicateWarnHint') }}</div>
            </div>
          </template>
        </Alert>
      </div>
      <FormItem :label="$t('customer.phoneRegion')" name="phoneRegionCodes">
        <Cascader
          :key="'phone-region-' + regionOptionsReady"
          v-model:value="formState.phoneRegionCodes"
          :options="regionTreeOptions"
          :placeholder="$t('customer.locationRegionPlaceholder')"
          class="w-full"
          allow-clear
          :change-on-select="false"
          :show-search="{
            // 这里参数类型由 ant-design-vue 的 Cascader 定义，使用 any[] 以兼容 DefaultOptionType 的可选字段
            filter: (inputValue: string, options: any[]) =>
              options.some((node) => String(node?.label ?? '').toLowerCase().includes(inputValue.toLowerCase())),
          }"
        />
      </FormItem>
      <FormItem :label="$t('customer.projectRegion')" name="projectRegionCodes">
        <Cascader
          :key="'project-region-' + regionOptionsReady"
          v-model:value="formState.projectRegionCodes"
          :options="regionTreeOptions"
          :placeholder="$t('customer.locationRegionPlaceholder')"
          class="w-full"
          allow-clear
          :change-on-select="false"
          :show-search="{
            // 这里参数类型由 ant-design-vue 的 Cascader 定义，使用 any[] 以兼容 DefaultOptionType 的可选字段
            filter: (inputValue: string, options: any[]) =>
              options.some((node) => String(node?.label ?? '').toLowerCase().includes(inputValue.toLowerCase())),
          }"
        />
      </FormItem>
      <FormItem :label="$t('customer.consultationContent')" name="consultationContent" required>
        <Input.TextArea
          v-model:value="formState.consultationContent"
          :placeholder="$t('ui.formRules.required', [$t('customer.consultationContent')])"
          :rows="4"
        />
      </FormItem>
    </Form>
    <template #prepend-footer>
      <Button type="primary" danger @click="formRef?.resetFields()">{{ $t('common.reset') }}</Button>
    </template>
  </Drawer>
</template>
