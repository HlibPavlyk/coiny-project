import {UsernameModel} from "../../services/username.model";

export interface AlbumGetDto {
  id: string;
  name: string;
  description: string;
  rate: number;
  status: string;
  author: UsernameModel;
  updatedAt: string;
  createdAt: string;
}
