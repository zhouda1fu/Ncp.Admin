import { requestClient } from '#/api/request';

export namespace IncomeExpenseTypeOptionApi {
  export interface IncomeExpenseTypeOptionItem {
    id: string;
    name: string;
    typeValue: number;
    sortOrder: number;
  }
}

function unwrap<T>(res: T | { data?: T }): T[] {
  if (Array.isArray(res)) return res;
  const data = (res as { data?: T })?.data;
  return Array.isArray(data) ? data : [];
}

async function getIncomeExpenseTypeOptionList() {
  const res = await requestClient.get<
    IncomeExpenseTypeOptionApi.IncomeExpenseTypeOptionItem[] | { data?: IncomeExpenseTypeOptionApi.IncomeExpenseTypeOptionItem[] }
  >('/income-expense-type-options');
  return unwrap(res);
}

async function createIncomeExpenseTypeOption(data: {
  name: string;
  typeValue: number;
  sortOrder?: number;
}) {
  const result = await requestClient.post<{ id: string }>('/income-expense-type-options', {
    name: data.name,
    typeValue: data.typeValue,
    sortOrder: data.sortOrder ?? 0,
  });
  return (result as { data?: { id: string } })?.data ?? result;
}

async function updateIncomeExpenseTypeOption(
  id: string,
  data: { name: string; typeValue: number; sortOrder: number },
) {
  return requestClient.put(`/income-expense-type-options/${id}`, data);
}

async function deleteIncomeExpenseTypeOption(id: string) {
  return requestClient.delete(`/income-expense-type-options/${id}`);
}

export {
  getIncomeExpenseTypeOptionList,
  createIncomeExpenseTypeOption,
  updateIncomeExpenseTypeOption,
  deleteIncomeExpenseTypeOption,
};
