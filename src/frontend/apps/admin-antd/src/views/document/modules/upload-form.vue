<script lang="ts" setup>
import { ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { uploadDocument } from '#/api/system/document';
import { $t } from '#/locales';

const emit = defineEmits(['success']);
const title = ref('');
const fileList = ref<File[]>([]);

const [Drawer, drawerApi] = useVbenDrawer({
  async onConfirm() {
    if (!title.value.trim()) return;
    if (fileList.value.length === 0) return;
    drawerApi.lock();
    try {
      const formData = new FormData();
      formData.append('title', title.value.trim());
      formData.append('file', fileList.value[0]);
      await uploadDocument(formData);
      drawerApi.close();
      emit('success');
    } finally {
      drawerApi.lock(false);
    }
  },
  onOpenChange(isOpen) {
    if (isOpen) {
      title.value = '';
      fileList.value = [];
    }
  },
});

function onFileChange(e: Event) {
  const input = e.target as HTMLInputElement;
  if (input.files) fileList.value = Array.from(input.files);
}
</script>

<template>
  <Drawer :title="$t('document.upload')">
    <div class="mx-4 space-y-4">
      <div>
        <label class="block text-sm font-medium mb-1">{{ $t('document.titleField') }}</label>
        <input
          v-model="title"
          type="text"
          class="w-full border rounded px-3 py-2"
          :placeholder="$t('document.titleField')"
        />
      </div>
      <div>
        <label class="block text-sm font-medium mb-1">{{ $t('document.name') }}</label>
        <input type="file" class="w-full" @change="onFileChange" />
        <p v-if="fileList.length" class="text-sm text-gray-500 mt-1">
          {{ fileList[0].name }} ({{ (fileList[0].size / 1024).toFixed(1) }} KB)
        </p>
      </div>
    </div>
  </Drawer>
</template>
