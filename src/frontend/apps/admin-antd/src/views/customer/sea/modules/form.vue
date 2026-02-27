<script lang="ts" setup>
import type { RegionApi } from '#/api/system/region';

import { computed, nextTick, onMounted, ref, watch } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { Button, Form, FormItem, Input, Select } from 'ant-design-vue';

function filterOption(input: string, option: unknown) {
  const o = option as { label?: string } | undefined;
  const label = o?.label ?? '';
  return label.toLowerCase().includes((input ?? '').toLowerCase());
}

import type { CustomerApi } from '#/api/system/customer';
import { createCustomer, updateSeaCustomer } from '#/api/system/customer';
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
  statusId: 0,
  mainContactName: '',
  mainContactPhone: '',
  contactQq: '',
  contactWechat: '',
  phoneProvinceCode: undefined as string | undefined,
  phoneCityCode: undefined as string | undefined,
  phoneDistrictCode: undefined as string | undefined,
  provinceCode: undefined as string | undefined,
  cityCode: undefined as string | undefined,
  districtCode: undefined as string | undefined,
  consultationContent: '',
});

/** 省级：level=2 的省份 + 港澳台（parentId=9 且 level=1） */
const provinceFilter = (r: RegionApi.RegionItem) =>
  r.level === 2 || (Number(r.parentId) === 9 && r.level === 1);

const phoneProvinceOptions = computed(() =>
  regionList.value
    .filter(provinceFilter)
    .map((r) => ({ label: r.name, value: String(r.id) })),
);
const phoneCityOptions = computed(() => {
  const pid = formState.value.phoneProvinceCode;
  if (!pid) return [];
  const parentStr = String(pid);
  return regionList.value
    .filter((r) => String(r.parentId) === parentStr)
    .map((r) => ({ label: r.name, value: String(r.id) }));
});
const phoneDistrictOptions = computed(() => {
  const cid = formState.value.phoneCityCode;
  if (!cid) return [];
  const parentStr = String(cid);
  return regionList.value
    .filter((r) => String(r.parentId) === parentStr)
    .map((r) => ({ label: r.name, value: String(r.id) }));
});

const projectProvinceOptions = computed(() =>
  regionList.value
    .filter(provinceFilter)
    .map((r) => ({ label: r.name, value: String(r.id) })),
);
const projectCityOptions = computed(() => {
  const pid = formState.value.provinceCode;
  if (!pid) return [];
  const parentStr = String(pid);
  return regionList.value
    .filter((r) => String(r.parentId) === parentStr)
    .map((r) => ({ label: r.name, value: String(r.id) }));
});
const projectDistrictOptions = computed(() => {
  const cid = formState.value.cityCode;
  if (!cid) return [];
  const parentStr = String(cid);
  return regionList.value
    .filter((r) => String(r.parentId) === parentStr)
    .map((r) => ({ label: r.name, value: String(r.id) }));
});

/** 当前省下是否有市选项（无则市/区可不选，如港澳台） */
function hasCityOptions(provinceCode: string | undefined) {
  if (provinceCode == null) return false;
  const pid = String(provinceCode);
  return regionList.value.some((r) => String(r.parentId) === pid);
}
/** 当前市下是否有区选项（无则区可不选） */
function hasDistrictOptions(cityCode: string | undefined) {
  if (cityCode == null) return false;
  const cid = String(cityCode);
  return regionList.value.some((r) => String(r.parentId) === cid);
}

watch(
  () => formState.value.phoneProvinceCode,
  () => {
    formState.value.phoneCityCode = undefined;
    formState.value.phoneDistrictCode = undefined;
    nextTick(() => {
      formRef.value?.clearValidate?.(['phoneCityCode', 'phoneDistrictCode']);
    });
  },
);
watch(
  () => formState.value.phoneCityCode,
  () => {
    formState.value.phoneDistrictCode = undefined;
    nextTick(() => {
      formRef.value?.clearValidate?.(['phoneDistrictCode']);
    });
  },
);
watch(
  () => formState.value.provinceCode,
  () => {
    formState.value.cityCode = undefined;
    formState.value.districtCode = undefined;
    nextTick(() => {
      formRef.value?.clearValidate?.(['cityCode', 'districtCode']);
    });
  },
);
watch(
  () => formState.value.cityCode,
  () => {
    formState.value.districtCode = undefined;
    nextTick(() => {
      formRef.value?.clearValidate?.(['districtCode']);
    });
  },
);

/** 市/区仅在有下级选项时必填，用 computed 随当前省/市变化 */
const formRules = computed(() => {
  const phoneProvince = formState.value.phoneProvinceCode;
  const phoneCity = formState.value.phoneCityCode;
  const projectProvince = formState.value.provinceCode;
  const projectCity = formState.value.cityCode;
  const requiredPhoneRegion = $t('ui.formRules.required', [$t('customer.phoneRegion')]) as string;
  const requiredProjectRegion = $t('ui.formRules.required', [$t('customer.projectRegion')]) as string;
  return {
    customerSourceId: [{ required: true, message: $t('ui.formRules.required', [$t('customer.customerSource')]) }],
    fullName: [{ required: true, message: $t('ui.formRules.required', [$t('customer.fullName')]) }],
    phoneProvinceCode: [{ required: true, message: requiredPhoneRegion }],
    phoneCityCode: hasCityOptions(phoneProvince)
      ? [{ required: true, message: requiredPhoneRegion }]
      : [],
    phoneDistrictCode: hasDistrictOptions(phoneCity)
      ? [{ required: true, message: requiredPhoneRegion }]
      : [],
    provinceCode: [{ required: true, message: requiredProjectRegion }],
    cityCode: hasCityOptions(projectProvince)
      ? [{ required: true, message: requiredProjectRegion }]
      : [],
    districtCode: hasDistrictOptions(projectCity)
      ? [{ required: true, message: requiredProjectRegion }]
      : [],
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
    getCustomerSourceList().then((list) => {
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
    const payload = {
      fullName: v.fullName,
      customerSourceId: v.customerSourceId,
      customerSourceName,
      statusId: v.statusId ?? 0,
      mainContactName: v.mainContactName || undefined,
      mainContactPhone: v.mainContactPhone || undefined,
      contactQq: v.contactQq || undefined,
      contactWechat: v.contactWechat || undefined,
      phoneProvinceCode: v.phoneProvinceCode ?? '',
      phoneCityCode: v.phoneCityCode ?? '',
      phoneDistrictCode: v.phoneDistrictCode ?? '',
      phoneProvinceName: getRegionName(v.phoneProvinceCode),
      phoneCityName: getRegionName(v.phoneCityCode),
      phoneDistrictName: getRegionName(v.phoneDistrictCode),
      provinceCode: v.provinceCode ?? '',
      cityCode: v.cityCode ?? '',
      districtCode: v.districtCode ?? '',
      provinceName: getRegionName(v.provinceCode),
      cityName: getRegionName(v.cityCode),
      districtName: getRegionName(v.districtCode),
      consultationContent: v.consultationContent ?? '',
    };
    drawerApi.lock();
    try {
      if (editId.value) {
        await updateSeaCustomer(editId.value, payload);
      } else {
        await createCustomer(payload);
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
      if (data?.id) {
        editId.value = data.id;
        formState.value = {
          customerSourceId: data.customerSourceId ?? '',
          fullName: data.fullName ?? '',
          statusId: data.statusId ?? 0,
          mainContactName: data.mainContactName ?? '',
          mainContactPhone: data.mainContactPhone ?? '',
          contactQq: data.contactQq ?? '',
          contactWechat: data.contactWechat ?? '',
          phoneProvinceCode: data.phoneProvinceCode ?? undefined,
          phoneCityCode: data.phoneCityCode ?? undefined,
          phoneDistrictCode: data.phoneDistrictCode ?? undefined,
          provinceCode: data.provinceCode ?? undefined,
          cityCode: data.cityCode ?? undefined,
          districtCode: data.districtCode ?? undefined,
          consultationContent: data.consultationContent ?? '',
        };
      } else {
        editId.value = null;
        formState.value = {
          customerSourceId: '',
          fullName: '',
          statusId: 0,
          mainContactName: '',
          mainContactPhone: '',
          contactQq: '',
          contactWechat: '',
          phoneProvinceCode: undefined,
          phoneCityCode: undefined,
          phoneDistrictCode: undefined,
          provinceCode: undefined,
          cityCode: undefined,
          districtCode: undefined,
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
      <FormItem :label="$t('customer.fullName')" name="fullName" required>
        <Input v-model:value="formState.fullName" :placeholder="$t('ui.formRules.required', [$t('customer.fullName')])" />
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
      <FormItem :label="$t('customer.phoneRegion')" name="phoneProvinceCode" required>
        <Select
          v-model:value="formState.phoneProvinceCode"
          :options="phoneProvinceOptions"
          :placeholder="$t('customer.province')"
          class="w-full"
          allow-clear
          show-search
          :filter-option="filterOption"
        />
      </FormItem>
      <FormItem
        :label="' '"
        name="phoneCityCode"
        :required="hasCityOptions(formState.phoneProvinceCode)"
        :label-col="{ span: 0 }"
        :wrapper-col="{ span: 24 }"
      >
        <Select
          v-model:value="formState.phoneCityCode"
          :options="phoneCityOptions"
          :placeholder="$t('customer.city')"
          class="w-full"
          allow-clear
          show-search
          :filter-option="filterOption"
          :disabled="!formState.phoneProvinceCode"
        />
      </FormItem>
      <FormItem
        :label="' '"
        name="phoneDistrictCode"
        :required="hasDistrictOptions(formState.phoneCityCode)"
        :label-col="{ span: 0 }"
        :wrapper-col="{ span: 24 }"
      >
        <Select
          v-model:value="formState.phoneDistrictCode"
          :options="phoneDistrictOptions"
          :placeholder="$t('customer.district')"
          class="w-full"
          allow-clear
          show-search
          :filter-option="filterOption"
          :disabled="!formState.phoneCityCode"
        />
      </FormItem>
      <FormItem :label="$t('customer.projectRegion')" name="provinceCode" required>
        <Select
          v-model:value="formState.provinceCode"
          :options="projectProvinceOptions"
          :placeholder="$t('customer.province')"
          class="w-full"
          allow-clear
          show-search
          :filter-option="filterOption"
        />
      </FormItem>
      <FormItem
        :label="' '"
        name="cityCode"
        :required="hasCityOptions(formState.provinceCode)"
        :label-col="{ span: 0 }"
        :wrapper-col="{ span: 24 }"
      >
        <Select
          v-model:value="formState.cityCode"
          :options="projectCityOptions"
          :placeholder="$t('customer.city')"
          class="w-full"
          allow-clear
          show-search
          :filter-option="filterOption"
          :disabled="!formState.provinceCode"
        />
      </FormItem>
      <FormItem
        :label="' '"
        name="districtCode"
        :required="hasDistrictOptions(formState.cityCode)"
        :label-col="{ span: 0 }"
        :wrapper-col="{ span: 24 }"
      >
        <Select
          v-model:value="formState.districtCode"
          :options="projectDistrictOptions"
          :placeholder="$t('customer.district')"
          class="w-full"
          allow-clear
          show-search
          :filter-option="filterOption"
          :disabled="!formState.cityCode"
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
