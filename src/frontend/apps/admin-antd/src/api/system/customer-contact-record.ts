import type { Recordable } from '@vben/types';

import { requestClient } from '#/api/request';

export namespace CustomerContactRecordApi {
  export interface ListItem {
    id: string;
    customerId: string;
    customerName: string;
    industryNames: string[];
    ownerId: string;
    ownerName: string;
    ownerDeptName: string;
    regionName: string;
    /** 1电话 2出差 3微信 4其他 */
    recordType: number;
    recordAt: string;
    nextVisitAt?: string;
    /** 0待选择 1有效 2无效 */
    status: number;
    title?: string;
    content: string;
    customerContactIds: string[];
    remark?: string;
    reminderIntervalDays: number;
    reminderCount: number;
    filePath?: string;
    customerAddress?: string;
    visitAddress?: string;
    creatorId: string;
    createdAt: string;
    modifierId: string;
    modifiedAt: string;
  }

  export interface ListParams extends Recordable<any> {
    pageIndex?: number;
    pageSize?: number;
    keyword?: string;
    recordTypeId?: number;
    statusId?: number;
    ownerId?: string;
    recordAtFrom?: string;
    recordAtTo?: string;
    nextVisitAtFrom?: string;
    nextVisitAtTo?: string;
  }
}

export async function getCustomerContactRecordList(params: CustomerContactRecordApi.ListParams) {
  return requestClient.get<{ items: CustomerContactRecordApi.ListItem[]; total: number }>(
    '/customer-contact-records',
    { params },
  );
}
