<script lang="ts" setup>
import type { VbenFormSchema } from '@vben/common-ui';

import { computed, markRaw } from 'vue';

import { AuthenticationLogin, SliderCaptcha, z } from '@vben/common-ui';
import { $t } from '@vben/locales';

import { useAuthStore } from '#/store';

defineOptions({ name: 'Login' });

const authStore = useAuthStore();

const formSchema = computed((): VbenFormSchema[] => {
  return [
    {
      component: 'VbenInput',
      componentProps: {
        placeholder: $t('authentication.usernameTip'),
      },
      fieldName: 'username',
      label: $t('authentication.username'),
      rules: z.string().min(1, { message: $t('authentication.usernameTip') }),
    },
    {
      component: 'VbenInputPassword',
      componentProps: {
        placeholder: $t('authentication.password'),
      },
      fieldName: 'password',
      label: $t('authentication.password'),
      rules: z.string().min(1, { message: $t('authentication.passwordTip') }),
    },
    {
      component: markRaw(SliderCaptcha),
      componentProps: {
        text: '请按住滑块推动',
      },
      fieldName: 'captcha',
      rules: z.boolean().refine((value) => value, {
        message: $t('authentication.verifyRequiredTip'),
      }),
    },
  ];
});
</script>

<template>
  <div class="login-panel">
    <AuthenticationLogin
      :form-schema="formSchema"
      :loading="authStore.loginLoading"
      :show-code-login="false"
      :show-qrcode-login="false"
      :show-register="false"
      :show-third-party-login="false"
      sub-title="请输入您的账户信息以开始管理您的项目"
      title="欢迎登录 👋"
      @submit="authStore.authLogin"
    />
  </div>
</template>

<style scoped>
.login-panel {
  width: 100%;
  max-width: 400px;
  margin: 0 auto;
}

.login-panel :deep(.mb-7) {
  margin-bottom: 20px;
}

.login-panel :deep(h2) {
  font-size: 24px;
  font-weight: 700;
  color: hsl(var(--foreground));
}

.login-panel :deep(.mb-6) {
  margin-bottom: 16px;
}

.login-panel :deep(.text-muted-foreground) {
  color: hsl(var(--muted-foreground));
}

.login-panel :deep(input) {
  height: 40px;
  border-radius: 8px;
}

.login-panel :deep(.w-full.h-10) {
  height: 40px;
  border-radius: 8px;
}
</style>
