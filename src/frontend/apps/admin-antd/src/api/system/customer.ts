import type { Recordable } from '@vben/types';

import { requestClient } from '#/api/request';

export namespace CustomerApi {
  export interface CustomerItem {
    id: string;
    ownerId?: string;
    ownerDeptId?: string;
    ownerDeptName?: string;
    customerSourceId: string;
    customerSourceName: string;
    fullName: string;
    shortName?: string;
    /** 客户状态枚举值：0 跟进中-暂不明了 1 跟进中-有意向 2 合作中客户 3 曾合作客户 */
    status?: number;
    /** 公司性质枚举值：0 个体 1 民营 2 国企或央企 3 外企 4 其他 5 终端客户 6 运营商 */
    nature?: number;
    provinceCode?: string;
    cityCode?: string;
    districtCode?: string;
    provinceName?: string;
    cityName?: string;
    districtName?: string;
    phoneProvinceCode?: string;
    phoneCityCode?: string;
    phoneDistrictCode?: string;
    phoneProvinceName?: string;
    phoneCityName?: string;
    phoneDistrictName?: string;
    consultationContent?: string;
    contactQq?: string;
    contactWechat?: string;
    coverRegion?: string;
    registerAddress?: string;
    /** 员工数量 */
    employeeCount?: number;
    /** 营业执照（路径或 URL） */
    businessLicense?: string;
    mainContactName?: string;
    mainContactPhone?: string;
    wechatStatus?: string;
    remark?: string;
    isKeyAccount: boolean;
    isHidden: boolean;
    combineFlag: boolean;
    isInSea: boolean;
    releasedToSeaAt?: string;
    creatorId: string;
    creatorName?: string;
    ownerName?: string;
    claimedAt?: string;
    /** 公海作废标记 */
    isVoided?: boolean;
    createdAt: string;
    contactCount: number;
    industryIds: string[];
  }

  export interface CustomerContactItem {
    id: string;
    name: string;
    contactType?: string;
    /** 性别（必填） */
    gender: number;
    /** 生日 ISO 字符串（必填） */
    birthday: string;
    position?: string;
    mobile?: string;
    phone?: string;
    email?: string;
    qq?: string;
    wechat?: string;
    isWechatAdded?: boolean;
    isPrimary: boolean;
  }

  export interface CustomerContactRecordItem {
    id: string;
    recordAt: string;
    /** 1电话 2出差 3微信 4其他 */
    recordType: number;
    title?: string;
    content: string;
    nextVisitAt?: string;
    /** 0待选择 1有效联系 2无效联系 */
    status: number;
    customerContactIds?: string[];
    ownerId?: string;
    ownerName?: string;
    ownerDeptId?: string;
    ownerDeptName?: string;
    creatorId?: string;
    /** 录入人姓名（接口若返回则展示） */
    creatorName?: string;
    createdAt?: string;
    modifierId?: string;
    modifiedAt?: string;
    remark?: string;
    reminderIntervalDays?: number;
    reminderCount?: number;
    filePath?: string;
    customerAddress?: string;
    visitAddress?: string;
    /** @deprecated 使用 status */
    statusId?: number;
    /** @deprecated 使用 ownerName */
    recorderName?: string;
  }

  export interface CustomerDetail extends CustomerItem {
    contacts: CustomerContactItem[];
    contactRecords: CustomerContactRecordItem[];
  }

  export interface CustomerSearchItem {
    id: string;
    fullName: string;
    shortName?: string;
    mainContactPhone?: string;
  }
}

export interface GetCustomerListParams extends Recordable<any> {
  pageIndex?: number;
  pageSize?: number;
  fullName?: string;
  ownerId?: string;
}

async function getCustomerList(params: GetCustomerListParams) {
  const res = await requestClient.get<{
    items: CustomerApi.CustomerItem[];
    total: number;
  }>('/customers', { params });
  return res;
}

async function getCustomer(id: string) {
  return requestClient.get<CustomerApi.CustomerDetail>(`/customers/${id}`);
}

async function createCustomer(
  data: Recordable<any>,
  options?: { idempotencyKey?: string },
) {
  const headers: Record<string, string> = {};
  if (options?.idempotencyKey)
    headers['Idempotency-Key'] = options.idempotencyKey;
  return requestClient.post<{ id: string }>('/customers', data, {
    ...(Object.keys(headers).length ? { headers } : {}),
  });
}

/** 公海录入专用：创建公海客户（无需客户名称、无负责人） */
async function createSeaCustomer(data: Recordable<any>) {
  return requestClient.post<{ id: string }>('/customers/sea', data);
}

/** 公海录入专用：检查联系方式是否重复（电话/QQ/微信） */
async function checkSeaCustomerDuplicateContacts(data: {
  mainContactPhone?: string;
  contactQq?: string;
  contactWechat?: string;
}) {
  return requestClient.post<{
    items: Array<{
      customerId: string;
      customerName: string;
      customerSourceName: string;
      ownerName: string;
      duplicatePhones: string[];
      duplicateQqs: string[];
      duplicateWechats: string[];
    }>;
  }>('/customers/sea/check-duplicate-contacts', {
    mainContactPhone: data.mainContactPhone ?? '',
    contactQq: data.contactQq ?? '',
    contactWechat: data.contactWechat ?? '',
  });
}

async function updateCustomer(id: string, data: Recordable<any>) {
  return requestClient.put(`/customers/${id}`, data);
}

async function getCustomerSearch(params: Recordable<any>) {
  const res = await requestClient.get<{
    items: CustomerApi.CustomerSearchItem[];
    total: number;
  }>('/customers/search', { params });
  return res;
}

async function getCustomerShares(id: string) {
  return requestClient.get<{ sharedToUserIds: string[] }>(`/customers/${id}/shares`);
}

async function shareCustomer(id: string, sharedToUserIds: string[]) {
  return requestClient.post(`/customers/${id}/share`, { id, sharedToUserIds });
}

async function unshareCustomer(id: string, sharedToUserIds: string[]) {
  return requestClient.delete(`/customers/${id}/share`, { data: { id, sharedToUserIds } as any });
}

async function releaseCustomerToSea(id: string) {
  return requestClient.post(`/customers/${id}/release-to-sea`, {});
}

async function claimCustomerFromSea(id: string) {
  return requestClient.post(`/customers/${id}/claim`, {});
}

async function updateSeaCustomer(id: string, data: Recordable<any>) {
  return requestClient.put(`/customers/${id}/sea`, data);
}

async function updateSeaCustomerConsultation(id: string, consultationContent: string) {
  return requestClient.put(`/customers/${id}/sea/consultation`, {
    id,
    consultationContent,
  });
}

export interface VoidCustomerWorkflowResponse {
  workflowInstanceId: string;
  title: string;
}

async function voidCustomer(id: string, routingRoleId?: string) {
  const body = routingRoleId ? { routingRoleId } : {};
  return requestClient.post<VoidCustomerWorkflowResponse>(`/customers/${id}/void`, body);
}

async function deleteCustomer(id: string) {
  return requestClient.delete(`/customers/${id}`);
}

async function addCustomerContact(customerId: string, data: Recordable<any>) {
  return requestClient.post<{ id: string }>(`/customers/${customerId}/contacts`, data);
}

async function updateCustomerContact(
  customerId: string,
  contactId: string,
  data: Recordable<any>,
) {
  return requestClient.put(`/customers/${customerId}/contacts/${contactId}`, data);
}

async function removeCustomerContact(customerId: string, contactId: string) {
  return requestClient.delete(`/customers/${customerId}/contacts/${contactId}`);
}

async function addCustomerContactRecord(customerId: string, data: Recordable<any>) {
  return requestClient.post<{ id: string }>(`/customers/${customerId}/contact-records`, data);
}

async function updateCustomerContactRecord(
  customerId: string,
  recordId: string,
  data: Recordable<any>,
) {
  return requestClient.put(`/customers/${customerId}/contact-records/${recordId}`, data);
}

async function removeCustomerContactRecord(customerId: string, recordId: string) {
  return requestClient.delete(`/customers/${customerId}/contact-records/${recordId}`);
}

export {
  getCustomerList,
  getCustomer,
  createCustomer,
  createSeaCustomer,
  checkSeaCustomerDuplicateContacts,
  updateCustomer,
  updateSeaCustomer,
  updateSeaCustomerConsultation,
  getCustomerSearch,
  getCustomerShares,
  shareCustomer,
  unshareCustomer,
  releaseCustomerToSea,
  claimCustomerFromSea,
  voidCustomer,
  deleteCustomer,
  addCustomerContact,
  updateCustomerContact,
  removeCustomerContact,
  addCustomerContactRecord,
  updateCustomerContactRecord,
  removeCustomerContactRecord,
};
