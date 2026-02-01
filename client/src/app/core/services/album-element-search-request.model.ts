export interface AlbumElementSearchRequest {
  albumId: string;
  page: number;
  size: number;
  sortItem: string;
  isAscending: boolean;
  search?: string | null;
}
