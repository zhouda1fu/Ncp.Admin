import type { Recordable } from '@vben/types';

import { requestClient } from '#/api/request';

/** 车辆状态：0 可用 1 已禁用 */
export type VehicleStatus = 0 | 1;
/** 预订状态：0 已预订 1 已取消 2 已完成 */
export type VehicleBookingStatus = 0 | 1 | 2;

export namespace VehicleApi {
  export interface VehicleItem {
    id: string;
    plateNumber: string;
    model: string;
    status: VehicleStatus;
    remark?: string;
    createdAt: string;
  }

  export interface BookingItem {
    id: string;
    vehicleId: string;
    plateNumber: string;
    model: string;
    bookerId: number | string;
    purpose: string;
    startAt: string;
    endAt: string;
    status: VehicleBookingStatus;
    createdAt: string;
  }
}

async function getVehicleList(params: Recordable<any>) {
  const res = await requestClient.get<{ items: VehicleApi.VehicleItem[]; total: number }>(
    '/vehicles',
    { params },
  );
  return res;
}

async function getVehicle(id: string) {
  return requestClient.get<VehicleApi.VehicleItem>(`/vehicles/${id}`);
}

async function createVehicle(data: { plateNumber: string; model: string; remark?: string }) {
  return requestClient.post<{ id: string }>('/vehicles', data);
}

async function updateVehicle(
  id: string,
  data: { plateNumber: string; model: string; remark?: string },
) {
  return requestClient.put(`/vehicles/${id}`, data);
}

async function getVehicleBookingList(params: Recordable<any>) {
  const res = await requestClient.get<{
    items: VehicleApi.BookingItem[];
    total: number;
  }>('/vehicle-bookings', { params });
  return res;
}

async function createVehicleBooking(data: {
  vehicleId: string;
  purpose: string;
  startAt: string;
  endAt: string;
}) {
  return requestClient.post<{ id: string }>('/vehicle-bookings', data);
}

async function cancelVehicleBooking(id: string) {
  return requestClient.post(`/vehicle-bookings/${id}/cancel`);
}

async function completeVehicleBooking(id: string) {
  return requestClient.post(`/vehicle-bookings/${id}/complete`);
}

export {
  getVehicleList,
  getVehicle,
  createVehicle,
  updateVehicle,
  getVehicleBookingList,
  createVehicleBooking,
  cancelVehicleBooking,
  completeVehicleBooking,
};
