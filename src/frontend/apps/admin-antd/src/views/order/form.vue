<script lang="ts" setup>
import { computed, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';
import { ArrowLeft } from '@vben/icons';

import { Button, DatePicker, Input, InputNumber, message, Select, Table } from 'ant-design-vue';

import { createOrder, getOrder, updateOrder } from '#/api/system/order';
import { getContractList } from '#/api/system/contract';
import { getCustomerSearch } from '#/api/system/customer';
import { getProductList } from '#/api/system/product';
import { getProjectList } from '#/api/system/project';
import { getUserList } from '#/api/system/user';
import { $t } from '#/locales';
import { OrderTypeEnum } from '#/api/system/order';

const route = useRoute();
const router = useRouter();

const id = computed(() => route.params.id as string | undefined);
const isNew = computed(() => !id.value);

const loading = ref(false);
const submitting = ref(false);
const customerOptions = ref<{ label: string; value: string; name: string }[]>([]);
const userOptions = ref<{ label: string; value: string }[]>([]);
const contractOptions = ref<{ label: string; value: string }[]>([]);
const projectOptions = ref<{ label: string; value: string }[]>([]);
const productOptions = ref<{ label: string; value: string; name: string; model: string; unit: string }[]>([]);

const form = ref({
  customerId: '',
  customerName: '',
  projectId: '' as string,
  contractId: '' as string,
  orderNumber: '',
  type: OrderTypeEnum.Sales as number,
  status: 1,
  amount: 0,
  remark: '',
  ownerId: '',
  ownerName: '',
  receiverName: '',
  receiverPhone: '',
  receiverAddress: '',
  payDate: '' as string,
  deliveryDate: '' as string,
  items: [] as {
    productId: string;
    productName: string;
    model: string;
    number: string;
    qty: number;
    unit: string;
    price: number;
    amount: number;
    remark: string;
  }[],
});

const itemColumns = [
  { title: () => $t('order.product'), dataIndex: 'productId', width: 180, key: 'productId' },
  { title: () => $t('order.productName'), dataIndex: 'productName', width: 140, key: 'productName' },
  { title: () => $t('order.model'), dataIndex: 'model', width: 100, key: 'model' },
  { title: () => $t('order.number'), dataIndex: 'number', width: 90, key: 'number' },
  { title: () => $t('order.qty'), dataIndex: 'qty', width: 70, key: 'qty' },
  { title: () => $t('order.unit'), dataIndex: 'unit', width: 60, key: 'unit' },
  { title: () => $t('order.price'), dataIndex: 'price', width: 90, key: 'price' },
  { title: () => $t('order.itemAmount'), dataIndex: 'amount', width: 90, key: 'amount' },
  { title: () => $t('order.remark'), dataIndex: 'remark', key: 'remark', ellipsis: true },
  { title: () => $t('order.operation'), key: 'action', width: 80, fixed: 'right' as const },
];

function addItem() {
  form.value.items.push({
    productId: '',
    productName: '',
    model: '',
    number: '',
    qty: 1,
    unit: '台',
    price: 0,
    amount: 0,
    remark: '',
  });
}

function onProductSelect(val: string, index: number) {
  const opt = productOptions.value.find((o) => o.value === val);
  const item = form.value.items[index];
  if (opt && item) {
    item.productName = opt.name;
    item.model = opt.model;
    item.unit = opt.unit;
  }
}

function removeItem(index: number) {
  form.value.items.splice(index, 1);
}

function onCustomerSearch(keyword: string) {
  if (!keyword || keyword.length < 1) {
    customerOptions.value = [];
    return;
  }
  getCustomerSearch({ keyword, pageIndex: 1, pageSize: 20 }).then((res) => {
    customerOptions.value = (res.items ?? []).map((x) => ({
      label: `${x.fullName} ${x.mainContactPhone || ''}`.trim(),
      value: x.id,
      name: x.fullName,
    }));
  });
}

function recalcItemAmount(index: number) {
  const item = form.value.items[index];
  if (item) {
    item.amount = Number((item.qty * item.price).toFixed(2));
  }
  form.value.amount = form.value.items.reduce((sum, i) => sum + (i?.amount ?? 0), 0);
}

async function loadDetail() {
  if (!id.value) return;
  loading.value = true;
  try {
    const data = await getOrder(id.value);
    form.value = {
      customerId: data.customerId,
      customerName: data.customerName,
      projectId: data.projectId ?? '',
      contractId: data.contractId ?? '',
      orderNumber: data.orderNumber,
      type: data.type,
      status: data.status,
      amount: data.amount,
      remark: data.remark ?? '',
      ownerId: data.ownerId,
      ownerName: data.ownerName ?? '',
      receiverName: data.receiverName ?? '',
      receiverPhone: data.receiverPhone ?? '',
      receiverAddress: data.receiverAddress ?? '',
      payDate: data.payDate ? data.payDate.slice(0, 10) : '',
      deliveryDate: data.deliveryDate ? data.deliveryDate.slice(0, 10) : '',
      items: (data.items ?? []).map((i) => ({
        productId: i.productId ?? '',
        productName: i.productName,
        model: i.model ?? '',
        number: i.number ?? '',
        qty: i.qty,
        unit: i.unit ?? '',
        price: Number(i.price),
        amount: Number(i.amount),
        remark: i.remark ?? '',
      })),
    };
  } finally {
    loading.value = false;
  }
}

onMounted(() => {
  getUserList({ pageIndex: 1, pageSize: 500 }).then((res) => {
    userOptions.value = (res.items ?? []).map((x) => ({
      label: x.realName || x.name || x.userId,
      value: String(x.userId),
    }));
  });
  getContractList({ pageIndex: 1, pageSize: 500 }).then((res) => {
    contractOptions.value = (res.items ?? []).map((x) => ({
      label: `${x.code} - ${x.title}`,
      value: x.id,
    }));
  });
  getProjectList({ pageIndex: 1, pageSize: 500 }).then((res) => {
    projectOptions.value = (res.items ?? []).map((x) => ({
      label: x.name || x.id,
      value: x.id,
    }));
  });
  getProductList({ pageIndex: 1, pageSize: 1000 }).then((res) => {
    productOptions.value = (res.items ?? []).map((x) => ({
      label: `${x.code} - ${x.name}`,
      value: x.id,
      name: x.name,
      model: x.model,
      unit: x.unit,
    }));
  });
  if (!isNew.value) loadDetail();
});

function toIsoDate(v: string): string {
  if (!v) return new Date().toISOString();
  const d = new Date(v);
  return isNaN(d.getTime()) ? new Date().toISOString() : d.toISOString();
}

async function onSubmit() {
  if (!form.value.customerId || !form.value.orderNumber || !form.value.ownerId) {
    message.warning($t('ui.formRules.required', [$t('order.customer')]));
    return;
  }
  if (!form.value.projectId) {
    message.warning($t('ui.formRules.required', [$t('order.project')]));
    return;
  }
  if (!form.value.contractId) {
    message.warning($t('ui.formRules.required', [$t('order.contract')]));
    return;
  }
  if (!form.value.payDate || !form.value.deliveryDate) {
    message.warning($t('ui.formRules.required', [$t('order.payDate')]));
    return;
  }
  if (!form.value.items.length || form.value.items.some((i) => !i.productId)) {
    message.warning($t('ui.formRules.required', [$t('order.items')]));
    return;
  }
  submitting.value = true;
  try {
    const payload = {
      customerId: form.value.customerId,
      customerName: form.value.customerName,
      projectId: form.value.projectId,
      contractId: form.value.contractId,
      orderNumber: form.value.orderNumber,
      type: form.value.type,
      status: form.value.status,
      amount: form.value.amount,
      remark: form.value.remark ?? '',
      ownerId: form.value.ownerId,
      ownerName: form.value.ownerName ?? '',
      receiverName: form.value.receiverName ?? '',
      receiverPhone: form.value.receiverPhone ?? '',
      receiverAddress: form.value.receiverAddress ?? '',
      payDate: toIsoDate(form.value.payDate),
      deliveryDate: toIsoDate(form.value.deliveryDate),
      items: form.value.items.map((i) => ({
        productId: i.productId,
        productName: i.productName,
        model: i.model ?? '',
        number: i.number ?? '',
        qty: i.qty,
        unit: i.unit ?? '',
        price: i.price,
        amount: i.amount,
        remark: i.remark ?? '',
      })),
    };
    if (isNew.value) {
      await createOrder(payload);
      message.success($t('common.success'));
      router.push('/order/list');
    } else {
      await updateOrder({ ...payload, id: id.value });
      message.success($t('common.success'));
      router.push('/order/list');
    }
  } catch (e) {
    message.error(String(e));
  } finally {
    submitting.value = false;
  }
}

function goBack() {
  router.push('/order/list');
}
</script>

<template>
  <Page auto-content-height :loading="loading">
    <div class="p-4">
      <div class="mb-4 flex items-center gap-2">
        <Button @click="goBack">
          <ArrowLeft class="size-4" />
        </Button>
        <span class="text-lg font-medium text-foreground">{{ isNew ? $t('order.create') : $t('order.edit') }}</span>
      </div>

      <div class="grid gap-4 md:grid-cols-2">
        <div>
          <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.customer') }} *</label>
          <Select
            v-model:value="form.customerId"
            allow-clear
            :filter-option="() => true"
            placeholder="输入关键词搜索客户"
            show-search
            class="w-full"
            :options="customerOptions"
            :field-names="{ label: 'label', value: 'value' }"
            @search="onCustomerSearch"
            @select="(_: unknown, opt: unknown) => { form.customerName = (opt as { name?: string })?.name ?? ''; }"
          />
        </div>
        <div>
          <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.project') }} *</label>
          <Select
            v-model:value="form.projectId"
            allow-clear
            class="w-full"
            :options="projectOptions"
            :field-names="{ label: 'label', value: 'value' }"
            placeholder="请选择项目"
          />
        </div>
        <div>
          <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.contract') }} *</label>
          <Select
            v-model:value="form.contractId"
            allow-clear
            class="w-full"
            :options="contractOptions"
            :field-names="{ label: 'label', value: 'value' }"
            placeholder="请选择合同"
          />
        </div>
        <div>
          <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.orderNumber') }} *</label>
          <Input v-model:value="form.orderNumber" placeholder="" />
        </div>
        <div>
          <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.type') }}</label>
          <Select
            v-model:value="form.type"
            class="w-full"
            :options="[
              { label: $t('order.typeSales'), value: 0 },
              { label: $t('order.typeAfterSales'), value: 1 },
              { label: $t('order.typeSample'), value: 2 },
              { label: $t('order.typeGeneralTest'), value: 3 },
            ]"
          />
        </div>
        <div>
          <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.status') }}</label>
          <Select
            v-model:value="form.status"
            class="w-full"
            :options="[
              { label: $t('order.statusPendingAudit'), value: 1 },
              { label: $t('order.statusOrdered'), value: 2 },
              { label: $t('order.statusCompleted'), value: 3 },
              { label: $t('order.statusRejected'), value: 4 },
              { label: $t('order.statusUnpaid'), value: 5 },
            ]"
          />
        </div>
        <div>
          <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.owner') }} *</label>
          <Select
            v-model:value="form.ownerId"
            allow-clear
            class="w-full"
            :options="userOptions"
            :field-names="{ label: 'label', value: 'value' }"
            @select="(_: unknown, opt: unknown) => { form.ownerName = (opt as { label?: string })?.label ?? ''; }"
          />
        </div>
        <div>
          <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.amount') }}</label>
          <InputNumber v-model:value="form.amount" class="w-full" :min="0" :precision="2" />
        </div>
        <div class="md:col-span-2">
          <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.remark') }}</label>
          <Input.TextArea v-model:value="form.remark" :rows="2" placeholder="" />
        </div>
        <div>
          <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.receiverName') }}</label>
          <Input v-model:value="form.receiverName" />
        </div>
        <div>
          <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.receiverPhone') }}</label>
          <Input v-model:value="form.receiverPhone" />
        </div>
        <div class="md:col-span-2">
          <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.receiverAddress') }}</label>
          <Input v-model:value="form.receiverAddress" />
        </div>
        <div>
          <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.payDate') }} *</label>
          <DatePicker
            v-model:value="form.payDate"
            value-format="YYYY-MM-DD"
            class="w-full"
            style="width: 100%"
            placeholder="选择付款日期"
          />
        </div>
        <div>
          <label class="mb-1 block text-sm font-medium text-foreground">{{ $t('order.deliveryDate') }} *</label>
          <DatePicker
            v-model:value="form.deliveryDate"
            value-format="YYYY-MM-DD"
            class="w-full"
            style="width: 100%"
            placeholder="选择发货日期"
          />
        </div>
      </div>

      <div class="mt-4">
        <div class="mb-2 flex items-center justify-between">
          <span class="text-sm font-medium text-foreground">{{ $t('order.items') }}</span>
          <Button type="primary" size="small" @click="addItem">{{ $t('order.addItem') }}</Button>
        </div>
        <Table
          :columns="itemColumns"
          :data-source="form.items"
          :pagination="false"
          :row-key="(_, i) => String(i)"
          size="small"
          bordered
        >
          <template #bodyCell="{ column, record, index }">
            <template v-if="column.key === 'productId'">
              <Select
                v-model:value="record.productId"
                allow-clear
                show-search
                class="w-full"
                :options="productOptions"
                :field-names="{ label: 'label', value: 'value' }"
                placeholder="选择产品"
                @select="(value: unknown) => onProductSelect(String(value), index)"
              />
            </template>
            <template v-else-if="column.key === 'productName'">
              <Input v-model:value="record.productName" size="small" />
            </template>
            <template v-else-if="column.key === 'model'">
              <Input v-model:value="record.model" size="small" />
            </template>
            <template v-else-if="column.key === 'number'">
              <Input v-model:value="record.number" size="small" />
            </template>
            <template v-else-if="column.key === 'qty'">
              <InputNumber
                v-model:value="record.qty"
                size="small"
                :min="1"
                class="w-full"
                @change="() => recalcItemAmount(index)"
              />
            </template>
            <template v-else-if="column.key === 'unit'">
              <Input v-model:value="record.unit" size="small" />
            </template>
            <template v-else-if="column.key === 'price'">
              <InputNumber
                v-model:value="record.price"
                size="small"
                :min="0"
                :precision="2"
                class="w-full"
                @change="() => recalcItemAmount(index)"
              />
            </template>
            <template v-else-if="column.key === 'amount'">
              {{ record.amount }}
            </template>
            <template v-else-if="column.key === 'remark'">
              <Input v-model:value="record.remark" size="small" />
            </template>
            <template v-else-if="column.key === 'action'">
              <Button type="link" size="small" danger @click="removeItem(index)">{{ $t('order.delete') }}</Button>
            </template>
          </template>
        </Table>
      </div>

      <div class="mt-4 flex gap-2">
        <Button type="primary" :loading="submitting" @click="onSubmit">{{ $t('common.confirm') }}</Button>
        <Button @click="goBack">{{ $t('common.cancel') }}</Button>
      </div>
    </div>
  </Page>
</template>
