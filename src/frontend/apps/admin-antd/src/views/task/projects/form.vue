<script lang="ts" setup>
import type { RegionCascaderOption } from './data';

import { computed, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';
import { ArrowLeft } from '@vben/icons';

import { Button, message, Modal, Space, Table, Tag } from 'ant-design-vue';
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

const contactList = computed(() => projectDetail.value?.contacts ?? []);
const followUpRecordList = computed(() => projectDetail.value?.followUpRecords ?? []);

const contactColumns: TableColumnType<ProjectApi.ProjectContactItem>[] = [
  { title: () => $t('task.project.contactName'), dataIndex: 'name', key: 'name', width: 100 },
  { title: () => $t('task.project.contactPosition'), dataIndex: 'position', key: 'position', width: 90 },
  { title: () => $t('task.project.contactMobile'), dataIndex: 'mobile', key: 'mobile', width: 120 },
  { title: () => $t('task.project.contactOfficePhone'), dataIndex: 'officePhone', key: 'officePhone', width: 110 },
  { title: () => $t('task.project.contactEmail'), dataIndex: 'email', key: 'email', width: 140 },
  { title: () => $t('task.project.isPrimary'), dataIndex: 'isPrimary', key: 'isPrimary', width: 90 },
  { title: () => $t('task.project.operation'), key: 'action', width: 160 },
];

const followUpRecordColumns: TableColumnType<ProjectApi.ProjectFollowUpRecordItem>[] = [
  { title: () => $t('task.project.followUpTitle'), dataIndex: 'title', key: 'title', width: 140 },
  { title: () => $t('task.project.followUpVisitDate'), dataIndex: 'visitDate', key: 'visitDate', width: 120 },
  { title: () => $t('task.project.followUpReminderFrequency'), dataIndex: 'reminderIntervalDays', key: 'reminderIntervalDays', width: 100 },
  { title: () => $t('task.project.followUpContent'), dataIndex: 'content', key: 'content', ellipsis: true },
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

const [Form, formApi] = useVbenForm({
  layout: 'vertical',
  schema: computed(() => useSchema(regionTreeOptions.value)) as any,
  showDefaultActions: false,
  wrapperClass: 'grid-cols-1 md:grid-cols-4 gap-x-6 gap-y-4',
});

function goBack() {
  router.push('/task/projects');
}

function resetForm() {
  formApi.resetForm();
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
    description: detail.description ?? '',
    projectTypeId: detail.projectTypeId ?? undefined,
    projectStatusOptionId: detail.projectStatusOptionId ?? undefined,
    projectNumber: detail.projectNumber ?? '',
    projectIndustryId: detail.projectIndustryId ?? '',
    customerId: detail.customerId ?? '',
    regionIds: regionIds.length > 0 ? regionIds : undefined,
    startDate: detail.startDate ?? undefined,
    projectEstimate: detail.projectEstimate ?? '',
    purchaseAmount: detail.purchaseAmount ?? undefined,
    projectContent: detail.projectContent ?? '',
  });
  if (detail.customerId) {
    try {
      const customerDetail = await getCustomer(detail.customerId);
      customerContacts.value = customerDetail.contacts ?? [];
    } catch {
      customerContacts.value = [];
    }
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
        description: data.description != null ? String(data.description) : '',
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
        projectEstimate: data.projectEstimate != null ? String(data.projectEstimate) : '',
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
        description: data.description != null ? String(data.description) : '',
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
        projectEstimate: data.projectEstimate != null ? String(data.projectEstimate) : '',
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
  loadDetail();
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
      <Form />
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
                <Tag v-if="record.isPrimary" color="blue">{{ $t('task.project.isPrimary') }}</Tag>
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
        <!-- 项目跟进记录 -->
        <div class="mt-6 border-t border-gray-200 pt-6">
          <div class="mb-3 flex items-center justify-between">
            <span class="text-base font-medium">{{ $t('task.project.followUpRecords') }}</span>
            <Button type="primary" class="inline-flex items-center gap-1" @click="openFollowUpRecordDrawer()">
              + {{ $t('task.project.addFollowUpRecord') }}
            </Button>
          </div>
          <Table
            :columns="followUpRecordColumns"
            :data-source="followUpRecordList"
            :pagination="false"
            row-key="id"
            size="small"
            class="mt-2"
          >
            <template #bodyCell="{ column, record }">
              <template v-if="column.key === 'reminderIntervalDays'">
                {{ record.reminderIntervalDays === 0 ? '-' : $t('task.project.followUpReminderDays', [String(record.reminderIntervalDays)]) }}
              </template>
              <template v-else-if="column.key === 'action'">
                <Space>
                  <Button type="link" size="small" @click="openFollowUpRecordDrawer(record)">
                    {{ $t('task.project.editFollowUpRecord') }}
                  </Button>
                  <Button type="link" size="small" danger @click="handleDeleteFollowUpRecord(record)">
                    {{ $t('task.project.deleteFollowUpRecord') }}
                  </Button>
                </Space>
              </template>
            </template>
          </Table>
        </div>
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
