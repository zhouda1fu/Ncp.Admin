<script setup lang="ts">
import type { UserInfo } from '@vben/types';

import { computed, onMounted, ref } from 'vue';

import { Profile } from '@vben/common-ui';
import { useUserStore } from '@vben/stores';

import { message } from 'ant-design-vue';

import { getUserInfoApi, uploadCurrentUserAvatarApi } from '#/api';
import { resolveUserAvatarBlobUrl } from '#/utils/user-avatar';

import ProfileBase from './base-setting.vue';
import ProfilePasswordSetting from './password-setting.vue';

const userStore = useUserStore();

const tabsValue = ref<string>('basic');
const avatarInputRef = ref<HTMLInputElement>();
const avatarUploading = ref(false);
const profileUserInfo = computed(() => userStore.userInfo as (UserInfo & Record<string, any>) | null);

const tabs = ref([
  {
    label: '基本设置',
    value: 'basic',
  },
  {
    label: '修改密码',
    value: 'password',
  },
]);

function setProfileUserInfo(data: Record<string, any>) {
  userStore.setUserInfo({
    ...(userStore.userInfo ?? {}),
    ...data,
  } as unknown as UserInfo);
}

async function refreshAvatarPreview(avatarUrl?: string) {
  const avatar = avatarUrl ? await resolveUserAvatarBlobUrl(avatarUrl) : '';
  setProfileUserInfo({
    avatar,
    avatarUrl: avatarUrl ?? '',
  });
}

async function loadCurrentProfile() {
  const data = (await getUserInfoApi()) as UserInfo & Record<string, any>;
  setProfileUserInfo({
    ...data,
    username: data.username || data.name,
  });
  await refreshAvatarPreview(data.avatarUrl);
}

function openAvatarPicker() {
  if (!avatarUploading.value) {
    avatarInputRef.value?.click();
  }
}

async function handleAvatarChange(event: Event) {
  const input = event.target as HTMLInputElement;
  const file = input.files?.[0];
  input.value = '';

  if (!file) {
    return;
  }

  if (!file.type.startsWith('image/')) {
    message.warning('请选择图片文件');
    return;
  }

  avatarUploading.value = true;
  try {
    const { avatarUrl } = await uploadCurrentUserAvatarApi(file);
    await refreshAvatarPreview(avatarUrl);
    message.success('头像已更新');
  } finally {
    avatarUploading.value = false;
  }
}

onMounted(loadCurrentProfile);
</script>
<template>
  <Profile
    v-model:model-value="tabsValue"
    title="个人中心"
    :user-info="profileUserInfo"
    :tabs="tabs"
  >
    <template #avatar="{ avatar }">
      <button
        class="profile-avatar-upload"
        type="button"
        :disabled="avatarUploading"
        @click="openAvatarPicker"
      >
        <img :src="avatar" alt="头像" />
        <span class="profile-avatar-mask">
          {{ avatarUploading ? '上传中' : '更换头像' }}
        </span>
      </button>
      <input
        ref="avatarInputRef"
        accept="image/*"
        class="profile-avatar-input"
        type="file"
        @change="handleAvatarChange"
      />
    </template>

    <template #content>
      <ProfileBase v-if="tabsValue === 'basic'" />
      <ProfilePasswordSetting v-if="tabsValue === 'password'" />
    </template>
  </Profile>
</template>

<style scoped>
.profile-avatar-upload {
  position: relative;
  width: 80px;
  height: 80px;
  padding: 0;
  overflow: hidden;
  cursor: pointer;
  background: transparent;
  border: 0;
  border-radius: 999px;
}

.profile-avatar-upload:disabled {
  cursor: wait;
}

.profile-avatar-upload img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  border-radius: inherit;
}

.profile-avatar-mask {
  position: absolute;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  color: #fff;
  opacity: 0;
  background: rgb(0 0 0 / 45%);
  transition: opacity 0.2s ease;
}

.profile-avatar-upload:hover .profile-avatar-mask {
  opacity: 1;
}

.profile-avatar-input {
  display: none;
}
</style>
