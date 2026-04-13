import type { Recordable } from '@vben/types';

import { requestClient } from '#/api/request';

export namespace CustomerSeaRegionAssignApi {
  export interface AssignUserListItem {
    userId: string;
    deptName: string;
    roleNames: string[];
    name: string;
  }

  export interface AssignUserAuditListItem {
    id: string;
    operatorUserName: string;
    createdAt: string;
    addedRegionNames: string[];
    removedRegionNames: string[];
  }

  export interface AuthorizedRegionsSummary {
    authorizedUserCount: number;
    isTargetVisible: boolean;
    authorizedRegionIds: Array<number | string>;
    authorizedRegionNames: string[];
    targetRegionIds: Array<number | string>;
    targetRegionNames: string[];
    missingFromTargetRegionIds: Array<number | string>;
    missingFromTargetRegionNames: string[];
  }
}

export namespace CustomerSeaRegionAssignApiTypes {
  export interface AssignUsersParams extends Recordable<any> {
    pageIndex?: number;
    pageSize?: number;
    keyword?: string;
  }
}

export async function getAssignUsers(params: CustomerSeaRegionAssignApiTypes.AssignUsersParams) {
  return requestClient.get<{ items: CustomerSeaRegionAssignApi.AssignUserListItem[]; total: number }>(
    '/customer-sea-region-assign/users',
    { params },
  );
}

export async function getUserRegions(userId: string) {
  return requestClient.get<Array<number | string>>(`/customer-sea-region-assign/users/${userId}/regions`);
}

export async function saveUserRegions(userId: string, selectedRegionIds: Array<number | string>) {
  return requestClient.put<boolean>(`/customer-sea-region-assign/users/${userId}/regions`, {
    selectedRegionIds,
  });
}

export async function getUserAudits(userId: string, params: { pageIndex: number; pageSize: number }) {
  return requestClient.get<{
    items: CustomerSeaRegionAssignApi.AssignUserAuditListItem[];
    total: number;
  }>(`/customer-sea-region-assign/users/${userId}/audits`, {
    params,
  });
}

export async function getAuthorizedRegions(userId: string) {
  return requestClient.get<CustomerSeaRegionAssignApi.AuthorizedRegionsSummary>(
    `/customer-sea-region-assign/users/${userId}/authorized-regions`,
  );
}

