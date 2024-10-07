import {UsernameModel} from "../../services/username.model";

export interface AlbumGetDto {
  id: string;
  name: string;
  description: string;
  rate: number;
  author: UsernameModel;
  updatedAt: string;
  createdAt: string;
}
