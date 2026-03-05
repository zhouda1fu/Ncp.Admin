<script lang="ts" setup>
import type { ContractApi } from '#/api/system/contract';
import type { ContractTypeOptionItem, IncomeExpenseTypeOptionItem } from './data';

import { computed, nextTick, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';
import { ArrowLeft } from '@vben/icons';

import { Button, Card, Divider, message } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import {
  createContract,
  updateContract,
  getContract,
} from '#/api/system/contract';
import type { ContractTypeOptionApi } from '#/api/system/contract-type';
import type { IncomeExpenseTypeOptionApi } from '#/api/system/income-expense-type';
import { getContractTypeOptionList } from '#/api/system/contract-type';
import { getIncomeExpenseTypeOptionList } from '#/api/system/income-expense-type';
import { $t } from '#/locales';

import { useFormPageSchema } from './data';

const route = useRoute();
const router = useRouter();
const id = computed(() => route.params.id as string | undefined);
const isEdit = computed(() => !!id.value);

const contractTypeOptions = ref<ContractTypeOptionItem[]>([]);
const incomeExpenseTypeOptions = ref<IncomeExpenseTypeOptionItem[]>([]);

onMounted(async () => {
  const [ctList, ieList] = await Promise.all([
    getContractTypeOptionList(),
    getIncomeExpenseTypeOptionList(),
  ]) as unknown as [ContractTypeOptionApi.ContractTypeOptionItem[], IncomeExpenseTypeOptionApi.IncomeExpenseTypeOptionItem[]];
  contractTypeOptions.value = ctList.map((x) => ({ label: x.name, value: x.typeValue }));
  incomeExpenseTypeOptions.value = ieList.map((x) => ({ label: x.name, value: x.typeValue }));
  if (id.value) {
    const detail = await getContract(id.value);
    await nextTick();
    setFormValues(detail);
  } else {
    const today = new Date().toISOString().slice(0, 10);
    const orderIdFromQuery = route.query.orderId as string | undefined;
    formApi.setValues({
      status: 0,
      startDate: today,
      endDate: today,
      nextPaymentReminder: false,
      contractExpiryReminder: false,
      isInstallmentPayment: false,
      ...(orderIdFromQuery && { orderId: orderIdFromQuery }),
    });
  }
});

const formSchema = computed(() => useFormPageSchema(contractTypeOptions.value, incomeExpenseTypeOptions.value));

const [Form, formApi] = useVbenForm({
  layout: 'vertical',
  schema: formSchema as unknown as ReturnType<typeof useFormPageSchema>,
  showDefaultActions: false,
  grid: { cols: 4, collapsed: false },
});

function setFormValues(d: ContractApi.ContractItem) {
  const start = d.startDate ? String(d.startDate).slice(0, 10) : '';
  const end = d.endDate ? String(d.endDate).slice(0, 10) : '';
  const sign = d.signDate ? String(d.signDate).slice(0, 10) : '';
  formApi.setValues({
    code: d.code,
    title: d.title,
    partyA: d.partyA,
    partyB: d.partyB,
    amount: d.amount,
    startDate: start,
    endDate: end,
    status: d.status,
    contractType: d.contractType,
    incomeExpenseType: d.incomeExpenseType,
    signDate: sign,
    orderId: d.orderId ?? '',
    customerId: d.customerId ?? '',
    note: d.note ?? '',
    fileStorageKey: d.fileStorageKey ?? '',
    departmentId: d.departmentId ?? '',
    businessManager: d.businessManager ?? '',
    responsibleProject: d.responsibleProject ?? '',
    inputCustomer: d.inputCustomer ?? '',
    nextPaymentReminder: d.nextPaymentReminder ?? false,
    contractExpiryReminder: d.contractExpiryReminder ?? false,
    singleDoubleProfit: d.singleDoubleProfit,
    invoicingInformation: d.invoicingInformation ?? '',
    paymentStatus: d.paymentStatus,
    warrantyPeriod: d.warrantyPeriod ?? '',
    isInstallmentPayment: d.isInstallmentPayment ? 1 : 0,
    accumulatedAmount: d.accumulatedAmount,
  });
}

async function onSubmit() {
  const { valid } = await formApi.validate();
  if (!valid) return;
  const data = await formApi.getValues();
  const payload = {
    code: String(data.code ?? ''),
    title: String(data.title ?? ''),
    partyA: String(data.partyA ?? ''),
    partyB: String(data.partyB ?? ''),
    amount: Number(data.amount) ?? 0,
    startDate: data.startDate ? new Date(data.startDate).toISOString() : new Date().toISOString(),
    endDate: data.endDate ? new Date(data.endDate).toISOString() : new Date().toISOString(),
    fileStorageKey: data.fileStorageKey ? String(data.fileStorageKey) : undefined,
    contractType: data.contractType != null ? Number(data.contractType) : 0,
    incomeExpenseType: data.incomeExpenseType != null ? Number(data.incomeExpenseType) : 0,
    signDate: data.signDate ? new Date(data.signDate).toISOString() : undefined,
    note: data.note ? String(data.note) : undefined,
    description: data.description ? String(data.description) : undefined,
    orderId: data.orderId ? String(data.orderId) : undefined,
    customerId: data.customerId ? String(data.customerId) : undefined,
    departmentId: data.departmentId ? String(data.departmentId) : undefined,
    businessManager: data.businessManager ? String(data.businessManager) : undefined,
    responsibleProject: data.responsibleProject ? String(data.responsibleProject) : undefined,
    inputCustomer: data.inputCustomer ? String(data.inputCustomer) : undefined,
    nextPaymentReminder: Boolean(data.nextPaymentReminder),
    contractExpiryReminder: Boolean(data.contractExpiryReminder),
    singleDoubleProfit: data.singleDoubleProfit != null ? Number(data.singleDoubleProfit) : undefined,
    invoicingInformation: data.invoicingInformation ? String(data.invoicingInformation) : undefined,
    paymentStatus: data.paymentStatus != null ? Number(data.paymentStatus) : undefined,
    warrantyPeriod: data.warrantyPeriod ? String(data.warrantyPeriod) : undefined,
    isInstallmentPayment: data.isInstallmentPayment === 1 || data.isInstallmentPayment === true,
    accumulatedAmount: data.accumulatedAmount != null ? Number(data.accumulatedAmount) : undefined,
  };
  try {
    if (isEdit.value && id.value) {
      await updateContract(id.value, payload);
      message.success($t('common.success'));
    } else {
      await createContract(payload);
      message.success($t('common.success'));
    }
    router.push('/contract/list');
  } catch (e) {
    message.error((e as Error)?.message ?? $t('common.error'));
  }
}

function goBack() {
  router.push('/contract/list');
}
</script>

<template>
  <Page auto-content-height>
    <div class="px-4 py-4">
      <div class="mb-4 flex items-center gap-2">
        <Button type="text" class="inline-flex items-center gap-1" @click="goBack">
          <ArrowLeft class="size-5" />
          {{ $t('common.back') }}
        </Button>
      </div>

      <Card :title="isEdit ? $t('contract.edit') : $t('contract.create')" class="mb-4">
        <Form />
        <div class="mt-4 flex gap-2">
          <Button type="primary" @click="onSubmit">
            {{ $t('common.confirm') }}
          </Button>
          <Button @click="goBack">
            {{ $t('common.cancel') }}
          </Button>
        </div>
      </Card>
    </div>
  </Page>
</template>
