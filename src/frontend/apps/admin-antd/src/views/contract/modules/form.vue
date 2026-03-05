<script lang="ts" setup>
import type { ContractApi } from '#/api/system/contract';
import type {
  ContractTypeOptionItem,
  IncomeExpenseTypeOptionItem,
} from '../data';

import { computed, onMounted, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { Button } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import {
  createContract,
  updateContract,
} from '#/api/system/contract';
import type { ContractTypeOptionApi } from '#/api/system/contract-type';
import type { IncomeExpenseTypeOptionApi } from '#/api/system/income-expense-type';
import { getContractTypeOptionList } from '#/api/system/contract-type';
import { getIncomeExpenseTypeOptionList } from '#/api/system/income-expense-type';
import { $t } from '#/locales';

import { useSchema } from '../data';

const emit = defineEmits(['success']);
const formData = ref<Partial<ContractApi.ContractItem> & { id?: string }>();

const contractTypeOptions = ref<ContractTypeOptionItem[]>([]);
const incomeExpenseTypeOptions = ref<IncomeExpenseTypeOptionItem[]>([]);

onMounted(async () => {
  const [ctList, ieList] = await Promise.all([
    getContractTypeOptionList(),
    getIncomeExpenseTypeOptionList(),
  ]) as unknown as [ContractTypeOptionApi.ContractTypeOptionItem[], IncomeExpenseTypeOptionApi.IncomeExpenseTypeOptionItem[]];
  contractTypeOptions.value = ctList.map((x) => ({ label: x.name, value: x.typeValue }));
  incomeExpenseTypeOptions.value = ieList.map((x) => ({ label: x.name, value: x.typeValue }));
});

const formSchema = computed(() =>
  useSchema(contractTypeOptions.value, incomeExpenseTypeOptions.value),
);

const [Form, formApi] = useVbenForm({
  layout: 'vertical',
  schema: formSchema as unknown as ReturnType<typeof useSchema>,
  showDefaultActions: false,
});

function resetForm() {
  formApi.resetForm();
  formApi.setValues(formData.value || {});
}

const [Drawer, drawerApi] = useVbenDrawer({
  async onConfirm() {
    const { valid } = await formApi.validate();
    if (!valid) return;
    drawerApi.lock();
    const data = await formApi.getValues();
    const payload = {
      code: String(data.code),
      title: String(data.title),
      partyA: String(data.partyA),
      partyB: String(data.partyB),
      amount: Number(data.amount) ?? 0,
      startDate: data.startDate
        ? new Date(data.startDate).toISOString()
        : new Date().toISOString(),
      endDate: data.endDate
        ? new Date(data.endDate).toISOString()
        : new Date().toISOString(),
      fileStorageKey: data.fileStorageKey ? String(data.fileStorageKey) : undefined,
      contractType: data.contractType != null ? Number(data.contractType) : 0,
      incomeExpenseType: data.incomeExpenseType != null ? Number(data.incomeExpenseType) : 0,
      signDate: data.signDate ? new Date(data.signDate).toISOString() : undefined,
      note: data.note ? String(data.note) : undefined,
      description: data.description ? String(data.description) : undefined,
      ...(data.orderId && { orderId: String(data.orderId) }),
      ...(data.customerId && { customerId: String(data.customerId) }),
    };
    try {
      if (formData.value?.id) {
        await updateContract(formData.value.id, payload);
      } else {
        await createContract(payload);
      }
      drawerApi.close();
      emit('success');
    } finally {
      drawerApi.lock(false);
    }
  },
  onOpenChange(isOpen) {
    if (isOpen) {
      const data = drawerApi.getData<Partial<ContractApi.ContractItem> & { id?: string; orderId?: string }>();
      formData.value = data;
      const start = data?.startDate
        ? (typeof data.startDate === 'string'
            ? data.startDate.slice(0, 10)
            : '')
        : '';
      const end = data?.endDate
        ? (typeof data.endDate === 'string'
            ? data.endDate.slice(0, 10)
            : '')
        : '';
      const sign = data?.signDate
        ? (typeof data.signDate === 'string'
            ? data.signDate.slice(0, 10)
            : '')
        : '';
      formApi.setValues({
        code: data?.code ?? '',
        title: data?.title ?? '',
        partyA: data?.partyA ?? '',
        partyB: data?.partyB ?? '',
        amount: data?.amount ?? 0,
        startDate: start,
        endDate: end,
        contractType: data?.contractType ?? 0,
        incomeExpenseType: data?.incomeExpenseType ?? 0,
        signDate: sign,
        note: data?.note ?? '',
        description: data?.description ?? '',
        fileStorageKey: data?.fileStorageKey ?? '',
        orderId: data?.orderId ?? '',
        customerId: data?.customerId ?? '',
      });
    }
  },
});
</script>

<template>
  <Drawer :title="formData?.id ? $t('contract.edit') : $t('contract.create')">
    <Form class="mx-4" />
    <template #prepend-footer>
      <div class="flex-auto">
        <Button type="primary" danger @click="resetForm">
          {{ $t('common.reset') }}
        </Button>
      </div>
    </template>
  </Drawer>
</template>
