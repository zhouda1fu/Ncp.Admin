<script lang="ts" setup>
import type { RegionApi } from '#/api/system/region';

import { computed, onMounted, ref, watch } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { Button, Cascader, Form, FormItem, Input, Select } from 'ant-design-vue';

import type { RegionCascaderOption } from '../../data';

function filterOption(input: string, option: unknown) {
  const o = option as { label?: string } | undefined;
  const label = o?.label ?? '';
  return label.toLowerCase().includes((input ?? '').toLowerCase());
}

import type { CustomerApi } from '#/api/system/customer';
import { createSeaCustomer, updateSeaCustomer } from '#/api/system/customer';
import { getCustomerSourceList } from '#/api/system/customerSource';
import { getRegionList } from '#/api/system/region';
import { $t } from '#/locales';

const emit = defineEmits(['success']);

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
  const requiredPhoneRegion = $t('ui.formRules.required', [$t('customer.phoneRegion')]) as string;
  const requiredProjectRegion = $t('ui.formRules.required', [$t('customer.projectRegion')]) as string;
  return {
    customerSourceId: [{ required: true, message: $t('ui.formRules.required', [$t('customer.customerSource')]) }],
    phoneRegionCodes: [
      {
        required: true,
        message: requiredPhoneRegion,
        validator: (_: unknown, value: string[] | undefined) => {
          if (value && value.length > 0) return Promise.resolve();
          return Promise.reject(new Error(requiredPhoneRegion));
        },
      },
    ],
    projectRegionCodes: [
      {
        required: true,
        message: requiredProjectRegion,
        validator: (_: unknown, value: string[] | undefined) => {
          if (value && value.length > 0) return Promise.resolve();
          return Promise.reject(new Error(requiredProjectRegion));
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
        await updateSeaCustomer(editId.value, payload);
      } else {
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
      <FormItem :label="$t('customer.phoneRegion')" name="phoneRegionCodes" required>
        <Cascader
          :key="'phone-region-' + regionOptionsReady"
          v-model:value="formState.phoneRegionCodes"
          :options="regionTreeOptions"
          :placeholder="$t('customer.locationRegionPlaceholder')"
          class="w-full"
          allow-clear
          :change-on-select="false"
          :show-search="{
            filter: (inputValue: string, path: { label: string }[]) =>
              path.some((node) => node.label.toLowerCase().includes(inputValue.toLowerCase())),
          }"
        />
      </FormItem>
      <FormItem :label="$t('customer.projectRegion')" name="projectRegionCodes" required>
        <Cascader
          :key="'project-region-' + regionOptionsReady"
          v-model:value="formState.projectRegionCodes"
          :options="regionTreeOptions"
          :placeholder="$t('customer.locationRegionPlaceholder')"
          class="w-full"
          allow-clear
          :change-on-select="false"
          :show-search="{
            filter: (inputValue: string, path: { label: string }[]) =>
              path.some((node) => node.label.toLowerCase().includes(inputValue.toLowerCase())),
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
