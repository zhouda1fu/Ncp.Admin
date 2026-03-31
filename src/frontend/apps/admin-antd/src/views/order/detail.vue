<script lang="ts" setup>
import { onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { useAccessStore } from '@vben/stores';
import { Page } from '@vben/common-ui';
import { ArrowLeft } from '@vben/icons';

import { Button, Descriptions, message, Modal, Table } from 'ant-design-vue';

import { getOrder, submitOrder } from '#/api/system/order';
import { PermissionCodes } from '#/constants/permission-codes';
import type { OrderApi } from '#/api/system/order';
import { OrderStatusEnum } from '#/api/system/order';
import { $t } from '#/locales';
import { productCategoryNameInitial } from '#/utils/product-category-label';

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
  0: 'order.statusDraft',
  1: 'order.statusPendingAudit',
  2: 'order.statusOrdered',
  3: 'order.statusCompleted',
  4: 'order.statusRejected',
  5: 'order.statusUnpaid',
};

const paymentStatusMap: Record<number, string> = {
  0: 'order.paymentStatusFullPayment',
  1: 'order.paymentStatusPartialPayment',
  2: 'order.paymentStatusInstallmentUrgent',
  3: 'order.paymentStatusPendingConfirmation',
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

const accessStore = useAccessStore();
const canSubmitOrder = () =>
  accessStore.accessCodes?.includes(PermissionCodes.OrderSubmit) ?? false;
const hasPermission = (code: string) =>
  accessStore.accessCodes?.includes(code) ?? false;
const showSubmitApproval = () =>
  detail.value &&
  canSubmitOrder() &&
  (detail.value.status === OrderStatusEnum.Draft ||
    detail.value.status === OrderStatusEnum.Rejected);

function onSubmitApproval() {
  if (!detail.value) return;
  Modal.confirm({
    title: $t('order.submitApprovalConfirmTitle'),
    content: $t('order.submitApprovalConfirmContent'),
    onOk: async () => {
      await submitOrder(detail.value!.id);
      message.success($t('order.submitApprovalSuccess'));
      detail.value = await getOrder(id.value);
    },
  });
}

function goContractList() {
  router.push(`/contract/list?orderId=${id.value}`);
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
        <div class="flex items-center gap-2">
          <Button @click="goContractList">{{ $t('contract.list') }}</Button>
          <Button
            v-if="showSubmitApproval()"
            type="dashed"
            @click="onSubmitApproval"
          >
            {{ $t('order.submit') }}
          </Button>
          <Button
            v-if="showSubmitApproval()"
            type="primary"
            @click="onEdit"
          >
            {{ $t('order.edit') }}
          </Button>
        </div>
      </div>

      <Descriptions v-if="detail" bordered :column="2" size="small">
        <Descriptions.Item :label="$t('order.orderNumber')">{{ detail.orderNumber }}</Descriptions.Item>
        <Descriptions.Item :label="$t('order.customerName')">{{ detail.customerName }}</Descriptions.Item>
        <Descriptions.Item :label="$t('order.project')">{{ detail.projectId }}</Descriptions.Item>
        <Descriptions.Item v-if="hasPermission(PermissionCodes.OrderContractSelect)" :label="$t('order.contract')">{{ detail.contractId }}</Descriptions.Item>
        <Descriptions.Item :label="$t('order.type')">{{ $t(typeMap[detail.type] ?? '') }}</Descriptions.Item>
        <Descriptions.Item :label="$t('order.status')">{{ $t(statusMap[detail.status] ?? '') }}</Descriptions.Item>
        <Descriptions.Item :label="$t('order.amount')">{{ detail.amount }}</Descriptions.Item>
        <Descriptions.Item :label="$t('order.ownerName')">{{ detail.ownerName }}</Descriptions.Item>
        <Descriptions.Item :label="$t('order.dept')">{{ detail.deptName }}</Descriptions.Item>
        <Descriptions.Item :label="$t('order.projectContactName')">{{ detail.projectContactName }}</Descriptions.Item>
        <Descriptions.Item :label="$t('order.projectContactPhone')">{{ detail.projectContactPhone }}</Descriptions.Item>
        <Descriptions.Item :label="$t('order.warranty')">{{ detail.warranty }}</Descriptions.Item>
        <Descriptions.Item :label="$t('order.contractSigningCompany')">{{ detail.contractSigningCompany }}</Descriptions.Item>
        <Descriptions.Item :label="$t('order.contractTrustee')">{{ detail.contractTrustee }}</Descriptions.Item>
        <Descriptions.Item v-if="hasPermission(PermissionCodes.OrderNeedInvoice)" :label="$t('order.needInvoice')">{{ detail.needInvoice ? $t('order.yes') : $t('order.no') }}</Descriptions.Item>
        <Descriptions.Item :label="$t('order.installationFee')">{{ detail.installationFee }}</Descriptions.Item>
        <Descriptions.Item :label="$t('order.estimatedFreight')">{{ detail.estimatedFreight }}</Descriptions.Item>
        <Descriptions.Item v-if="hasPermission(PermissionCodes.OrderContractSelect)" :label="$t('order.selectContract')">{{ detail.selectedContractFileId === 1 ? $t('order.yes') : $t('order.no') }}</Descriptions.Item>
        <Descriptions.Item :label="$t('order.isShipped')">{{ detail.isShipped ? $t('order.yes') : $t('order.no') }}</Descriptions.Item>
        <Descriptions.Item :label="$t('order.paymentStatus')">{{ $t(paymentStatusMap[detail.paymentStatus] ?? '') }}</Descriptions.Item>
        <Descriptions.Item v-if="hasPermission(PermissionCodes.OrderContractNotCompanyTemplate)" :label="$t('order.contractNotCompanyTemplate')">{{ detail.contractNotCompanyTemplate ? $t('order.yes') : $t('order.no') }}</Descriptions.Item>
        <template v-if="detail.orderCategories?.length">
          <Descriptions.Item
            v-for="b in detail.orderCategories"
            :key="b.id"
            :label="$t('order.contractDiscountWithCategory', [productCategoryNameInitial(b.categoryName)])"
          >
            {{ b.discountPoints }}
          </Descriptions.Item>
        </template>
        <Descriptions.Item v-if="hasPermission(PermissionCodes.OrderContractAmount)" :label="$t('order.contractAmount')">{{ detail.contractAmount }}</Descriptions.Item>
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
