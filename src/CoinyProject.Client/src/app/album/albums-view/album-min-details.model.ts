import {UsernameModel} from "../../services/username.model";

export interface AlbumMinDetailsModel {
  id: string;
  name: string;
  author: UsernameModel;
}
