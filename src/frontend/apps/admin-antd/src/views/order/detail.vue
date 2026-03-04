<script lang="ts" setup>
import { onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';
import { ArrowLeft } from '@vben/icons';

import { Button, Descriptions, Table } from 'ant-design-vue';

import { getOrder } from '#/api/system/order';
import type { OrderApi } from '#/api/system/order';
import { $t } from '#/locales';

const route = useRoute();
const router = useRouter();
const id = ref(route.params.id as string);
const loading = ref(false);
const detail = ref<OrderApi.OrderDetail | null>(null);

const typeMap: Record<number, string> = {
  0: 'order.typeSales',
  1: 'order.typeAfterSales',
  2: 'order.typeSample',
  3: 'order.typeGeneralTest',
};
const statusMap: Record<number, string> = {
  1: 'order.statusPendingAudit',
  2: 'order.statusOrdered',
  3: 'order.statusCompleted',
  4: 'order.statusRejected',
  5: 'order.statusUnpaid',
};

const itemColumns = [
  { title: () => $t('order.product'), dataIndex: 'productId', key: 'productId', width: 120 },
  { title: () => $t('order.productName'), dataIndex: 'productName', key: 'productName' },
  { title: () => $t('order.model'), dataIndex: 'model', key: 'model' },
  { title: () => $t('order.number'), dataIndex: 'number', key: 'number' },
  { title: () => $t('order.qty'), dataIndex: 'qty', key: 'qty', width: 80 },
  { title: () => $t('order.unit'), dataIndex: 'unit', key: 'unit', width: 60 },
  { title: () => $t('order.price'), dataIndex: 'price', key: 'price', width: 100 },
  { title: () => $t('order.itemAmount'), dataIndex: 'amount', key: 'amount', width: 100 },
  { title: () => $t('order.remark'), dataIndex: 'remark', key: 'remark', ellipsis: true },
];

onMounted(async () => {
  loading.value = true;
  try {
    detail.value = await getOrder(id.value);
  } finally {
    loading.value = false;
  }
});

function goBack() {
  router.push('/order/list');
}

function onEdit() {
  router.push(`/order/${id.value}/edit`);
}
</script>

<template>
  <Page auto-content-height :loading="loading">
    <div class="p-4">
      <div class="mb-4 flex items-center justify-between">
        <div class="flex items-center gap-2">
          <Button @click="goBack">
            <ArrowLeft class="size-4" />
          </Button>
          <span class="text-lg font-medium">{{ $t('order.detail') }}</span>
        </div>
        <Button type="primary" @click="onEdit">{{ $t('order.edit') }}</Button>
      </div>

      <Descriptions v-if="detail" bordered :column="2" size="small">
        <Descriptions.Item :label="$t('order.orderNumber')">{{ detail.orderNumber }}</Descriptions.Item>
        <Descriptions.Item :label="$t('order.customerName')">{{ detail.customerName }}</Descriptions.Item>
        <Descriptions.Item :label="$t('order.project')">{{ detail.projectId }}</Descriptions.Item>
        <Descriptions.Item :label="$t('order.contract')">{{ detail.contractId }}</Descriptions.Item>
        <Descriptions.Item :label="$t('order.type')">{{ $t(typeMap[detail.type] ?? '') }}</Descriptions.Item>
        <Descriptions.Item :label="$t('order.status')">{{ $t(statusMap[detail.status] ?? '') }}</Descriptions.Item>
        <Descriptions.Item :label="$t('order.amount')">{{ detail.amount }}</Descriptions.Item>
        <Descriptions.Item :label="$t('order.ownerName')">{{ detail.ownerName }}</Descriptions.Item>
        <Descriptions.Item :label="$t('order.receiverName')">{{ detail.receiverName }}</Descriptions.Item>
        <Descriptions.Item :label="$t('order.receiverPhone')">{{ detail.receiverPhone }}</Descriptions.Item>
        <Descriptions.Item :label="$t('order.receiverAddress')" :span="2">{{ detail.receiverAddress }}</Descriptions.Item>
        <Descriptions.Item :label="$t('order.payDate')">{{ detail.payDate }}</Descriptions.Item>
        <Descriptions.Item :label="$t('order.deliveryDate')">{{ detail.deliveryDate }}</Descriptions.Item>
        <Descriptions.Item :label="$t('order.createdAt')">{{ detail.createdAt }}</Descriptions.Item>
        <Descriptions.Item :label="$t('order.remark')" :span="2">{{ detail.remark }}</Descriptions.Item>
      </Descriptions>

      <div v-if="detail?.items?.length" class="mt-4">
        <div class="mb-2 font-medium">{{ $t('order.items') }}</div>
        <Table
          :columns="itemColumns"
          :data-source="detail.items"
          :pagination="false"
          row-key="id"
          size="small"
          bordered
        />
      </div>
    </div>
  </Page>
</template>
