import type { Recordable } from '@vben/types';

import { requestClient } from '#/api/request';

export namespace CustomerApi {
  export interface CustomerItem {
    id: string;
    ownerId?: string;
    customerSourceId: string;
    customerSourceName: string;
    fullName: string;
    shortName?: string;
    nature?: string;
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
    createdAt: string;
    contactCount: number;
    industryIds: string[];
  }

  export interface CustomerContactItem {
    id: string;
    name: string;
    contactType?: string;
    gender?: number;
    birthday?: string;
    position?: string;
    mobile?: string;
    phone?: string;
    email?: string;
    isPrimary: boolean;
  }

  export interface CustomerDetail extends CustomerItem {
    contacts: CustomerContactItem[];
  }

  export interface CustomerSearchItem {
    id: string;
    fullName: string;
    shortName?: string;
    mainContactPhone?: string;
  }
}

async function getCustomerList(params: Recordable<any>) {
  const res = await requestClient.get<{
    items: CustomerApi.CustomerItem[];
    total: number;
  }>('/customers', { params });
  return res;
}

async function getCustomer(id: string) {
  return requestClient.get<CustomerApi.CustomerDetail>(`/customers/${id}`);
}

async function createCustomer(data: Recordable<any>) {
  return requestClient.post<{ id: string }>('/customers', data);
}

/** 公海录入专用：创建公海客户（无需客户名称、无负责人） */
async function createSeaCustomer(data: Recordable<any>) {
  return requestClient.post<{ id: string }>('/customers/sea', data);
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

async function releaseCustomerToSea(id: string) {
  return requestClient.post(`/customers/${id}/release-to-sea`);
}

async function claimCustomerFromSea(id: string) {
  return requestClient.post(`/customers/${id}/claim`);
}

async function updateSeaCustomer(id: string, data: Recordable<any>) {
  return requestClient.put(`/customers/${id}/sea`, data);
}

async function voidCustomer(id: string) {
  return requestClient.post(`/customers/${id}/void`);
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

export {
  getCustomerList,
  getCustomer,
  createCustomer,
  createSeaCustomer,
  updateCustomer,
  updateSeaCustomer,
  getCustomerSearch,
  releaseCustomerToSea,
  claimCustomerFromSea,
  voidCustomer,
  deleteCustomer,
  addCustomerContact,
  updateCustomerContact,
  removeCustomerContact,
};
