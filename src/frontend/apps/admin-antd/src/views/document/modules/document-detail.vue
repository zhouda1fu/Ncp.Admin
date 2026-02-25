<script lang="ts" setup>
import type { DocumentApi } from '#/api/system/document';

import { computed, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { Button } from 'ant-design-vue';

import {
  addDocumentVersion,
  createShareLink,
  getDocumentById,
  getVersionDownloadPath,
  updateDocumentTitle,
} from '#/api/system/document';
import { useAppConfig } from '@vben/hooks';
import { message } from 'ant-design-vue';
import { $t } from '#/locales';

const emit = defineEmits(['success']);
const { apiURL } = useAppConfig(import.meta.env, import.meta.env.PROD);
const doc = ref<DocumentApi.DocumentItem | null>(null);
const editTitle = ref('');
const addingVersion = ref(false);
const shareLinkToken = ref('');

const documentId = computed(() => doc.value?.id ?? '');
const versions = computed(() => {
  if (!doc.value) return [];
  const cur = doc.value.currentVersion;
  if (!cur) return [];
  return [cur];
});

const [Drawer, drawerApi] = useVbenDrawer({
  showConfirmButton: false,
  onOpenChange(isOpen) {
    if (isOpen) {
      const data = drawerApi.getData<DocumentApi.DocumentItem>();
      doc.value = data ?? null;
      editTitle.value = doc.value?.title ?? '';
      shareLinkToken.value = '';
      if (data?.id) loadDocument(data.id);
    }
  },
});

async function loadDocument(id: string) {
  const res = await getDocumentById(id);
  doc.value = res ?? null;
  if (doc.value) editTitle.value = doc.value.title;
}

function getDownloadUrl(versionId: string) {
  if (!documentId.value) return '#';
  return `${apiURL}${getVersionDownloadPath(documentId.value, versionId)}`;
}

async function onSaveTitle() {
  if (!doc.value || !editTitle.value.trim()) return;
  await updateDocumentTitle(doc.value.id, editTitle.value.trim());
  message.success($t('common.saveSuccess'));
  emit('success');
  await loadDocument(doc.value.id);
}

async function onAddVersion(e: Event) {
  const input = e.target as HTMLInputElement;
  if (!input.files?.length || !doc.value) return;
  addingVersion.value = true;
  try {
    const formData = new FormData();
    formData.append('file', input.files[0]);
    await addDocumentVersion(doc.value.id, formData);
    message.success($t('document.addVersion'));
    emit('success');
    await loadDocument(doc.value.id);
  } finally {
    addingVersion.value = false;
    input.value = '';
  }
}

async function onCreateShareLink() {
  if (!doc.value) return;
  const res = await createShareLink({ documentId: doc.value.id });
  shareLinkToken.value = res.token;
  message.success($t('document.createShareLink'));
}
</script>

<template>
  <Drawer :title="doc?.title ?? $t('document.name')">
    <div v-if="doc" class="mx-4 space-y-4">
      <div>
        <label class="block text-sm font-medium mb-1">{{ $t('document.titleField') }}</label>
        <div class="flex gap-2">
          <input
            v-model="editTitle"
            type="text"
            class="flex-1 border rounded px-3 py-2"
          />
          <Button type="primary" @click="onSaveTitle">{{ $t('common.save') }}</Button>
        </div>
      </div>
      <div>
        <div class="flex items-center justify-between mb-2">
          <span class="text-sm font-medium">{{ $t('document.version') }}</span>
          <label class="cursor-pointer">
            <input type="file" class="hidden" @change="onAddVersion" />
            <Button type="link" size="small" :loading="addingVersion">
              {{ $t('document.addVersion') }}
            </Button>
          </label>
        </div>
        <ul class="list-disc list-inside text-sm space-y-1">
          <li v-for="v in versions" :key="v.id">
            <a
              :href="getDownloadUrl(v.id)"
              target="_blank"
              rel="noopener noreferrer"
            >
              {{ v.fileName }} (v{{ v.versionNumber }})
            </a>
          </li>
        </ul>
      </div>
      <div>
        <Button type="primary" ghost class="mb-2" @click="onCreateShareLink">
          {{ $t('document.createShareLink') }}
        </Button>
        <p v-if="shareLinkToken" class="text-sm text-gray-600 break-all">
          {{ $t('document.shareLink') }}: {{ shareLinkToken }}
        </p>
      </div>
    </div>
  </Drawer>
</template>
