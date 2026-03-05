import { requestClient } from '#/api/request';

export namespace ContractTypeOptionApi {
  export interface ContractTypeOptionItem {
    id: string;
    name: string;
    typeValue: number;
    orderSigningCompanyOptionDisplay: boolean;
    sortOrder: number;
  }
}

function unwrap<T>(res: T | { data?: T }): T[] {
  if (Array.isArray(res)) return res;
  const data = (res as { data?: T })?.data;
  return Array.isArray(data) ? data : [];
}

async function getContractTypeOptionList() {
  const res = await requestClient.get<
    ContractTypeOptionApi.ContractTypeOptionItem[] | { data?: ContractTypeOptionApi.ContractTypeOptionItem[] }
  >('/contract-type-options');
  return unwrap(res);
}

async function createContractTypeOption(data: {
  name: string;
  typeValue: number;
  orderSigningCompanyOptionDisplay?: boolean;
  sortOrder?: number;
}) {
  const result = await requestClient.post<{ id: string }>('/contract-type-options', {
    name: data.name,
    typeValue: data.typeValue,
    orderSigningCompanyOptionDisplay: data.orderSigningCompanyOptionDisplay ?? false,
    sortOrder: data.sortOrder ?? 0,
  });
  return (result as { data?: { id: string } })?.data ?? result;
}

async function updateContractTypeOption(
  id: string,
  data: { name: string; typeValue: number; orderSigningCompanyOptionDisplay: boolean; sortOrder: number },
) {
  return requestClient.put(`/contract-type-options/${id}`, data);
}

async function deleteContractTypeOption(id: string) {
  return requestClient.delete(`/contract-type-options/${id}`);
}

export {
  getContractTypeOptionList,
  createContractTypeOption,
  updateContractTypeOption,
  deleteContractTypeOption,
};
