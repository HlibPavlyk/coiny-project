import {UsernameModel} from "../../services/username.model";

export interface AlbumViewGetDto {
  id: string;
  name: string;
  description: string;
  rate: number;
  author: UsernameModel;
  updatedAt: string;
  imagesUrls: string[];
  currentImageIndex: number;
}
