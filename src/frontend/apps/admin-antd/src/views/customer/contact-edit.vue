<script lang="ts" setup>
import { computed, onActivated, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';
import { Button, Card, message, Space, Table, Tag } from 'ant-design-vue';

import { useVbenForm, z } from '#/adapter/form';
import type { CustomerApi } from '#/api/system/customer';
import { getCustomer, updateCustomerContact } from '#/api/system/customer';
import { $t } from '#/locales';
import {
  hasAtLeastOneContactChannel,
  isValidMobileIfPresent,
  isValidQqIfPresent,
} from '#/utils/customer-contact-validation';
import {
  formatCustomerContactRecordStatus,
  formatCustomerContactRecordType,
} from '#/utils/customer-contact-record-display';

const route = useRoute();
const router = useRouter();

const customerId = computed(() => String(route.params.customerId ?? ''));
const contactId = computed(() => String(route.params.contactId ?? ''));

const loading = ref(false);
const detail = ref<CustomerApi.CustomerDetail | null>(null);
const contact = computed(() => {
  const list = detail.value?.contacts ?? [];
  return list.find((c) => String(c.id) === contactId.value) ?? null;
});

const contactRecords = computed(() => {
  const list = detail.value?.contactRecords ?? [];
  return list.filter((r) => (r.customerContactIds ?? []).map(String).includes(contactId.value));
});

const [Form, formApi] = useVbenForm({
  layout: 'vertical',
  showDefaultActions: false,
  wrapperClass: 'grid-cols-1 sm:grid-cols-2 gap-x-6 gap-y-4',
  schema: [
    {
      component: 'Input',
      componentProps: { class: 'w-full', placeholder: '姓名' },
      fieldName: 'name',
      label: '姓名',
      rules: z.string().min(1, '姓名必填'),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full', placeholder: '职位' },
      fieldName: 'position',
      label: '职位',
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full', placeholder: '手机号' },
      fieldName: 'mobile',
      label: '手机号',
      rules: z.any().refine(isValidMobileIfPresent, $t('customer.contactMobileInvalid')),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full', placeholder: '办公电话' },
      fieldName: 'phone',
      label: '办公电话',
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full', placeholder: 'QQ' },
      fieldName: 'qq',
      label: 'QQ',
      rules: z.any().refine(isValidQqIfPresent, $t('customer.contactQqInvalid')),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full', placeholder: '微信' },
      fieldName: 'wechat',
      label: '微信',
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full', placeholder: '邮箱' },
      fieldName: 'email',
      label: '邮箱',
    },
    {
      component: 'Switch',
      componentProps: {
        checkedChildren: '是',
        unCheckedChildren: '否',
      },
      fieldName: 'isWechatAdded',
      label: '微信添加',
    },
  ] as any,
});

const recordColumns = [
  { title: '类型', dataIndex: 'recordType', key: 'recordType', width: 100 },
  { title: '拜访日期', dataIndex: 'recordAt', key: 'recordAt', width: 170 },
  { title: '下次拜访日期', dataIndex: 'nextVisitAt', key: 'nextVisitAt', width: 170 },
  { title: '状态', dataIndex: 'status', key: 'status', width: 100 },
  { title: '沟通内容', dataIndex: 'content', key: 'content', ellipsis: true },
  { title: '操作', key: 'action', width: 140 },
];

function formatDateTime(v?: string) {
  if (!v) return '';
  const d = new Date(v);
  return Number.isNaN(d.getTime()) ? v : d.toLocaleString('zh-CN');
}

async function load() {
  if (!customerId.value) return;
  loading.value = true;
  try {
    detail.value = await getCustomer(customerId.value);
    const c = contact.value;
    if (!c) {
      message.error('未找到联系人');
      return;
    }
    formApi.setValues({
      name: c.name ?? '',
      position: c.position ?? '',
      mobile: c.mobile ?? '',
      phone: c.phone ?? '',
      email: c.email ?? '',
      qq: (c as any).qq ?? '',
      wechat: (c as any).wechat ?? '',
      isWechatAdded: Boolean((c as any).isWechatAdded ?? false),
    });
  } finally {
    loading.value = false;
  }
}

async function saveContact() {
  const { valid } = await formApi.validate();
  if (!valid) return;
  const c = contact.value;
  if (!c) return;
  const data = await formApi.getValues();
  if (!hasAtLeastOneContactChannel(data.mobile, data.phone, data.qq, data.wechat)) {
    message.warning($t('customer.contactChannelAtLeastOne'));
    return;
  }
  await updateCustomerContact(customerId.value, c.id, {
    // 保持后端现有字段兼容：contactType/gender/birthday/isPrimary 从原对象带回
    name: String(data.name ?? ''),
    contactType: c.contactType ?? '',
    gender: Number(c.gender ?? 0),
    birthday: c.birthday ?? new Date().toISOString(),
    position: String(data.position ?? ''),
    mobile: String(data.mobile ?? ''),
    phone: String(data.phone ?? ''),
    email: String(data.email ?? ''),
    qq: String(data.qq ?? ''),
    wechat: String(data.wechat ?? ''),
    isWechatAdded: Boolean(data.isWechatAdded),
    isPrimary: Boolean(c.isPrimary),
  });
  message.success('保存成功');
  await load();
}

function openAddRecord() {
  router.push({
    name: 'CustomerContactRecordCreate',
    params: { customerId: customerId.value },
    query: { contactIds: contactId.value },
  });
}

function openEditRecord(record: CustomerApi.CustomerContactRecordItem) {
  router.push({
    name: 'CustomerContactRecordEdit',
    params: { customerId: customerId.value, recordId: record.id },
  });
}

onMounted(load);

const skipFirstActivatedRefresh = ref(true);
onActivated(() => {
  if (skipFirstActivatedRefresh.value) {
    skipFirstActivatedRefresh.value = false;
    return;
  }
  if (customerId.value && contactId.value) load();
});
</script>

<template>
  <Page auto-content-height content-class="flex flex-col">
    <div class="w-full flex-1 min-w-0">
      <div class="mb-4 flex items-center justify-between">
        <Space>
          <Button @click="router.back()">返回</Button>
        </Space>
        <Button type="primary" :loading="loading" @click="saveContact">保存联系人</Button>
      </div>

      <Card title="联系人信息" :loading="loading">
        <Form />
      </Card>

      <Card class="mt-6" title="客户联系记录">
        <template #extra>
          <Button type="primary" @click="openAddRecord">新增</Button>
        </template>
        <Table
          :columns="recordColumns"
          :data-source="contactRecords"
          :pagination="false"
          row-key="id"
          size="small"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'recordType'">
              {{ formatCustomerContactRecordType(record.recordType) }}
            </template>
            <template v-else-if="column.key === 'recordAt'">
              {{ formatDateTime(record.recordAt) }}
            </template>
            <template v-else-if="column.key === 'nextVisitAt'">
              {{ formatDateTime(record.nextVisitAt) }}
            </template>
            <template v-else-if="column.key === 'status'">
              <Tag>{{ formatCustomerContactRecordStatus(record.status ?? record.statusId) }}</Tag>
            </template>
            <template v-else-if="column.key === 'action'">
              <Space>
                <Button
                  type="link"
                  size="small"
                  @click="openEditRecord(record as CustomerApi.CustomerContactRecordItem)"
                >
                  查看/修改
                </Button>
              </Space>
            </template>
          </template>
        </Table>

      </Card>
    </div>
  </Page>
</template>

