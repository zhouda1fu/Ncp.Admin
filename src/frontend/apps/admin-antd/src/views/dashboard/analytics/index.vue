<script lang="ts" setup>
import type { HomeDashboardApi } from '#/api/system/home-dashboard';

import { onMounted, ref } from 'vue';

import { Page } from '@vben/common-ui';

import { getHomeDashboard } from '#/api/system/home-dashboard';

import HomeDashboardCards from './HomeDashboardCards.vue';

const loading = ref(false);
const dashboard = ref<HomeDashboardApi.HomeDashboard | null>(null);
const calendarQuery = ref<{ calendarYear?: number; calendarMonth?: number }>({});

async function loadDashboard(calendar?: { year: number; month: number }) {
  loading.value = true;
  try {
    if (calendar) {
      calendarQuery.value = {
        calendarYear: calendar.year,
        calendarMonth: calendar.month,
      };
    }
    dashboard.value = await getHomeDashboard(
      Object.keys(calendarQuery.value).length > 0 ? calendarQuery.value : undefined,
    );
  } finally {
    loading.value = false;
  }
}

onMounted(() => {
  loadDashboard();
});
</script>

<template>
  <Page auto-content-height>
    <HomeDashboardCards
      :data="dashboard"
      :loading="loading"
      @refresh="(cal) => loadDashboard(cal)"
    />
  </Page>
</template>
