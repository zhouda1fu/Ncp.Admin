<script lang="ts" setup>
import type { ProjectApi } from '#/api/system/project';
import type { RegionCascaderOption } from '../data';

import { computed, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { Button } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import { createProject, updateProject } from '#/api/system/project';
import { getRegionList } from '#/api/system/region';
import type { RegionApi } from '#/api/system/region';
import { $t } from '#/locales';

import { useSchema } from '../data';

const emit = defineEmits(['success']);
const formData = ref<Partial<ProjectApi.ProjectItem> & { id?: string }>();
const regionList = ref<RegionApi.RegionItem[]>([]);

const getTitle = computed(() =>
  formData.value?.id
    ? $t('ui.actionTitle.edit', [$t('task.project.name')])
    : $t('ui.actionTitle.create', [$t('task.project.name')]),
);

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

const [Form, formApi] = useVbenForm({
  layout: 'vertical',
  schema: computed(() => useSchema(regionTreeOptions.value)),
  showDefaultActions: false,
});

function resetForm() {
  formApi.resetForm();
  formApi.setValues(formData.value || {});
}

const [Drawer, drawerApi] = useVbenDrawer({
  async onConfirm() {
    const { valid } = await formApi.validate();
    if (valid) {
      drawerApi.lock();
      const data = await formApi.getValues();
      try {
        const regionIds = (data.regionIds as string[] | undefined) ?? [];
        const provinceRegionId =
          regionIds.length > 0 && regionIds[0] ? Number(regionIds[0]) : undefined;
        const cityRegionId =
          regionIds.length > 1 && regionIds[1] ? Number(regionIds[1]) : undefined;
        const districtRegionId =
          regionIds.length > 2 && regionIds[2] ? Number(regionIds[2]) : undefined;

        if (formData.value?.id) {
          await updateProject(formData.value.id, {
            name: String(data.name),
            projectTypeId: data.projectTypeId ? String(data.projectTypeId) : undefined,
            projectStatusOptionId: data.projectStatusOptionId
              ? String(data.projectStatusOptionId)
              : undefined,
            projectNumber: data.projectNumber ? String(data.projectNumber) : undefined,
            projectIndustryId: String(data.projectIndustryId),
            provinceRegionId,
            cityRegionId,
            districtRegionId,
            startDate: data.startDate ? String(data.startDate) : undefined,
            budget:
              data.budget != null && data.budget !== '' ? Number(data.budget) : undefined,
            purchaseAmount:
              data.purchaseAmount != null && data.purchaseAmount !== ''
                ? Number(data.purchaseAmount)
                : undefined,
            projectContent: data.projectContent ? String(data.projectContent) : undefined,
          });
        } else {
          await createProject({
            name: String(data.name),
            customerId: String(data.customerId),
            projectIndustryId: String(data.projectIndustryId),
            projectTypeId: data.projectTypeId ? String(data.projectTypeId) : undefined,
            projectStatusOptionId: data.projectStatusOptionId
              ? String(data.projectStatusOptionId)
              : undefined,
            projectNumber: data.projectNumber ? String(data.projectNumber) : undefined,
            provinceRegionId,
            cityRegionId,
            districtRegionId,
            startDate: data.startDate ? String(data.startDate) : undefined,
            budget:
              data.budget != null && data.budget !== '' ? Number(data.budget) : undefined,
            purchaseAmount:
              data.purchaseAmount != null && data.purchaseAmount !== ''
                ? Number(data.purchaseAmount)
                : undefined,
            projectContent: data.projectContent ? String(data.projectContent) : undefined,
          });
        }
        drawerApi.close();
        emit('success');
      } finally {
        drawerApi.lock(false);
      }
    }
  },
  onOpenChange(isOpen) {
    if (isOpen) {
      getRegionList().then((list) => {
        regionList.value = list;
      });
      const data = drawerApi.getData<Partial<ProjectApi.ProjectItem> & { id?: string }>();
      formData.value = data;
      const regionIds: string[] = [];
      if (data?.provinceRegionId != null) regionIds.push(String(data.provinceRegionId));
      if (data?.cityRegionId != null) regionIds.push(String(data.cityRegionId));
      if (data?.districtRegionId != null) regionIds.push(String(data.districtRegionId));
      formApi.setValues({
        name: data?.name ?? '',
        projectTypeId: data?.projectTypeId ?? undefined,
        projectStatusOptionId: data?.projectStatusOptionId ?? undefined,
        projectNumber: data?.projectNumber ?? '',
        projectIndustryId: data?.projectIndustryId ?? '',
        customerId: data?.customerId ?? '',
        regionIds: regionIds.length > 0 ? regionIds : undefined,
        startDate: data?.startDate ?? undefined,
        budget: data?.budget ?? undefined,
        purchaseAmount: data?.purchaseAmount ?? undefined,
        projectContent: data?.projectContent ?? '',
      });
    }
  },
});
</script>

<template>
  <Drawer :title="getTitle">
    <Form class="mx-4" />
    <template #prepend-footer>
      <div class="flex-auto">
        <Button type="primary" danger @click="resetForm">
          {{ $t('common.reset') }}
        </Button>
      </div>
    </template>
  </Drawer>
</template>
