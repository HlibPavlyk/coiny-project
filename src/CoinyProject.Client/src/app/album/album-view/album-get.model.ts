export interface AlbumViewGetDto {
  id: string;
  name: string;
  description: string;
  rate: number;
  updatedAt: string;
  imagesUrls: string[];
  currentImageIndex: number;
}
