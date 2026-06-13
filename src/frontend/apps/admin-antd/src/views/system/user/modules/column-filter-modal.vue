<script lang="ts" setup>
import { computed, ref, watch } from 'vue';

import { Button, Checkbox, Modal, Spin } from 'ant-design-vue';

import { $t } from '#/locales';

const props = defineProps<{
  appliedValues: string[];
  loading?: boolean;
  modalTitle: string;
  open: boolean;
  options: Array<{ label: string; value: string }>;
}>();

const emit = defineEmits<{
  apply: [string[]];
  clear: [];
  'update:open': [boolean];
}>();

const draft = ref<string[]>([]);

watch(
  () => props.open,
  (isOpen) => {
    if (isOpen) {
      draft.value = [...props.appliedValues];
    }
  },
  { immediate: true },
);

const optionValues = computed(() => props.options.map((item) => item.value));

const allChecked = computed(
  () =>
    optionValues.value.length > 0
    && optionValues.value.every((value) => draft.value.includes(value)),
);

const indeterminate = computed(
  () =>
    draft.value.length > 0
    && !allChecked.value
    && optionValues.value.some((value) => draft.value.includes(value)),
);

function toggleAll(checked: boolean) {
  draft.value = checked ? [...optionValues.value] : [];
}

function close() {
  emit('update:open', false);
}

function onApply() {
  emit('apply', [...draft.value]);
  close();
}

function onClear() {
  emit('clear');
  close();
}
</script>

<template>
  <Modal
    :open="open"
    :title="modalTitle"
    :footer="null"
    destroy-on-close
    @cancel="close"
  >
    <Spin :spinning="loading">
      <div class="mb-3 flex items-center justify-between gap-2">
        <Checkbox
          :checked="allChecked"
          :indeterminate="indeterminate"
          @change="(event) => toggleAll(event.target.checked)"
        >
          {{ $t('common.selectAll') }}
        </Checkbox>
        <div class="flex gap-2">
          <Button size="small" @click="onClear">
            {{ $t('common.reset') }}
          </Button>
          <Button size="small" type="primary" @click="onApply">
            {{ $t('common.confirm') }}
          </Button>
        </div>
      </div>
      <Checkbox.Group v-model:value="draft" class="flex w-full flex-col gap-2">
        <Checkbox v-for="item in options" :key="item.value" :value="item.value">
          {{ item.label }}
        </Checkbox>
      </Checkbox.Group>
    </Spin>
  </Modal>
</template>
