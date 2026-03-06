<script lang="ts" setup>
import type { RegionCascaderOption } from './data';

import { computed, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';
import { ArrowLeft, Search } from '@vben/icons';

import {
  Button,
  Card,
  Input,
  message,
  Modal,
  Space,
  Table,
  Tag,
  Timeline,
  TimelineItem,
} from 'ant-design-vue';
import type { TableColumnType } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import {
  createProject,
  getProject,
  removeProjectContact,
  removeProjectFollowUpRecord,
  updateProject,
} from '#/api/system/project';
import type { ProjectApi } from '#/api/system/project';
import { getCustomer, getCustomerSearch } from '#/api/system/customer';
import type { CustomerApi } from '#/api/system/customer';
import { getRegionList } from '#/api/system/region';
import type { RegionApi } from '#/api/system/region';
import { getProjectIndustryList } from '#/api/system/project-industry';
import { getProjectStatusList } from '#/api/system/project-status';
import { getProjectTypeList } from '#/api/system/project-type';
import { $t } from '#/locales';

import ContactDrawer from './modules/contact-drawer.vue';
import FollowUpRecordDrawer from './modules/follow-up-record-drawer.vue';
import CustomerSelectModal from './modules/customer-select-modal.vue';
import { useSchema } from './data';

const route = useRoute();
const router = useRouter();

const id = computed(() => route.params.id as string | undefined);
const isNew = computed(() => !id.value);

const regionList = ref<RegionApi.RegionItem[]>([]);
const projectTypeOptions = ref<{ label: string; value: string }[]>([]);
const projectStatusOptions = ref<{ label: string; value: string }[]>([]);
const projectIndustryOptions = ref<{ label: string; value: string }[]>([]);
const customerOptions = ref<{ label: string; value: string }[]>([]);
const submitting = ref(false);
const projectDetail = ref<ProjectApi.ProjectItem | null>(null);
const customerContacts = ref<CustomerApi.CustomerContactItem[]>([]);
const contactDrawerRef = ref<InstanceType<typeof ContactDrawer> | null>(null);
const followUpRecordDrawerRef = ref<InstanceType<typeof FollowUpRecordDrawer> | null>(null);
const customerSelectModalRef = ref<InstanceType<typeof CustomerSelectModal> | null>(null);
const selectedCustomerName = ref('');

const contactList = computed(() => projectDetail.value?.contacts ?? []);
const followUpRecordList = computed(() => projectDetail.value?.followUpRecords ?? []);

/** 按 visitDate 或 createdAt 排序，最新在上 */
const sortedFollowUpRecords = computed(() => {
  const list = [...(followUpRecordList.value ?? [])];
  return list.sort((a, b) => {
    const da = a.visitDate || a.createdAt || '';
    const db = b.visitDate || b.createdAt || '';
    return db.localeCompare(da);
  });
});

const contactColumns: TableColumnType<ProjectApi.ProjectContactItem>[] = [
  { title: () => $t('task.project.contactName'), dataIndex: 'name', key: 'name', width: 120 },
  { title: () => $t('task.project.contactPosition'), dataIndex: 'position', key: 'position', width: 100 },
  { title: () => $t('task.project.contactMobile'), dataIndex: 'mobile', key: 'mobile', width: 120 },
  { title: () => $t('task.project.contactOfficePhone'), dataIndex: 'officePhone', key: 'officePhone', width: 120 },
  { title: () => $t('task.project.contactEmail'), dataIndex: 'email', key: 'email', width: 140 },
  { title: () => $t('task.project.isPrimary'), dataIndex: 'isPrimary', key: 'isPrimary', width: 90 },
  { title: () => $t('task.project.operation'), key: 'action', width: 160 },
];

const provinceFilter = (r: RegionApi.RegionItem) =>
  r.level === 2 || (Number(r.parentId) === 9 && r.level === 1);

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

function getRegionNameById(regionId: number | string): string {
  const r = regionList.value.find((x) => String(x.id) === String(regionId));
  return r?.name ?? '';
}

function formatFollowUpDate(dt?: string): string {
  if (!dt) return '-';
  return new Date(dt).toLocaleDateString('zh-CN');
}

function formatReminderText(reminderIntervalDays: number): string {
  return reminderIntervalDays === 0
    ? '-'
    : $t('task.project.followUpReminderDays', [String(reminderIntervalDays)]);
}

/** 有提醒的跟进记录用橙色节点，无提醒用蓝色 */
function getFollowUpTimelineColor(reminderIntervalDays: number): string {
  return reminderIntervalDays > 0 ? 'orange' : 'blue';
}

const [Form, formApi] = useVbenForm({
  layout: 'vertical',
  schema: computed(() => useSchema(regionTreeOptions.value)) as any,
  showDefaultActions: false,
  wrapperClass: 'grid-cols-1 md:grid-cols-4 gap-x-6 gap-y-4',
});

function goBack() {
  router.push('/task/projects');
}

/** 生成项目编号，格式 yyyyMMddHHmmss */
function generateProjectNumber(): string {
  const now = new Date();
  const y = now.getFullYear();
  const m = String(now.getMonth() + 1).padStart(2, '0');
  const d = String(now.getDate()).padStart(2, '0');
  const h = String(now.getHours()).padStart(2, '0');
  const min = String(now.getMinutes()).padStart(2, '0');
  const s = String(now.getSeconds()).padStart(2, '0');
  return `${y}${m}${d}${h}${min}${s}`;
}

function resetForm() {
  formApi.resetForm();
  selectedCustomerName.value = '';
  if (isNew.value) {
    formApi.setFieldValue('projectNumber', generateProjectNumber());
  }
}

function openCustomerSelectModal() {
  customerSelectModalRef.value?.open({
    onSelect(row: CustomerApi.CustomerItem) {
      formApi.setFieldValue('customerId', row.id);
      const label = row.shortName ? `${row.fullName}（${row.shortName}）` : row.fullName;
      selectedCustomerName.value = label;
      if (!customerOptions.value.some((o) => o.value === row.id)) {
        customerOptions.value = [...customerOptions.value, { label, value: row.id }];
      }
    },
  });
}

async function loadDetail() {
  if (!id.value) return;
  const detail = await getProject(id.value);
  projectDetail.value = detail;
  const regionIds: string[] = [];
  if (detail.provinceRegionId != null) regionIds.push(String(detail.provinceRegionId));
  if (detail.cityRegionId != null) regionIds.push(String(detail.cityRegionId));
  if (detail.districtRegionId != null) regionIds.push(String(detail.districtRegionId));
  formApi.setValues({
    name: detail.name ?? '',
    projectTypeId: detail.projectTypeId ?? undefined,
    projectStatusOptionId: detail.projectStatusOptionId ?? undefined,
    projectNumber: detail.projectNumber ?? '',
    projectIndustryId: detail.projectIndustryId ?? '',
    customerId: detail.customerId ?? '',
    regionIds: regionIds.length > 0 ? regionIds : undefined,
    startDate: detail.startDate ?? undefined,
    budget: detail.budget ?? undefined,
    purchaseAmount: detail.purchaseAmount ?? undefined,
    projectContent: detail.projectContent ?? '',
  });
  if (detail.customerId) {
    try {
      const customerDetail = await getCustomer(detail.customerId);
      customerContacts.value = customerDetail.contacts ?? [];
      selectedCustomerName.value = customerDetail.shortName
        ? `${customerDetail.fullName}（${customerDetail.shortName}）`
        : customerDetail.fullName;
    } catch {
      customerContacts.value = [];
      selectedCustomerName.value =
        customerOptions.value.find((o) => o.value === detail.customerId)?.label ?? '';
    }
  } else {
    selectedCustomerName.value = '';
  }
}

function openContactDrawer(contact?: ProjectApi.ProjectContactItem | Record<string, unknown>) {
  contactDrawerRef.value?.open(contact as ProjectApi.ProjectContactItem | undefined);
}

async function onContactSuccess() {
  if (id.value) {
    const detail = await getProject(id.value);
    projectDetail.value = detail;
  }
}

async function handleDeleteContact(contact: ProjectApi.ProjectContactItem | Record<string, unknown>) {
  if (!id.value) return;
  const c = contact as ProjectApi.ProjectContactItem;
  Modal.confirm({
    title: $t('task.project.confirmDeleteContact'),
    onOk: async () => {
      await removeProjectContact(id.value!, c.id);
      message.success($t('ui.actionMessage.operationSuccess'));
      const detail = await getProject(id.value!);
      projectDetail.value = detail;
    },
  });
}

function openFollowUpRecordDrawer(record?: ProjectApi.ProjectFollowUpRecordItem | Record<string, unknown>) {
  followUpRecordDrawerRef.value?.open(record as ProjectApi.ProjectFollowUpRecordItem | undefined);
}

async function onFollowUpRecordSuccess() {
  if (id.value) {
    const detail = await getProject(id.value);
    projectDetail.value = detail;
  }
}

async function handleDeleteFollowUpRecord(record: ProjectApi.ProjectFollowUpRecordItem | Record<string, unknown>) {
  if (!id.value) return;
  const r = record as ProjectApi.ProjectFollowUpRecordItem;
  Modal.confirm({
    title: $t('task.project.confirmDeleteFollowUpRecord'),
    onOk: async () => {
      await removeProjectFollowUpRecord(id.value!, r.id);
      message.success($t('ui.actionMessage.operationSuccess'));
      const detail = await getProject(id.value!);
      projectDetail.value = detail;
    },
  });
}

function resolveNames(data: Record<string, unknown>) {
  const projectTypeId = String(data.projectTypeId ?? '');
  const projectStatusOptionId = String(data.projectStatusOptionId ?? '');
  const projectIndustryId = String(data.projectIndustryId ?? '');
  const customerId = String(data.customerId ?? '');
  const regionIds = (data.regionIds as string[] | undefined) ?? [];
  const provinceRegionId = regionIds.length > 0 && regionIds[0] ? Number(regionIds[0]) : 0;
  const cityRegionId = regionIds.length > 1 && regionIds[1] ? Number(regionIds[1]) : 0;
  const districtRegionId = regionIds.length > 2 && regionIds[2] ? Number(regionIds[2]) : 0;
  return {
    projectTypeName: projectTypeOptions.value.find((o) => o.value === projectTypeId)?.label ?? '',
    projectStatusOptionName:
      projectStatusOptions.value.find((o) => o.value === projectStatusOptionId)?.label ?? '',
    projectIndustryName:
      projectIndustryOptions.value.find((o) => o.value === projectIndustryId)?.label ?? '',
    customerName: customerOptions.value.find((o) => o.value === customerId)?.label ?? '',
    provinceRegionId,
    provinceName: getRegionNameById(provinceRegionId),
    cityRegionId,
    cityName: getRegionNameById(cityRegionId),
    districtRegionId,
    districtName: getRegionNameById(districtRegionId),
  };
}

async function onSubmit() {
  const { valid } = await formApi.validate();
  if (!valid) return;
  submitting.value = true;
  try {
    const data = await formApi.getValues();
    const names = resolveNames(data);
    const regionIds = (data.regionIds as string[] | undefined) ?? [];
    const provinceRegionId =
      regionIds.length > 0 && regionIds[0] ? Number(regionIds[0]) : 0;
    const cityRegionId = regionIds.length > 1 && regionIds[1] ? Number(regionIds[1]) : 0;
    const districtRegionId =
      regionIds.length > 2 && regionIds[2] ? Number(regionIds[2]) : 0;

    if (id.value) {
      await updateProject(id.value, {
        name: String(data.name),
        projectTypeId: String(data.projectTypeId ?? ''),
        projectTypeName: names.projectTypeName,
        projectStatusOptionId: String(data.projectStatusOptionId ?? ''),
        projectStatusOptionName: names.projectStatusOptionName,
        projectNumber: data.projectNumber != null ? String(data.projectNumber) : '',
        projectIndustryId: String(data.projectIndustryId),
        projectIndustryName: names.projectIndustryName,
        provinceRegionId,
        provinceName: names.provinceName,
        cityRegionId,
        cityName: names.cityName,
        districtRegionId,
        districtName: names.districtName,
        startDate: data.startDate ? String(data.startDate) : undefined,
        budget:
          data.budget != null && data.budget !== '' ? Number(data.budget) : 0,
        purchaseAmount:
          data.purchaseAmount != null && data.purchaseAmount !== ''
            ? Number(data.purchaseAmount)
            : 0,
        projectContent: data.projectContent != null ? String(data.projectContent) : '',
      });
      message.success($t('ui.actionMessage.operationSuccess'));
    } else {
      await createProject({
        name: String(data.name),
        customerId: String(data.customerId),
        customerName: names.customerName,
        projectTypeId: String(data.projectTypeId ?? ''),
        projectTypeName: names.projectTypeName,
        projectStatusOptionId: String(data.projectStatusOptionId ?? ''),
        projectStatusOptionName: names.projectStatusOptionName,
        projectIndustryId: String(data.projectIndustryId),
        projectIndustryName: names.projectIndustryName,
        provinceRegionId,
        provinceName: names.provinceName,
        cityRegionId,
        cityName: names.cityName,
        districtRegionId,
        districtName: names.districtName,
        projectNumber: data.projectNumber != null ? String(data.projectNumber) : '',
        startDate: data.startDate ? String(data.startDate) : undefined,
        budget:
          data.budget != null && data.budget !== '' ? Number(data.budget) : 0,
        purchaseAmount:
          data.purchaseAmount != null && data.purchaseAmount !== ''
            ? Number(data.purchaseAmount)
            : 0,
        projectContent: data.projectContent != null ? String(data.projectContent) : '',
      });
      message.success($t('ui.actionMessage.operationSuccess'));
    }
    goBack();
  } catch (err) {
    message.error((err as Error)?.message ?? $t('ui.actionMessage.operationFailed'));
  } finally {
    submitting.value = false;
  }
}

onMounted(() => {
  getRegionList().then((list) => {
    regionList.value = list;
  });
  getProjectTypeList().then((list) => {
    projectTypeOptions.value = list.map((x) => ({ label: x.name, value: x.id }));
  });
  getProjectStatusList().then((list) => {
    projectStatusOptions.value = list.map((x) => ({ label: x.name, value: x.id }));
  });
  getProjectIndustryList().then((list) => {
    projectIndustryOptions.value = list.map((x) => ({ label: x.name, value: x.id }));
  });
  getCustomerSearch({ pageIndex: 1, pageSize: 500 }).then((res) => {
    const items = res?.items ?? [];
    customerOptions.value = items.map((x) => ({
      label: x.shortName ? `${x.fullName}（${x.shortName}）` : x.fullName,
      value: x.id,
    }));
  });
  loadDetail().then(() => {
    if (isNew.value) {
      formApi.setFieldValue('projectNumber', generateProjectNumber());
    }
  });
});
</script>

<template>
  <Page auto-content-height content-class="flex flex-col">
    <div class="mb-4 flex items-center gap-2">
      <Button type="text" @click="goBack">
        <ArrowLeft class="size-4" />
      </Button>
      <span class="text-lg font-medium">
        {{
          isNew
            ? $t('ui.actionTitle.create', [$t('task.project.name')])
            : $t('ui.actionTitle.edit', [$t('task.project.name')])
        }}
      </span>
    </div>

    <div class="w-full flex-1 min-w-0">
      <Form>
        <template #customerId>
          <Input
            :value="selectedCustomerName"
            readonly
            class="w-full cursor-pointer"
            :placeholder="$t('task.project.searchCustomer')"
            @click="openCustomerSelectModal"
          >
            <template #suffix>
              <span
                class="cursor-pointer text-gray-500 hover:text-primary transition-colors"
                role="button"
                tabindex="0"
                @click.stop="openCustomerSelectModal"
                @keydown.enter.prevent="openCustomerSelectModal"
              >
                <Search class="size-4" />
              </span>
            </template>
          </Input>
        </template>
      </Form>
      <CustomerSelectModal ref="customerSelectModalRef" />
      <!-- 项目联系人（仅编辑页展示） -->
      <template v-if="id">
        <div class="mt-8 border-t border-gray-200 pt-6">
          <div class="mb-3 flex items-center justify-between">
            <span class="text-base font-medium">{{ $t('task.project.contacts') }}</span>
            <Button type="primary" class="inline-flex items-center gap-1" @click="openContactDrawer()">
              + {{ $t('task.project.addContact') }}
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
                <Tag :color="record.isPrimary ? 'success' : 'default'">
                  {{ record.isPrimary ? $t('common.yes') : $t('common.no') }}
                </Tag>
              </template>
              <template v-else-if="column.key === 'action'">
                <Space>
                  <Button type="link" size="small" @click="openContactDrawer(record)">
                    {{ $t('task.project.editContact') }}
                  </Button>
                  <Button type="link" size="small" danger @click="handleDeleteContact(record)">
                    {{ $t('task.project.deleteContact') }}
                  </Button>
                </Space>
              </template>
            </template>
          </Table>
        </div>
        <ContactDrawer
          v-if="id"
          ref="contactDrawerRef"
          :project-id="id"
          :customer-contacts="customerContacts"
          @success="onContactSuccess"
        />
        <!-- 项目跟进记录（时间线展示，参考 workflow 审批记录 Card 布局） -->
        <Card
          :title="$t('task.project.followUpRecords')"
          class="mt-6 border-border bg-card"
          :bordered="true"
        >
          <template #extra>
            <Button type="primary" size="small" @click="openFollowUpRecordDrawer()">
              + {{ $t('task.project.addFollowUpRecord') }}
            </Button>
          </template>
          <div v-if="sortedFollowUpRecords.length === 0" class="py-12 text-center text-muted-foreground">
            {{ $t('task.project.noFollowUpRecords') }}
          </div>
          <Timeline v-else>
            <TimelineItem
              v-for="record in sortedFollowUpRecords"
              :key="record.id"
              :color="getFollowUpTimelineColor(record.reminderIntervalDays)"
            >
              <div class="flex items-start justify-between gap-4">
                <div class="min-w-0 flex-1">
                  <div class="font-medium text-foreground">
                    {{ record.title }}
                  </div>
                  <div class="mt-1 flex flex-wrap gap-x-4 gap-y-0 text-sm text-muted-foreground">
                    <span>{{ $t('task.project.followUpVisitDate') }}: {{ formatFollowUpDate(record.visitDate ?? record.createdAt) }}</span>
                    <span>{{ $t('task.project.followUpReminderFrequency') }}: {{ formatReminderText(record.reminderIntervalDays) }}</span>
                  </div>
                  <div v-if="record.content" class="mt-2 rounded-md border border-border bg-muted/30 px-3 py-2 whitespace-pre-wrap text-sm text-card-foreground">
                    {{ record.content }}
                  </div>
                </div>
                <Space class="shrink-0">
                  <Button type="link" size="small" class="!p-0" @click="openFollowUpRecordDrawer(record)">
                    {{ $t('task.project.editFollowUpRecord') }}
                  </Button>
                  <Button type="link" size="small" danger class="!p-0" @click="handleDeleteFollowUpRecord(record)">
                    {{ $t('task.project.deleteFollowUpRecord') }}
                  </Button>
                </Space>
              </div>
            </TimelineItem>
          </Timeline>
        </Card>
        <FollowUpRecordDrawer
          v-if="id"
          ref="followUpRecordDrawerRef"
          :project-id="id"
          @success="onFollowUpRecordSuccess"
        />
      </template>
      <div class="mt-6 flex gap-2">
        <Button
          type="primary"
          :loading="submitting"
          :disabled="submitting"
          @click="onSubmit"
        >
          {{ $t('common.confirm') }}
        </Button>
        <Button type="primary" danger @click="resetForm">
          {{ $t('common.reset') }}
        </Button>
        <Button @click="goBack">
          {{ $t('common.cancel') }}
        </Button>
      </div>
    </div>
  </Page>
</template>
