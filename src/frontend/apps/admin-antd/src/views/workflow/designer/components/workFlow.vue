<template>
  <div class="workflow-designer-shell flex h-full min-h-[420px] flex-col">
    <div
      class="workflow-designer-canvas relative flex-1 overflow-hidden rounded-2xl border border-[hsl(var(--border)_/_0.85)] bg-[hsl(var(--muted)_/_0.25)] shadow-[inset_0_1px_2px_hsl(var(--foreground)_/_0.04)]"
    >
      <div class="workflow-designer-canvas__pattern pointer-events-none absolute inset-0 rounded-2xl opacity-[0.65]" />
      <div class="workflow-designer-canvas__inner relative flex h-full min-h-0 flex-col">
        <div
          ref="panViewportRef"
          class="workflow-pan-viewport relative min-h-0 flex-1 touch-none overflow-hidden outline-none"
          :class="viewportCursorClass"
          tabindex="-1"
          @pointerdown="onPanPointerDown"
          @pointermove="onPanPointerMove"
          @pointerup="onPanPointerUp"
          @pointercancel="onPanPointerUp"
          @wheel.prevent="handleWheel"
        >
          <div
            class="workflow-pan-stage pointer-events-none absolute left-1/2 top-4 flex w-max max-w-none justify-center"
            :style="stageStyle"
          >
            <div class="pointer-events-auto">
              <sc-workflow
                v-if="innerConfig !== undefined"
                class="workflow"
                :model-value="innerConfig"
                :category="category"
                :view-only="viewOnly"
                @update:model-value="onConfigUpdate"
              />
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, nextTick, onMounted, onUnmounted, ref, watch } from 'vue';

import scWorkflow from './scWorkflow/index.vue';

const props = defineProps<{
  modelValue?: Record<string, any> | null;
  initialConfig?: Record<string, any>;
  category?: string;
  viewOnly?: boolean;
}>();
const category = computed(() => props.category ?? '');
const viewOnly = computed(() => props.viewOnly ?? false);

const emit = defineEmits<{
  'update:modelValue': [value: Record<string, any> | null];
}>();

const zoom = ref(1);
const pan = ref({ x: 0, y: 0 });
const isPanning = ref(false);
const spacePressed = ref(false);
const panViewportRef = ref<HTMLElement | null>(null);

const panDrag = ref<{
  pointerId: number;
  startClientX: number;
  startClientY: number;
  originPanX: number;
  originPanY: number;
} | null>(null);

const viewportCursorClass = computed(() => {
  if (isPanning.value) return 'cursor-grabbing select-none';
  if (spacePressed.value) return 'cursor-grab';
  return '';
});

const stageStyle = computed(() => {
  const z = zoom.value;
  const { x, y } = pan.value;
  return {
    transform: `translate(-50%, 0) translate(${x}px, ${y}px) scale(${z})`,
    transformOrigin: 'top center',
  };
});

function isInteractiveTarget(target: EventTarget | null): boolean {
  if (!target || !(target instanceof Element)) return false;
  return !!target.closest(
    [
      '.node-wrap-box',
      '.auto-judge',
      '.add-node-btn-box',
      'button',
      'input',
      'textarea',
      '.ant-select',
      '.ant-popover',
      '.ant-drawer',
      '.ant-modal',
      '[role="dialog"]',
    ].join(','),
  );
}

function onPanPointerDown(e: PointerEvent) {
  if (e.button !== 0 && e.button !== 1) return;

  if (e.button === 1) {
    e.preventDefault();
    beginPan(e);
    return;
  }

  if (spacePressed.value) {
    e.preventDefault();
    beginPan(e);
    return;
  }

  if (isInteractiveTarget(e.target)) return;

  e.preventDefault();
  beginPan(e);
}

function beginPan(e: PointerEvent) {
  isPanning.value = true;
  panDrag.value = {
    pointerId: e.pointerId,
    startClientX: e.clientX,
    startClientY: e.clientY,
    originPanX: pan.value.x,
    originPanY: pan.value.y,
  };
  const el = panViewportRef.value;
  if (el) {
    try {
      el.setPointerCapture(e.pointerId);
    } catch {
      /* ignore */
    }
    el.focus({ preventScroll: true });
  }
}

function onPanPointerMove(e: PointerEvent) {
  if (!isPanning.value || !panDrag.value) return;
  if (e.pointerId !== panDrag.value.pointerId) return;
  const d = panDrag.value;
  pan.value = {
    x: d.originPanX + (e.clientX - d.startClientX),
    y: d.originPanY + (e.clientY - d.startClientY),
  };
}

function endPan(e: PointerEvent) {
  if (!isPanning.value || !panDrag.value) return;
  if (e.pointerId !== panDrag.value.pointerId) return;
  const el = panViewportRef.value;
  if (el) {
    try {
      el.releasePointerCapture(panDrag.value.pointerId);
    } catch {
      /* ignore */
    }
  }
  isPanning.value = false;
  panDrag.value = null;
}

function onPanPointerUp(e: PointerEvent) {
  endPan(e);
}

function handleWheel(e: WheelEvent) {
  if (e.deltaY < 0) {
    zoom.value += 0.1;
  } else {
    zoom.value -= 0.1;
  }
  if (zoom.value <= 0.1) zoom.value = 0.1;
  else if (zoom.value >= 5) zoom.value = 5;
}

function onWindowKeydown(e: KeyboardEvent) {
  if (e.code !== 'Space') return;
  const t = e.target;
  if (t instanceof HTMLInputElement || t instanceof HTMLTextAreaElement) return;
  if (t instanceof HTMLElement && t.isContentEditable) return;
  e.preventDefault();
  spacePressed.value = true;
}

function onWindowKeyup(e: KeyboardEvent) {
  if (e.code === 'Space') spacePressed.value = false;
}

const defaultRoot = () => ({
  nodeName: '发起人',
  nodeKey: 'root_' + Date.now(),
  type: 0,
  nodeAssigneeList: [],
  childNode: null,
});

const innerConfig = ref<Record<string, any> | undefined>(
  props.modelValue ?? props.initialConfig ?? defaultRoot(),
);

watch(
  () => props.modelValue,
  (val) => {
    if (val != null) innerConfig.value = val;
  },
  { immediate: true },
);
watch(
  () => props.initialConfig,
  (val) => {
    if (innerConfig.value === undefined && val != null) {
      innerConfig.value = val;
      emit('update:modelValue', val);
    }
  },
  { immediate: true },
);

function onConfigUpdate(val: Record<string, any> | null) {
  innerConfig.value = val ?? undefined;
  emit('update:modelValue', val ?? null);
}

onMounted(() => {
  window.addEventListener('keydown', onWindowKeydown);
  window.addEventListener('keyup', onWindowKeyup);
  nextTick(() => {
    if (props.modelValue == null && innerConfig.value != null) {
      emit('update:modelValue', innerConfig.value);
    }
  });
});

onUnmounted(() => {
  window.removeEventListener('keydown', onWindowKeydown);
  window.removeEventListener('keyup', onWindowKeyup);
});
</script>

<style scoped>
.workflow-designer-canvas__pattern {
  background-color: hsl(var(--background));
  background-image:
    radial-gradient(hsl(var(--foreground) / 0.06) 1px, transparent 1px),
    linear-gradient(hsl(var(--border) / 0.35) 1px, transparent 1px),
    linear-gradient(90deg, hsl(var(--border) / 0.35) 1px, transparent 1px);
  background-size:
    20px 20px,
    100% 100%,
    100% 100%;
  background-position: 0 0, 0 0, 0 0;
}

.workflow-pan-viewport:focus-visible {
  box-shadow: inset 0 0 0 2px hsl(var(--primary) / 0.35);
  border-radius: 4px;
}

.workflow {
  padding: 8px 4px 16px;
}
</style>

<style>
#flowlong-designer-root.workflow-designer-root {
  margin: 0;
  min-height: 100%;
  background: linear-gradient(
    180deg,
    hsl(var(--background)) 0%,
    hsl(var(--muted) / 0.35) 48%,
    hsl(var(--background)) 100%
  );
}
</style>
