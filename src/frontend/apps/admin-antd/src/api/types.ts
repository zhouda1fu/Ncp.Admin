export interface PagedData<T> {
  items: T[];
  pageIndex: number;
  pageSize: number;
  total: number;
  totalCount?: number;
}
