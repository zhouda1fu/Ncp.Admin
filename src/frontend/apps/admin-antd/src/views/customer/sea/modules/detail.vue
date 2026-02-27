<script lang="ts" setup>
import type { CustomerApi } from '#/api/system/customer';

import { computed } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { Descriptions, DescriptionsItem } from 'ant-design-vue';

import { $t } from '#/locales';

const [Drawer, drawerApi] = useVbenDrawer();

const detail = computed(() => drawerApi.getData<CustomerApi.CustomerDetail | undefined>() ?? undefined);

const phoneRegionText = computed(() => {
  const d = detail.value;
  if (!d) return '-';
  return [d.phoneProvinceName, d.phoneCityName, d.phoneDistrictName].filter(Boolean).join(' ') || '-';
});

const projectRegionText = computed(() => {
  const d = detail.value;
  if (!d) return '-';
  return [d.provinceName, d.cityName, d.districtName].filter(Boolean).join(' ') || '-';
});

defineExpose({
  open: () => drawerApi.open(),
  close: () => drawerApi.close(),
});
</script>

<template>
  <Drawer :title="$t('customer.view')">
    <Descriptions v-if="detail" :column="1" bordered size="small" class="mx-4">
      <DescriptionsItem :label="$t('customer.customerSource')">{{ detail.customerSourceName }}</DescriptionsItem>
      <DescriptionsItem :label="$t('customer.mainContactName')">{{ detail.mainContactName ?? '-' }}</DescriptionsItem>
      <DescriptionsItem :label="$t('customer.mainContactPhone')">{{ detail.mainContactPhone ?? '-' }}</DescriptionsItem>
      <DescriptionsItem :label="$t('customer.contactQq')">{{ detail.contactQq ?? '-' }}</DescriptionsItem>
      <DescriptionsItem :label="$t('customer.contactWechat')">{{ detail.contactWechat ?? '-' }}</DescriptionsItem>
      <DescriptionsItem :label="$t('customer.phoneRegion')">{{ phoneRegionText }}</DescriptionsItem>
      <DescriptionsItem :label="$t('customer.projectRegion')">{{ projectRegionText }}</DescriptionsItem>
      <DescriptionsItem :label="$t('customer.consultationContent')">
        <span class="whitespace-pre-wrap">{{ detail.consultationContent ?? '-' }}</span>
      </DescriptionsItem>
      <DescriptionsItem :label="$t('customer.creatorName')">{{ detail.creatorName ?? '-' }}</DescriptionsItem>
      <DescriptionsItem :label="$t('customer.claimUserName')">{{ detail.ownerName ?? '-' }}</DescriptionsItem>
      <DescriptionsItem :label="$t('customer.claimTime')">{{ detail.claimedAt ?? '-' }}</DescriptionsItem>
      <DescriptionsItem :label="$t('customer.remark')">{{ detail.remark ?? '-' }}</DescriptionsItem>
      <DescriptionsItem :label="$t('customer.createdAt')">{{ detail.createdAt }}</DescriptionsItem>
    </Descriptions>
  </Drawer>
</template>
