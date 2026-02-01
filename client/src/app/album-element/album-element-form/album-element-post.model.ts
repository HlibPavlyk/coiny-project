export interface AlbumElementPostModel {
  name: string;
  description: string | null;
  photo: File | null;
  albumId: string;
}
