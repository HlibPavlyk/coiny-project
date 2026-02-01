export interface PagedResponse<T> {
  totalPages: number;
  items: T[];
}
