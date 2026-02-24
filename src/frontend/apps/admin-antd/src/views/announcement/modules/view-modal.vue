<script lang="ts" setup>
import type { AnnouncementApi } from '#/api/system/announcement';

import { ref } from 'vue';

import { useVbenModal } from '@vben/common-ui';

import { Button } from 'ant-design-vue';

import { markAnnouncementRead } from '#/api/system/announcement';
import { $t } from '#/locales';

const emit = defineEmits(['read']);
const detail = ref<AnnouncementApi.AnnouncementItem | null>(null);

const [Modal, modalApi] = useVbenModal({
  async onConfirm() {
    if (!detail.value?.id) return;
    modalApi.lock();
    try {
      await markAnnouncementRead(detail.value.id);
      modalApi.close();
      emit('read');
    } finally {
      modalApi.lock(false);
    }
  },
  onOpenChange(isOpen) {
    if (isOpen) {
      detail.value = modalApi.getData<AnnouncementApi.AnnouncementItem>() ?? null;
    } else {
      detail.value = null;
    }
  },
});
</script>

<template>
  <Modal :title="$t('announcement.detail')">
    <template #default>
      <div v-if="detail" class="px-4 space-y-3">
        <div class="space-y-2">
          <div>
            <span class="text-gray-500">{{ $t('announcement.titleField') }}：</span>
            <span class="font-medium">{{ detail.title }}</span>
          </div>
          <div>
            <span class="text-gray-500">{{ $t('announcement.publisher') }}：</span>
            <span>{{ detail.publisherName }}</span>
          </div>
          <div v-if="detail.publishAt">
            <span class="text-gray-500">{{ $t('announcement.publishAt') }}：</span>
            <span>{{ detail.publishAt }}</span>
          </div>
          <div class="border-t pt-3">
            <span class="text-gray-500 block mb-1">{{ $t('announcement.content') }}：</span>
            <div class="whitespace-pre-wrap rounded bg-gray-50 p-3">{{ detail.content }}</div>
          </div>
        </div>
      </div>
    </template>
    <template #footer>
      <Button @click="modalApi.close()">{{ $t('common.close') }}</Button>
      <Button v-if="detail?.status === 1 && detail?.isRead !== true" type="primary" @click="modalApi.submit()">
        {{ $t('announcement.markRead') }}
      </Button>
    </template>
  </Modal>
</template>
