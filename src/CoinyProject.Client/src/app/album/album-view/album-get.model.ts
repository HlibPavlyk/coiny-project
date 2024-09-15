export interface AlbumGetDto {
  id: string;
  name: string;
  description?: string;
  rate: number;
  imagesUrls: string[];
}
