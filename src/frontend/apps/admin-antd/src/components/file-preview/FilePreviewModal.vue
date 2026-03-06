<script lang="ts" setup>
import { computed, ref, watch } from 'vue';

import VueOfficeDocx from '@vue-office/docx';
import VueOfficeExcel from '@vue-office/excel';
import VueOfficePdf from '@vue-office/pdf';

import '@vue-office/docx/lib/index.css';
import '@vue-office/excel/lib/index.css';

import { Image, Modal, Spin } from 'ant-design-vue';

import { fetchFileBlob } from '#/api/system/file';
import type { FilePreviewType } from '#/utils/file-type';
import { getFilePreviewType } from '#/utils/file-type';

const props = defineProps<{
  open: boolean;
  filePath: string;
  fileName: string;
}>();

const emit = defineEmits<{
  'update:open': [value: boolean];
}>();

const loading = ref(false);
const error = ref('');
const fileBuffer = ref<ArrayBuffer | null>(null);
const blobUrl = ref('');
const textContent = ref('');

const previewType = computed<FilePreviewType>(() =>
  props.fileName ? getFilePreviewType(props.fileName) : 'unsupported',
);

const isPdf = computed(() => previewType.value === 'pdf');
const isDocx = computed(() => previewType.value === 'docx');
const isExcel = computed(() => previewType.value === 'excel');
const isImage = computed(() => previewType.value === 'image');
const isText = computed(() => previewType.value === 'text');
const isUnsupported = computed(() => previewType.value === 'unsupported');

function revokeBlobUrl() {
  if (blobUrl.value) {
    URL.revokeObjectURL(blobUrl.value);
    blobUrl.value = '';
  }
}

async function loadFile() {
  if (!props.filePath || !props.open) return;
  loading.value = true;
  error.value = '';
  fileBuffer.value = null;
  revokeBlobUrl();
  textContent.value = '';
  try {
    const blob = await fetchFileBlob(props.filePath);
    const type = previewType.value;
    if (type === 'image') {
      blobUrl.value = URL.createObjectURL(blob);
    } else if (type === 'text') {
      textContent.value = await blob.text();
    } else if (type === 'pdf' || type === 'docx' || type === 'excel') {
      fileBuffer.value = await blob.arrayBuffer();
    }
  } catch (e) {
    error.value = (e as Error)?.message ?? '加载失败';
  } finally {
    loading.value = false;
  }
}

watch(
  () => [props.open, props.filePath] as const,
  ([open, path]) => {
    if (open && path) {
      loadFile();
    } else {
      revokeBlobUrl();
      fileBuffer.value = null;
      textContent.value = '';
      error.value = '';
    }
  },
  { immediate: true },
);

function handleClose() {
  revokeBlobUrl();
  emit('update:open', false);
}
</script>

<template>
  <Modal
    :open="open"
    :title="fileName"
    width="90%"
    :footer="null"
    destroy-on-close
    wrap-class-name="file-preview-modal"
    @cancel="handleClose"
  >
    <Spin :spinning="loading">
      <div v-if="error" class="py-8 text-center text-red-500">
        {{ error }}
      </div>
      <template v-else-if="!loading">
        <div v-if="isPdf && fileBuffer" class="min-h-[70vh] overflow-auto">
          <VueOfficePdf :src="fileBuffer" style="min-height: 70vh" />
        </div>
        <div v-else-if="isDocx && fileBuffer" class="min-h-[70vh] overflow-auto">
          <VueOfficeDocx :src="fileBuffer" style="min-height: 70vh" />
        </div>
        <div v-else-if="isExcel && fileBuffer" class="min-h-[70vh] overflow-auto">
          <VueOfficeExcel :src="fileBuffer" style="min-height: 70vh" />
        </div>
        <div v-else-if="isImage && blobUrl" class="flex justify-center overflow-auto">
          <Image :src="blobUrl" :preview="false" class="max-w-full" />
        </div>
        <pre
          v-else-if="isText"
          class="max-h-[70vh] overflow-auto whitespace-pre-wrap rounded border border-gray-200 bg-gray-50 p-4 text-sm"
        >{{ textContent }}</pre>
        <div v-else-if="isUnsupported" class="py-8 text-center text-gray-500">
          该格式暂不支持预览，请下载后查看。
        </div>
      </template>
    </Spin>
  </Modal>
</template>
