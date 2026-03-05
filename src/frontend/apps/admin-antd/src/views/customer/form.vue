<script lang="ts" setup>
import type { RegionCascaderOption, IndustryTreeOption } from './data';

import { computed, onMounted, onUnmounted, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';
import { ArrowLeft } from '@vben/icons';

import { Button, message, Modal, Space, Table, Tag } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import {
  createCustomer,
  getCustomer,
  removeCustomerContact,
  removeCustomerContactRecord,
  updateCustomer,
} from '#/api/system/customer';
import type { CustomerApi } from '#/api/system/customer';
import { getCustomerSourceList } from '#/api/system/customerSource';
import { fetchFileBlob } from '#/api/system/file';
import { getIndustryList } from '#/api/system/industry';
import { getRegionList } from '#/api/system/region';
import type { RegionApi } from '#/api/system/region';
import { $t } from '#/locales';

import ContactDrawer from './modules/contact-drawer.vue';
import ContactRecordDrawer from './modules/contact-record-drawer.vue';
import { useSchema } from './data';

const route = useRoute();
const router = useRouter();

const id = computed(() => route.params.id as string | undefined);
const isNew = computed(() => !id.value);

const industryTreeOptions = ref<IndustryTreeOption[]>([]);
const customerSourceOptions = ref<{ label: string; value: string }[]>([]);
const regionList = ref<RegionApi.RegionItem[]>([]);
const customerDetail = ref<CustomerApi.CustomerDetail | null>(null);
const contactDrawerRef = ref<InstanceType<typeof ContactDrawer> | null>(null);
const contactRecordDrawerRef = ref<InstanceType<typeof ContactRecordDrawer> | null>(null);
const licenseBlobUrl = ref<string | null>(null);
const submitting = ref(false);
/** 新建客户幂等键：同一表单提交（含重复点击）使用同一 key，后端返回缓存响应防重复创建 */
const createIdempotencyKey = ref<string | null>(null);
const contactList = computed(() => customerDetail.value?.contacts ?? []);
const contactRecordList = computed(() => customerDetail.value?.contactRecords ?? []);

const contactColumns = [
  { title: () => $t('customer.contactName'), dataIndex: 'name', key: 'name', width: 120 },
  { title: () => $t('customer.contactType'), dataIndex: 'contactType', key: 'contactType', width: 100 },
  { title: () => $t('customer.contactMobile'), dataIndex: 'mobile', key: 'mobile', width: 120 },
  { title: () => $t('customer.contactPhone'), dataIndex: 'phone', key: 'phone', width: 120 },
  { title: () => $t('customer.contactPosition'), dataIndex: 'position', key: 'position', width: 100 },
  { title: () => $t('customer.isPrimary'), key: 'isPrimary', width: 90 },
  { title: () => $t('customer.operation'), key: 'action', width: 160 },
];

const contactRecordColumns = [
  { title: () => $t('customer.recordAt'), dataIndex: 'recordAt', key: 'recordAt', width: 170 },
  { title: () => $t('customer.recordType'), dataIndex: 'recordType', key: 'recordType', width: 100 },
  { title: () => $t('customer.recordContent'), dataIndex: 'content', key: 'content', ellipsis: true },
  { title: () => $t('customer.recorderName'), dataIndex: 'recorderName', key: 'recorderName', width: 100 },
  { title: () => $t('customer.operation'), key: 'action', width: 100 },
];

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

/** 将扁平的行业列表转为树形（父子层级、按 sortOrder 排序） */
function buildIndustryTree(
  list: { id: string; name: string; parentId?: string; sortOrder: number }[],
): IndustryTreeOption[] {
  const sorted = [...list].sort((a, b) => a.sortOrder - b.sortOrder);
  interface Node extends IndustryTreeOption {
    sortOrder: number;
    children: Node[];
  }
  const map = new Map<string, Node>();
  for (const x of sorted) {
    map.set(x.id, { label: x.name, value: x.id, sortOrder: x.sortOrder, children: [] });
  }
  const roots: Node[] = [];
  for (const x of sorted) {
    const node = map.get(x.id)!;
    const parentId = x.parentId && String(x.parentId).trim() ? x.parentId : undefined;
    if (!parentId || !map.has(parentId)) {
      roots.push(node);
    } else {
      map.get(parentId)!.children.push(node);
    }
  }
  function toOption(n: Node, isRoot: boolean): IndustryTreeOption {
    const children = n.children.length > 0 ? n.children.sort((a, b) => a.sortOrder - b.sortOrder).map((c) => toOption(c, false)) : undefined;
    const label = isRoot ? `▸ ${n.label}` : n.label;
    return { label, value: n.value, children };
  }
  return roots.sort((a, b) => a.sortOrder - b.sortOrder).map((r) => toOption(r, true));
}

onMounted(() => {
  Promise.all([
    getIndustryList().then((res) => {
      const list = Array.isArray(res) ? res : (res as any)?.data ?? [];
      industryTreeOptions.value = buildIndustryTree(list);
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
      industryTreeOptions.value,
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
    customerDetail.value = detail ?? null;
    const toStr = (v: unknown) =>
      v != null && String(v).trim() !== '' ? String(v) : undefined;
    const p = toStr(detail?.provinceCode);
    const c = toStr(detail?.cityCode);
    const d = toStr(detail?.districtCode);
    const regionCodes = [p, c, d].filter(Boolean) as string[];
    const licensePath = detail?.businessLicense ?? '';
    const licenseFileName = licensePath ? licensePath.split('/').pop() || $t('customer.businessLicense') : '';
    const isImageExt = (name: string) => /\.(bmp|gif|jpe?g|png|svg|webp)$/i.test(name);
    if (licenseBlobUrl.value) {
      URL.revokeObjectURL(licenseBlobUrl.value);
      licenseBlobUrl.value = null;
    }
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
      businessLicenseFileList: licensePath
        ? [
            {
              uid: 'business-license',
              name: licenseFileName || $t('customer.businessLicense'),
              url: undefined as string | undefined,
              thumbUrl: undefined as string | undefined,
            },
          ]
        : [],
      remark: detail?.remark ?? '',
      isHidden: detail?.isHidden ?? false,
      industryIds: detail?.industryIds ?? [],
    });
    if (licensePath) {
      try {
        const blob = await fetchFileBlob(licensePath);
        const blobUrl = URL.createObjectURL(blob);
        licenseBlobUrl.value = blobUrl;
        formApi.setFieldValue('businessLicenseFileList', [
          {
            uid: 'business-license',
            name: licenseFileName || $t('customer.businessLicense'),
            url: blobUrl,
            thumbUrl: isImageExt(licenseFileName) ? blobUrl : undefined,
          },
        ]);
      } catch {
        // 预览加载失败时保留文件名，仅无缩略图/预览
      }
    }
  } catch {
    message.error($t('ui.actionMessage.loadFailed'));
  }
}

onUnmounted(() => {
  if (licenseBlobUrl.value) {
    URL.revokeObjectURL(licenseBlobUrl.value);
    licenseBlobUrl.value = null;
  }
});

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

function openContactDrawer(contact?: CustomerApi.CustomerContactItem | Record<string, unknown>) {
  contactDrawerRef.value?.open(contact as CustomerApi.CustomerContactItem | undefined);
}

function onContactSuccess() {
  loadCustomer();
}

async function handleDeleteContact(contact: CustomerApi.CustomerContactItem | Record<string, unknown>) {
  const c = contact as CustomerApi.CustomerContactItem;
  if (!id.value) return;
  Modal.confirm({
    title: $t('customer.confirmDeleteContact'),
    okText: $t('common.confirm'),
    okType: 'danger',
    cancelText: $t('common.cancel'),
    async onOk() {
      await removeCustomerContact(id.value!, c.id);
      message.success($t('ui.actionMessage.deleteSuccess'));
      loadCustomer();
    },
  });
}

function openContactRecordDrawer() {
  contactRecordDrawerRef.value?.open();
}

function onContactRecordSuccess() {
  loadCustomer();
}

async function handleDeleteContactRecord(record: CustomerApi.CustomerContactRecordItem) {
  if (!id.value) return;
  Modal.confirm({
    title: $t('customer.confirmDeleteRecord'),
    okText: $t('common.confirm'),
    okType: 'danger',
    cancelText: $t('common.cancel'),
    async onOk() {
      await removeCustomerContactRecord(id.value!, record.id);
      message.success($t('ui.actionMessage.deleteSuccess'));
      loadCustomer();
    },
  });
}

function formatRecordAt(recordAt: string) {
  if (!recordAt) return '';
  try {
    const d = new Date(recordAt);
    return d.toLocaleString('zh-CN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
    });
  } catch {
    return recordAt;
  }
}

async function onSubmit() {
  if (submitting.value) return;
  const { valid } = await formApi.validate();
  if (!valid) return;
  submitting.value = true;
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
      createIdempotencyKey.value = createIdempotencyKey.value ?? crypto.randomUUID();
      await createCustomer(payload, { idempotencyKey: createIdempotencyKey.value });
      message.success($t('ui.actionMessage.createSuccess'));
    }
    goBack();
  } catch {
    // error handled by request
  } finally {
    submitting.value = false;
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
      <!-- 客户联系人（仅编辑页展示） -->
      <template v-if="id">
        <div class="mt-8 border-t border-gray-200 pt-6">
          <div class="mb-3 flex items-center justify-between">
            <span class="text-base font-medium">{{ $t('customer.customerContacts') }}</span>
            <Button type="primary" class="inline-flex items-center gap-1" @click="openContactDrawer()">
              + {{ $t('customer.addContact') }}
            </Button>
          </div>
          <Table
            :columns="contactColumns"
            :data-source="contactList"
            :pagination="false"
            row-key="id"
            size="small"
            class="mt-2"
          >
            <template #bodyCell="{ column, record }">
              <template v-if="column.key === 'isPrimary'">
                <Tag v-if="record.isPrimary" color="blue">{{ $t('customer.isPrimary') }}</Tag>
              </template>
              <template v-else-if="column.key === 'action'">
                <Space>
                  <Button type="link" size="small" @click="openContactDrawer(record)">
                    {{ $t('customer.editContact') }}
                  </Button>
                  <Button type="link" size="small" danger @click="handleDeleteContact(record)">
                    {{ $t('customer.deleteContact') }}
                  </Button>
                </Space>
              </template>
            </template>
          </Table>
        </div>
        <ContactDrawer
          v-if="id"
          ref="contactDrawerRef"
          :customer-id="id"
          @success="onContactSuccess"
        />
      </template>
      <!-- 客户联系记录 -->
      <div class="mt-6 border-t border-gray-200 pt-6">
        <div class="mb-3 flex items-center justify-between">
          <span class="text-base font-medium">{{ $t('customer.contactRecords') }}</span>
          <Button
            v-if="id"
            type="primary"
            class="inline-flex items-center gap-1"
            @click="openContactRecordDrawer"
          >
            + {{ $t('customer.addContactRecord') }}
          </Button>
        </div>
        <Table
          v-if="id"
          :columns="contactRecordColumns"
          :data-source="contactRecordList"
          :pagination="false"
          row-key="id"
          size="small"
          class="mt-2"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'recordAt'">
              {{ formatRecordAt(record.recordAt) }}
            </template>
            <template v-else-if="column.key === 'action'">
              <Button
                type="link"
                size="small"
                danger
                @click="handleDeleteContactRecord(record as CustomerApi.CustomerContactRecordItem)"
              >
                {{ $t('customer.deleteRecord') }}
              </Button>
            </template>
          </template>
        </Table>
        <ContactRecordDrawer
          v-if="id"
          ref="contactRecordDrawerRef"
          :customer-id="id"
          @success="onContactRecordSuccess"
        />
      </div>
      <div class="mt-6 flex gap-2">
        <Button type="primary" :loading="submitting" :disabled="submitting" @click="onSubmit">{{ $t('common.confirm') }}</Button>
        <Button type="primary" danger @click="resetForm">{{ $t('common.reset') }}</Button>
        <Button @click="goBack">{{ $t('common.cancel') }}</Button>
      </div>
    </div>
  </Page>
</template>
