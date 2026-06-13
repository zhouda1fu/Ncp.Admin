<script setup lang="ts">
import type { UserInfo } from '@vben/types';

import { computed, onMounted, ref } from 'vue';

import { Card, Space, Tag } from 'ant-design-vue';

import { getUserInfoApi } from '#/api';

const profile = ref<UserInfo & Record<string, any>>();
const loading = ref(false);

const displayName = computed(
  () => profile.value?.realName || profile.value?.name || profile.value?.username || '-',
);

const infoItems = computed(() => [
  { label: '用户名', value: profile.value?.name || profile.value?.username || '-' },
  { label: '手机号', value: profile.value?.phone || '-' },
  { label: '邮箱', value: profile.value?.email || '-' },
  { label: '部门', value: profile.value?.deptName || '-' },
]);

async function loadProfile() {
  loading.value = true;
  try {
    profile.value = (await getUserInfoApi()) as UserInfo & Record<string, any>;
  } finally {
    loading.value = false;
  }
}

onMounted(loadProfile);
</script>

<template>
  <Card :bordered="false" :loading="loading" class="profile-basic-card">
    <div class="profile-header">
      <div class="profile-title">
        <div class="profile-name">{{ displayName }}</div>
        <div class="profile-subtitle">
          {{ profile?.deptName || '未分配部门' }}
        </div>
        <Space class="profile-tags" wrap>
          <Tag :color="profile?.status === 1 ? 'success' : 'default'">
            {{ profile?.status === 1 ? '启用' : '禁用' }}
          </Tag>
          <Tag v-for="role in profile?.roles ?? []" :key="role">
            {{ role }}
          </Tag>
        </Space>
      </div>
    </div>

    <div class="info-grid">
      <div v-for="item in infoItems" :key="item.label" class="info-item">
        <span>{{ item.label }}</span>
        <strong>{{ item.value }}</strong>
      </div>
    </div>
  </Card>
</template>

<style scoped>
.profile-basic-card {
  max-width: 760px;
}

.profile-header {
  display: flex;
  align-items: center;
  padding-bottom: 22px;
  border-bottom: 1px solid hsl(var(--border));
}

.profile-title {
  min-width: 0;
}

.profile-name {
  color: hsl(var(--foreground));
  margin-bottom: 4px;
  overflow: hidden;
  font-size: 20px;
  font-weight: 600;
  line-height: 1.3;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.profile-subtitle {
  margin-bottom: 8px;
  color: hsl(var(--muted-foreground));
}

.profile-tags {
  min-height: 24px;
}

.info-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 14px;
  margin-top: 22px;
}

.info-item {
  min-width: 0;
  padding: 14px 16px;
  background: hsl(var(--muted) / 45%);
  border: 1px solid hsl(var(--border));
  border-radius: 8px;
}

.info-item span {
  display: block;
  margin-bottom: 6px;
  font-size: 12px;
  color: hsl(var(--muted-foreground));
}

.info-item strong {
  display: block;
  overflow: hidden;
  font-weight: 500;
  color: hsl(var(--foreground));
  text-overflow: ellipsis;
  white-space: nowrap;
}

@media (max-width: 640px) {
  .profile-header {
    align-items: flex-start;
  }

  .info-grid {
    grid-template-columns: 1fr;
  }
}
</style>
