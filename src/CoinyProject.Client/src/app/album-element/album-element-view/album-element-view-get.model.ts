import {AlbumMinDetailsModel} from "../../album/albums-view/album-min-details.model";

export interface AlbumElementViewGetModel {
  id: string;
  name: string;
  description: string;
  rate: number;
  imageUrl: string;
  album: AlbumMinDetailsModel;
  updatedAt: string;
  createdAt: string;
}
