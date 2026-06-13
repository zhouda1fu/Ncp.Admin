<script lang="ts" setup>
import { computed, ref, watch } from 'vue';

import VueOfficeDocx from '@vue-office/docx';
import VueOfficeExcel from '@vue-office/excel';
import VueOfficePdf from '@vue-office/pdf';

import '@vue-office/docx/lib/index.css';
import '@vue-office/excel/lib/index.css';

import { IconifyIcon } from '@vben/icons';

import { Button, Modal, Space, Spin, Tooltip } from 'ant-design-vue';

import { fetchFileBlob, fetchFilePreviewBlob } from '#/api/system/file';
import type { FilePreviewType } from '#/utils/file-type';
import { getFilePreviewType } from '#/utils/file-type';

const props = defineProps<{
  /** 弹窗是否打开。 */
  open: boolean;
  /** 文件存储路径或业务文件 key，用于默认文件接口拉取。 */
  filePath: string;
  /** 原始文件名，用于判断预览类型和展示标题。 */
  fileName: string;
  /** 已在前端内存中的 Blob，比如反馈提交前刚上传的本地文件。 */
  previewBlob?: Blob | null;
  /** 自定义 Blob 拉取函数，用于反馈附件这类需要业务鉴权的下载接口。 */
  fetchBlob?: (filePath: string) => Promise<Blob>;
}>();

const emit = defineEmits<{
  'update:open': [value: boolean];
}>();

const loading = ref(false);
const error = ref('');
const fileBuffer = ref<ArrayBuffer | null>(null);
const blobUrl = ref('');
const textContent = ref('');
const imageZoom = ref(1);
const imageRotation = ref(0);

/** 旧版 .doc 需走预览接口转 docx 后渲染 */
const isLegacyDoc = computed(
  () => /\.doc$/i.test(props.fileName ?? '') && !/\.docx$/i.test(props.fileName ?? ''),
);

const previewType = computed<FilePreviewType>(() => {
  if (!props.fileName) return 'unsupported';
  if (isLegacyDoc.value) return 'docx';
  return getFilePreviewType(props.fileName);
});

const isPdf = computed(() => previewType.value === 'pdf');
const isDocx = computed(() => previewType.value === 'docx');
const isExcel = computed(() => previewType.value === 'excel');
const isImage = computed(() => previewType.value === 'image');
const isText = computed(() => previewType.value === 'text');
const isUnsupported = computed(() => previewType.value === 'unsupported');
const imageStyle = computed(() => ({
  maxHeight: '70vh',
  maxWidth: '100%',
  transform: `rotate(${imageRotation.value}deg) scale(${imageZoom.value})`,
}));

function revokeBlobUrl() {
  if (blobUrl.value) {
    URL.revokeObjectURL(blobUrl.value);
    blobUrl.value = '';
  }
}

/** 加载并解析预览文件，按类型分发给图片、PDF、Office 或文本预览器。 */
async function loadFile() {
  if ((!props.filePath && !props.previewBlob) || !props.open) return;
  loading.value = true;
  error.value = '';
  fileBuffer.value = null;
  revokeBlobUrl();
  textContent.value = '';
  resetImageTransform();
  try {
    const blob = props.previewBlob
      ?? (props.fetchBlob
        ? await props.fetchBlob(props.filePath)
        : isLegacyDoc.value || isPdf.value || isDocx.value || isExcel.value
          ? await fetchFilePreviewBlob(props.filePath)
          : await fetchFileBlob(props.filePath));
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
  () => [props.open, props.filePath, props.previewBlob] as const,
  ([open, path, blob]) => {
    if (open && (path || blob)) {
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

/** 重置图片缩放和旋转状态。 */
function resetImageTransform() {
  imageZoom.value = 1;
  imageRotation.value = 0;
}

/** 调整图片缩放比例。 */
function zoomImage(delta: number) {
  imageZoom.value = Math.min(3, Math.max(0.25, Number((imageZoom.value + delta).toFixed(2))));
}

/** 调整图片旋转角度。 */
function rotateImage(delta: number) {
  imageRotation.value = (imageRotation.value + delta) % 360;
}

/** 鼠标滚轮缩放图片。 */
function handleImageWheel(event: WheelEvent) {
  if (event.deltaY < 0) {
    zoomImage(0.15);
  } else if (event.deltaY > 0) {
    zoomImage(-0.15);
  }
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
        <div v-else-if="isImage && blobUrl" class="min-h-[70vh]">
          <div class="mb-3 flex justify-center">
            <Space>
              <Tooltip title="缩小">
                <Button size="small" @click="zoomImage(-0.25)">
                  <template #icon>
                    <IconifyIcon icon="mdi:magnify-minus-outline" class="size-4" />
                  </template>
                </Button>
              </Tooltip>
              <Tooltip title="放大">
                <Button size="small" @click="zoomImage(0.25)">
                  <template #icon>
                    <IconifyIcon icon="mdi:magnify-plus-outline" class="size-4" />
                  </template>
                </Button>
              </Tooltip>
              <Tooltip title="向左旋转">
                <Button size="small" @click="rotateImage(-90)">
                  <template #icon>
                    <IconifyIcon icon="mdi:rotate-left" class="size-4" />
                  </template>
                </Button>
              </Tooltip>
              <Tooltip title="向右旋转">
                <Button size="small" @click="rotateImage(90)">
                  <template #icon>
                    <IconifyIcon icon="mdi:rotate-right" class="size-4" />
                  </template>
                </Button>
              </Tooltip>
              <Tooltip title="重置">
                <Button size="small" @click="resetImageTransform">
                  <template #icon>
                    <IconifyIcon icon="mdi:restore" class="size-4" />
                  </template>
                </Button>
              </Tooltip>
            </Space>
          </div>
          <div class="flex h-[70vh] items-center justify-center overflow-auto" @wheel.prevent="handleImageWheel">
            <img
              :src="blobUrl"
              :style="imageStyle"
              class="cursor-zoom-in object-contain transition-transform"
            />
          </div>
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
