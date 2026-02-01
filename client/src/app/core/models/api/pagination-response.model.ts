export interface PaginatedResponse<T> {
  totalCount: number;
  items: T[];
}
