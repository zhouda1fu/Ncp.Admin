<script lang="ts" setup>
import { computed, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';

import type { UploadFile } from 'ant-design-vue';
import {
  Button,
  Card,
  InputNumber,
  message,
  Space,
  Timeline,
  TimelineItem,
  Upload,
} from 'ant-design-vue';

import { useVbenForm, z } from '#/adapter/form';
import type { CustomerApi } from '#/api/system/customer';
import { addCustomerContactRecord, getCustomer, updateCustomerContactRecord } from '#/api/system/customer';
import { uploadFile } from '#/api/system/file';
import { $t } from '#/locales';
import {
  coerceRecordTypeToId,
  formatCustomerContactRecordType,
} from '#/utils/customer-contact-record-display';

const route = useRoute();
const router = useRouter();

const customerId = computed(() => String(route.params.customerId ?? ''));
const recordId = computed(() => {
  const id = route.params.recordId;
  return id ? String(id) : undefined;
});

const loading = ref(false);
const submitting = ref(false);
const detail = ref<CustomerApi.CustomerDetail | null>(null);

const pageTitle = computed(() =>
  recordId.value ? '编辑联络记录' : $t('customer.addRecordTitle'),
);

const customerDisplayName = computed(() => detail.value?.fullName ?? '');

/** 按联络时间倒序，便于与项目跟进记录一致「最新在上」 */
const sortedContactRecords = computed(() => {
  const list = [...(detail.value?.contactRecords ?? [])];
  return list.sort((a, b) => {
    const da = a.recordAt ?? '';
    const db = b.recordAt ?? '';
    return db.localeCompare(da);
  });
});

/** 时间线左侧：仅年月日，不含时分秒 */
function formatRecordTimelineDateOnly(v?: string): string {
  if (!v) return '-';
  const d = new Date(v);
  if (Number.isNaN(d.getTime())) {
    const s = String(v).trim();
    return s.slice(0, 10) || s || '-';
  }
  const y = d.getFullYear();
  const m = d.getMonth() + 1;
  const day = d.getDate();
  return `${y}年${m}月${day}日`;
}

function recordEnteredByName(rec: CustomerApi.CustomerContactRecordItem): string {
  const ext = rec as CustomerApi.CustomerContactRecordItem & { CreatorName?: string };
  return (
    rec.creatorName?.trim() ||
    ext.CreatorName?.trim() ||
    rec.recorderName?.trim() ||
    '-'
  );
}

function recordRelatedContactNames(rec: CustomerApi.CustomerContactRecordItem): string {
  const contacts = detail.value?.contacts ?? [];
  const ids = (rec.customerContactIds ?? []).map(String);
  if (!ids.length) return '-';
  const names = ids
    .map((id) => contacts.find((c) => String(c.id) === id)?.name)
    .filter((n): n is string => Boolean(n && String(n).trim()));
  return names.length ? names.join('、') : '-';
}

function timelineColor(reminderIntervalDays?: number) {
  return (reminderIntervalDays ?? 0) > 0 ? 'orange' : 'blue';
}

function goEditContactRecord(id: string) {
  router.push({
    name: 'CustomerContactRecordEdit',
    params: { customerId: customerId.value, recordId: id },
  });
}

/** 客户档案所在区域：省->市->区 */
function formatCustomerRegionLine(c: CustomerApi.CustomerDetail | null): string {
  if (!c) return '';
  const parts = [c.provinceName, c.cityName, c.districtName].filter(
    (x) => x != null && String(x).trim() !== '',
  ) as string[];
  return parts.join('->');
}

const hasCustomerRegion = computed(() => {
  const d = detail.value;
  if (!d) return false;
  return Boolean(d.provinceName || d.cityName || d.districtName);
});

/** 联系类型：2 = 出差（拜访） */
const RECORD_TYPE_VISIT = 2;

const filePathRef = ref('');
const attachmentFileList = ref<UploadFile[]>([]);
const reminderCountValue = ref(1);
const showReminderCount = ref(false);

const contactSelectOptions = computed(() =>
  (detail.value?.contacts ?? []).map((c) => ({
    label: `${c.name ?? ''}${c.position ? `（${c.position}）` : ''}`,
    value: String(c.id),
  })),
);

const recordTypeOptions = () => [
  { label: $t('customer.recordTypePhone'), value: 1 },
  { label: $t('customer.recordTypeVisit'), value: 2 },
  { label: $t('customer.recordTypeWechat'), value: 3 },
  { label: $t('customer.recordTypeOther'), value: 4 },
];

const recordFormSchema = computed(() => [
  {
    component: 'Input',
    componentProps: { class: 'w-full', placeholder: '标题（可选）' },
    fieldName: 'title',
    label: '标题',
  },
  {
    component: 'DatePicker',
    componentProps: {
      class: 'w-full',
      showTime: true,
      valueFormat: 'YYYY-MM-DDTHH:mm:ss',
      placeholder: $t('customer.recordAt'),
    },
    fieldName: 'recordAt',
    label: $t('customer.recordAt'),
    rules: z.any().refine(
      (val) => val != null && String(val).trim() !== '',
      $t('ui.formRules.required', [$t('customer.recordAt')]),
    ),
  },
  {
    component: 'Select',
    componentProps: {
      class: 'w-full',
      options: recordTypeOptions(),
      placeholder: $t('customer.recordType'),
    },
    fieldName: 'recordType',
    label: $t('customer.recordType'),
    rules: z.coerce
      .number()
      .refine((n) => n >= 1 && n <= 4, $t('ui.formRules.required', [$t('customer.recordType')])),
  },
  {
    component: 'DatePicker',
    componentProps: {
      class: 'w-full',
      showTime: true,
      valueFormat: 'YYYY-MM-DDTHH:mm:ss',
      placeholder: '下次拜访日期（可选）',
    },
    fieldName: 'nextVisitAt',
    label: '下次拜访日期',
  },
  {
    component: 'Select',
    componentProps: {
      class: 'w-full',
      options: [
        { label: '待选择', value: 0 },
        { label: '有效联系', value: 1 },
        { label: '无效联系', value: 2 },
      ],
      placeholder: $t('customer.contactRecordStatus'),
    },
    fieldName: 'status',
    label: $t('customer.contactRecordStatus'),
  },
  {
    component: 'Select',
    componentProps: {
      class: 'w-full',
      mode: 'multiple',
      optionFilterProp: 'label',
      options: contactSelectOptions.value,
      placeholder: '关联联系人（可多选）',
      allowClear: true,
    },
    fieldName: 'customerContactIds',
    label: '关联联系人',
  },
  {
    component: 'Textarea',
    componentProps: {
      class: 'w-full',
      placeholder: $t('customer.recordContentPlaceholder'),
      rows: 4,
      autoSize: { minRows: 4, maxRows: 10 },
    },
    fieldName: 'content',
    label: $t('customer.recordContent'),
  },
  {
    component: 'Textarea',
    componentProps: { class: 'w-full', placeholder: '备注', rows: 2 },
    fieldName: 'remark',
    label: '备注',
  },
  {
    component: 'Input',
    componentProps: {
      class: 'w-full',
      readonly: hasCustomerRegion.value,
      placeholder: hasCustomerRegion.value ? '' : '请输入客户地址',
    },
    fieldName: 'customerAddress',
    label: $t('customer.contactRecordCustomerAddress'),
  },
  {
    component: 'Input',
    componentProps: { class: 'w-full', placeholder: $t('customer.contactRecordVisitAddress') },
    dependencies: {
      show: (values) => Number(values.recordType) === RECORD_TYPE_VISIT,
      triggerFields: ['recordType'],
    },
    fieldName: 'visitAddress',
    label: $t('customer.contactRecordVisitAddress'),
  },
  {
    component: 'Select',
    componentProps: {
      class: 'w-full',
      options: [1, 2, 3, 10, 15, 20, 30, 50, 80, 100].map((d) => ({ label: `${d} 天`, value: d })),
    },
    fieldName: 'reminderIntervalDays',
    label: $t('customer.reminderIntervalDays'),
  },
]);

const [Form, formApi] = useVbenForm({
  layout: 'vertical',
  schema: recordFormSchema as any,
  showDefaultActions: false,
  /** 左侧半宽单列，避免字段过挤 */
  wrapperClass: 'grid-cols-1 gap-y-4',
});

function parseQueryContactIds(): string[] {
  const q = route.query.contactIds;
  if (typeof q === 'string' && q) return q.split(',').filter(Boolean);
  if (Array.isArray(q)) return q.map(String).filter(Boolean);
  return [];
}

function buildDefaultRecordAt() {
  const now = new Date();
  return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}T${String(now.getHours()).padStart(2, '0')}:${String(now.getMinutes()).padStart(2, '0')}:00`;
}

function setAttachmentFromPath(path: string) {
  filePathRef.value = path;
  attachmentFileList.value = path
    ? [
        {
          uid: 'contact-record-attachment',
          name: path.split('/').pop() || 'file',
          status: 'done',
        },
      ]
    : [];
}

function resolveCustomerAddressValue(record?: Partial<CustomerApi.CustomerContactRecordItem>) {
  const r = record ?? {};
  if (hasCustomerRegion.value) {
    return formatCustomerRegionLine(detail.value);
  }
  return r.customerAddress != null && String(r.customerAddress).trim() !== ''
    ? String(r.customerAddress)
    : '';
}

function applyFormValues(record?: Partial<CustomerApi.CustomerContactRecordItem>) {
  const defaultRecordAt = buildDefaultRecordAt();
  const r = record ?? {};
  const rt = coerceRecordTypeToId(r.recordType ?? (r as { recordTypeId?: number }).recordTypeId);
  const st = r.status ?? r.statusId ?? 0;
  const contactIds = recordId.value
    ? (r.customerContactIds ?? []).map(String)
    : parseQueryContactIds();

  reminderCountValue.value = r.reminderCount ?? 1;
  showReminderCount.value = false;
  setAttachmentFromPath(r.filePath ?? '');

  formApi.setValues({
    title: r.title ?? '',
    recordAt: r.recordAt ?? defaultRecordAt,
    recordType: rt,
    nextVisitAt: r.nextVisitAt ?? undefined,
    status: Number(st),
    customerContactIds: contactIds,
    content: r.content ?? '',
    remark: r.remark ?? '',
    customerAddress: resolveCustomerAddressValue(record),
    visitAddress: r.visitAddress ?? '',
    reminderIntervalDays: r.reminderIntervalDays ?? 1,
  });
}

function resetToInitial() {
  if (recordId.value) {
    const rec = detail.value?.contactRecords?.find((x) => String(x.id) === recordId.value);
    if (rec) applyFormValues(rec);
  } else {
    applyFormValues();
    const ids = parseQueryContactIds();
    if (ids.length) {
      formApi.setFieldValue('customerContactIds', ids);
    }
  }
}

async function loadCustomer() {
  if (!customerId.value) return;
  loading.value = true;
  try {
    detail.value = await getCustomer(customerId.value);
  } finally {
    loading.value = false;
  }
}

async function handleAttachmentUpload(options: {
  file: File | Blob;
  onSuccess?: (body: unknown) => void;
  onError?: (e: Error) => void;
}) {
  const file = options.file as File;
  try {
    const res = await uploadFile(file);
    filePathRef.value = res.path;
    options.onSuccess?.(res);
  } catch (e) {
    options.onError?.(e instanceof Error ? e : new Error(String(e)));
  }
}

function onAttachmentRemove() {
  filePathRef.value = '';
  attachmentFileList.value = [];
}

async function submit() {
  const { valid } = await formApi.validate();
  if (!valid) return;
  submitting.value = true;
  const data = await formApi.getValues();
  const recordAtVal = data.recordAt;
  const recordAtStr =
    typeof recordAtVal === 'string'
      ? recordAtVal
      : (recordAtVal as { format?: (f: string) => string })?.format?.('YYYY-MM-DDTHH:mm:ss') ??
        new Date().toISOString();
  const rt = Number(data.recordType);
  const rc =
    reminderCountValue.value < 1 ? 1 : Math.floor(Number(reminderCountValue.value) || 1);
  const payload = {
    recordAt: recordAtStr,
    recordType: rt,
    title: String(data.title ?? ''),
    content: String(data.content ?? ''),
    nextVisitAt: data.nextVisitAt ? String(data.nextVisitAt) : null,
    status: Number(data.status ?? 0),
    customerContactIds: Array.isArray(data.customerContactIds)
      ? data.customerContactIds.map((v: unknown) => String(v))
      : [],
    remark: String(data.remark ?? ''),
    reminderIntervalDays: Number(data.reminderIntervalDays ?? 1),
    reminderCount: rc,
    filePath: String(filePathRef.value ?? ''),
    customerAddress: String(data.customerAddress ?? ''),
    visitAddress: rt === RECORD_TYPE_VISIT ? String(data.visitAddress ?? '') : '',
  };
  try {
    if (recordId.value) {
      await updateCustomerContactRecord(customerId.value, recordId.value, payload);
    } else {
      await addCustomerContactRecord(customerId.value, payload);
    }
    message.success($t('common.saveSuccess'));
    router.back();
  } finally {
    submitting.value = false;
  }
}

onMounted(async () => {
  await loadCustomer();
  if (!detail.value) {
    message.error('未找到客户');
    router.back();
    return;
  }
  if (recordId.value) {
    const rec = detail.value.contactRecords?.find((x) => String(x.id) === recordId.value);
    if (!rec) {
      message.error('未找到联络记录');
      router.back();
      return;
    }
    applyFormValues(rec);
  } else {
    applyFormValues();
  }
});
</script>

<template>
  <Page auto-content-height content-class="flex flex-col">
    <div class="w-full min-w-0 flex-1 flex flex-col">
      <div class="mb-4 flex min-w-0 flex-col gap-0.5">
        <Space>
          <Button @click="router.back()">返回</Button>
        </Space>
        <span v-if="customerDisplayName" class="pl-0 text-sm text-muted-foreground">
          {{ customerDisplayName }}
        </span>
      </div>

      <div
        class="grid min-h-0 flex-1 grid-cols-1 gap-6 lg:grid-cols-2 lg:items-stretch"
      >
        <!-- 左侧：表单 -->
        <Card :title="pageTitle" :loading="loading" class="min-w-0 flex flex-col">
          <p
            v-if="hasCustomerRegion"
            class="mb-3 text-xs leading-relaxed text-muted-foreground"
          >
            {{ $t('customer.contactRecordCustomerAddressReadonlyHint') }}
          </p>
          <Form class="min-h-0" />
          <div class="mt-4 border-t border-border pt-4">
            <div class="mb-2 text-sm font-medium text-foreground">
              {{ $t('customer.contactRecordAttachment') }}
            </div>
            <Upload
              v-model:file-list="attachmentFileList"
              :max-count="1"
              :custom-request="handleAttachmentUpload"
              @remove="onAttachmentRemove"
            >
              <Button type="primary">
                {{ $t('customer.contactRecordAttachmentUploadTip') }}
              </Button>
            </Upload>
          </div>
          <div class="mt-4 flex flex-wrap items-center gap-3 border-t border-border pt-4">
            <Button type="link" class="!h-auto !px-0" @click="showReminderCount = !showReminderCount">
              {{
                showReminderCount
                  ? $t('customer.hideReminderCount')
                  : $t('customer.showReminderCount')
              }}
            </Button>
            <div v-show="showReminderCount" class="flex items-center gap-2">
              <span class="text-sm text-muted-foreground">{{ $t('customer.reminderCount') }}</span>
              <InputNumber v-model:value="reminderCountValue" :min="1" class="!w-28" />
            </div>
          </div>
        </Card>

        <!-- 右侧：客户联络记录（参考项目跟进记录时间线） -->
        <Card
          :title="$t('customer.contactRecords')"
          :loading="loading"
          class="min-w-0 flex flex-col border-border bg-card"
          :bordered="true"
          :body-style="{ overflow: 'visible' }"
        >
          <div
            v-if="sortedContactRecords.length === 0"
            class="flex flex-1 flex-col items-center justify-center py-12 text-muted-foreground"
          >
            {{ $t('customer.noContactRecordsHint') }}
          </div>
          <div
            v-else
            class="contact-records-scroll min-h-0 max-h-[min(70vh,calc(100vh-220px))] flex-1 overflow-x-hidden overflow-y-auto overscroll-y-contain px-1 pb-3 pt-3 [-ms-overflow-style:none] [scrollbar-width:thin] [&::-webkit-scrollbar]:w-1.5"
          >
            <Timeline class="contact-record-timeline">
              <TimelineItem
                v-for="rec in sortedContactRecords"
                :key="rec.id"
                :color="timelineColor(rec.reminderIntervalDays)"
              >
                <div class="flex w-full max-w-full items-stretch gap-0">
                  <!-- 左：拜访时间（在内容卡片外，独立列） -->
                  <div
                    class="flex w-[5.5rem] shrink-0 items-start justify-end border-r border-border/70 pr-2.5 pt-0.5 text-right sm:w-28 sm:pr-3"
                  >
                    <span
                      class="text-[13px] font-semibold leading-snug tracking-tight text-foreground sm:text-[14px]"
                    >
                      {{ formatRecordTimelineDateOnly(rec.recordAt) }}
                    </span>
                  </div>
                  <!-- 中：左中右元信息 + 内容框 -->
                  <div class="min-w-0 flex-1 px-2 sm:px-3">
                    <div
                      class="rounded-lg border px-3 py-2 shadow-sm transition-colors"
                      :class="
                        recordId && String(rec.id) === recordId
                          ? 'border-primary/80 bg-primary/5'
                          : 'border-border bg-card'
                      "
                    >
                      <div
                        class="grid grid-cols-1 gap-x-2 gap-y-1.5 text-[12px] leading-snug sm:grid-cols-3 sm:gap-y-0"
                      >
                        <div class="min-w-0 sm:text-left">
                          <span class="text-muted-foreground">{{
                            $t('customer.contactRecordContactWay')
                          }}</span>
                          <span class="ml-1 font-medium text-foreground">{{
                            formatCustomerContactRecordType(rec.recordType)
                          }}</span>
                        </div>
                        <div
                          class="min-w-0 sm:border-x sm:border-border/60 sm:px-2 sm:text-center"
                        >
                          <span class="text-muted-foreground">{{
                            $t('customer.contactRecordEnteredBy')
                          }}</span>
                          <span class="ml-1 font-medium text-foreground">{{
                            recordEnteredByName(rec)
                          }}</span>
                        </div>
                        <div class="min-w-0 sm:text-right">
                          <span class="text-muted-foreground">{{
                            $t('customer.contactRecordRelatedContacts')
                          }}</span>
                          <span class="ml-1 break-words font-medium text-foreground">{{
                            recordRelatedContactNames(rec)
                          }}</span>
                        </div>
                      </div>
                      <div
                        v-if="rec.content"
                        class="mt-2 max-h-36 overflow-y-auto rounded-md border border-border/70 bg-muted/25 px-2.5 py-2 text-sm leading-relaxed whitespace-pre-wrap text-card-foreground"
                      >
                        {{ rec.content }}
                      </div>
                    </div>
                  </div>
                  <!-- 右：操作 -->
                  <div
                    class="flex w-10 shrink-0 flex-col items-end justify-start pt-0.5 sm:w-12 md:w-14"
                  >
                    <Button
                      v-if="String(rec.id) !== recordId"
                      type="link"
                      size="small"
                      class="!h-auto !px-0 !py-0"
                      @click="goEditContactRecord(String(rec.id))"
                    >
                      {{ $t('common.edit') }}
                    </Button>
                  </div>
                </div>
              </TimelineItem>
            </Timeline>
          </div>
        </Card>
      </div>

      <div class="mt-6 flex flex-wrap justify-start gap-2">
        <Button danger @click="resetToInitial">
          {{ $t('common.reset') }}
        </Button>
        <Button type="primary" :loading="submitting" @click="submit">
          {{ $t('common.save') }}
        </Button>
      </div>
    </div>
  </Page>
</template>

<style scoped>
/* 抵消 Timeline 首条内容区默认上移，避免顶部边框/shadow 被滚动容器裁成「缺一块」 */
.contact-records-scroll :deep(.ant-timeline-item:first-child .ant-timeline-item-content) {
  margin-block-start: 0 !important;
  inset-block-start: 0 !important;
  top: 0 !important;
}
</style>
