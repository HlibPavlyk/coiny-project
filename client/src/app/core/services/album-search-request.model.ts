export interface AlbumSearchRequest {
  page: number;
  size: number;
  sortItem: string;
  isAscending: boolean;
  search?: string | null;
}
