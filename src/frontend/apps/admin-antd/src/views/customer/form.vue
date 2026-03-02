<script lang="ts" setup>
import type { RegionCascaderOption } from './data';

import { computed, onMounted, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';
import { ArrowLeft } from '@vben/icons';

import { Button, message } from 'ant-design-vue';

import { useAppConfig } from '@vben/hooks';
import { useVbenForm } from '#/adapter/form';
import { createCustomer, getCustomer, updateCustomer } from '#/api/system/customer';
import { getCustomerSourceList } from '#/api/system/customerSource';
import { getFileDownloadPath } from '#/api/system/file';
import { getIndustryList } from '#/api/system/industry';
import { getRegionList } from '#/api/system/region';
import type { RegionApi } from '#/api/system/region';
import { $t } from '#/locales';

import { useSchema } from './data';

const route = useRoute();
const router = useRouter();

const { apiURL } = useAppConfig(import.meta.env, import.meta.env.PROD);
const id = computed(() => route.params.id as string | undefined);
const isNew = computed(() => !id.value);

const industryOptions = ref<{ label: string; value: string }[]>([]);
const customerSourceOptions = ref<{ label: string; value: string }[]>([]);
const regionList = ref<RegionApi.RegionItem[]>([]);

/** 省级：与公海项目区域一致 */
const provinceFilter = (r: RegionApi.RegionItem) =>
  r.level === 2 || (Number(r.parentId) === 9 && r.level === 1);

/** 构建省市区级联树（与公海项目区域一致） */
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

onMounted(() => {
  Promise.all([
    getIndustryList().then((res) => {
      const list = Array.isArray(res) ? res : (res as any)?.data ?? [];
      industryOptions.value = list.map((x: { id: string; name: string }) => ({ label: x.name, value: x.id }));
    }),
    getCustomerSourceList().then((list) => {
      customerSourceOptions.value = list.map((x) => ({ label: x.name, value: x.id }));
    }),
    getRegionList().then((list) => {
      regionList.value = list;
    }),
  ]);
});

const [Form, formApi] = useVbenForm({
  layout: 'vertical',
  schema: computed(() =>
    useSchema(
      industryOptions.value,
      customerSourceOptions.value,
      regionTreeOptions.value,
      (path) => formApi.setFieldValue('businessLicense', path),
    ),
  ),
  showDefaultActions: false,
  wrapperClass: 'grid-cols-1 md:grid-cols-3 gap-x-6 gap-y-4',
});

async function loadCustomer() {
  if (!id.value) return;
  try {
    const detail = await getCustomer(id.value);
    const toStr = (v: unknown) =>
      v != null && String(v).trim() !== '' ? String(v) : undefined;
    const p = toStr(detail?.provinceCode);
    const c = toStr(detail?.cityCode);
    const d = toStr(detail?.districtCode);
    const regionCodes = [p, c, d].filter(Boolean) as string[];
    const licensePath = detail?.businessLicense ?? '';
    const licenseFileName = licensePath ? licensePath.split('/').pop() || $t('customer.businessLicense') : '';
    const licenseDownloadUrl = licensePath ? `${apiURL}${getFileDownloadPath(licensePath)}` : '';
    const isImageExt = (name: string) => /\.(bmp|gif|jpe?g|png|svg|webp)$/i.test(name);
    formApi.setValues({
      fullName: detail?.fullName ?? '',
      customerSourceId: detail?.customerSourceId ?? '',
      status: detail?.status,
      nature: detail?.nature,
      regionCodes: regionCodes.length > 0 ? regionCodes : undefined,
      coverRegion: detail?.coverRegion ?? '',
      registerAddress: detail?.registerAddress ?? '',
      employeeCount: detail?.employeeCount,
      businessLicense: licensePath,
      businessLicenseFileList:
        licensePath && licenseDownloadUrl
          ? [
              {
                uid: 'business-license',
                url: licenseDownloadUrl,
                name: licenseFileName || $t('customer.businessLicense'),
                thumbUrl: isImageExt(licenseFileName) ? licenseDownloadUrl : undefined,
              },
            ]
          : [],
      remark: detail?.remark ?? '',
      isHidden: detail?.isHidden ?? false,
      industryIds: detail?.industryIds ?? [],
    });
  } catch {
    message.error($t('ui.actionMessage.loadFailed'));
  }
}

watch(
  id,
  (v) => {
    if (v) loadCustomer();
    else formApi.resetForm();
  },
  { immediate: true },
);

function goBack() {
  router.push('/customer/list');
}

function resetForm() {
  formApi.resetForm();
  if (id.value) loadCustomer();
}

async function onSubmit() {
  const { valid } = await formApi.validate();
  if (!valid) return;
  const data = await formApi.getValues();
  const selectedSource = customerSourceOptions.value.find((o) => o.value === data.customerSourceId);
  const codes = (data.regionCodes as string[] | undefined) ?? [];
  const provinceCode = codes[0] ?? '';
  const cityCode = codes[1] ?? '';
  const districtCode = codes[2] ?? '';
  const getRegionName = (rid: string) =>
    regionList.value.find((r) => String(r.id) === String(rid))?.name ?? '';
  // 仅提交接口接受的字段；简称、负责人、主联系人、微信状态、是否重点客户由后端保护，不接收
  const payload = {
    fullName: String(data.fullName ?? ''),
    customerSourceId: String(data.customerSourceId ?? ''),
    customerSourceName: selectedSource?.label ?? '',
    status: data.status != null && data.status !== '' ? Number(data.status) : undefined,
    nature: data.nature != null && data.nature !== '' ? Number(data.nature) : undefined,
    provinceCode,
    cityCode,
    districtCode,
    provinceName: getRegionName(provinceCode),
    cityName: getRegionName(cityCode),
    districtName: getRegionName(districtCode),
    phoneProvinceCode: data.phoneProvinceCode ?? '',
    phoneCityCode: data.phoneCityCode ?? '',
    phoneDistrictCode: data.phoneDistrictCode ?? '',
    phoneProvinceName: data.phoneProvinceName ?? '',
    phoneCityName: data.phoneCityName ?? '',
    phoneDistrictName: data.phoneDistrictName ?? '',
    consultationContent: data.consultationContent ?? '',
    coverRegion: data.coverRegion ?? '',
    registerAddress: data.registerAddress ?? '',
    employeeCount: data.employeeCount != null && data.employeeCount !== '' ? Number(data.employeeCount) : 0,
    businessLicense:
      data.businessLicenseFileList?.length && data.businessLicense
        ? String(data.businessLicense)
        : undefined,
    contactQq: data.contactQq ?? '',
    contactWechat: data.contactWechat ?? '',
    remark: data.remark ?? '',
    isHidden: Boolean(data.isHidden),
    industryIds: Array.isArray(data.industryIds) ? data.industryIds : undefined,
  };
  if (!id.value) {
    (payload as Record<string, unknown>).ownerId = data.ownerId != null && data.ownerId !== '' ? Number(data.ownerId) : null;
  }
  try {
    if (id.value) {
      await updateCustomer(id.value, payload);
      message.success($t('ui.actionMessage.updateSuccess'));
    } else {
      await createCustomer(payload);
      message.success($t('ui.actionMessage.createSuccess'));
    }
    goBack();
  } catch {
    // error handled by request
  }
}
</script>

<template>
  <Page auto-content-height content-class="flex flex-col">
    <div class="mb-4 flex items-center gap-2">
      <Button type="text" @click="goBack">
        <ArrowLeft class="size-4" />
      </Button>
      <span class="text-lg font-medium">
        {{ isNew ? $t('customer.create') : $t('customer.edit') }}
      </span>
    </div>

    <div class="w-full flex-1 min-w-0">
      <Form />
      <!-- 客户联系人 -->
      <div class="mt-8 border-t border-gray-200 pt-6">
        <div class="mb-3 flex items-center justify-between">
          <span class="text-base font-medium">{{ $t('customer.customerContacts') }}</span>
          <Button type="primary" class="inline-flex items-center gap-1">
            + {{ $t('customer.addContact') }}
          </Button>
        </div>
      </div>
      <!-- 客户联系记录 -->
      <div class="mt-6 border-t border-gray-200 pt-6">
        <div class="mb-3 flex items-center justify-between">
          <span class="text-base font-medium">{{ $t('customer.contactRecords') }}</span>
          <Button type="primary" class="inline-flex items-center gap-1">
            + {{ $t('customer.addContact') }}
          </Button>
        </div>
      </div>
      <div class="mt-6 flex gap-2">
        <Button type="primary" @click="onSubmit">{{ $t('common.confirm') }}</Button>
        <Button type="primary" danger @click="resetForm">{{ $t('common.reset') }}</Button>
        <Button @click="goBack">{{ $t('common.cancel') }}</Button>
      </div>
    </div>
  </Page>
</template>
