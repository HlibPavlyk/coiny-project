import {AlbumStatusEnum} from "@core/models/enums/album-status.enum";
import {BaseLinkModel} from "@shared/models/base-link.model";
import {AlbumElementLinkModel} from "@shared/models/album-element-link.model";

export interface AlbumGetModel {
  id: string;
  name: string;
  description: string;
  rate: number;
  status: AlbumStatusEnum;
  author: BaseLinkModel;
  images: AlbumElementLinkModel[];
  updatedAt: string;
  createdAt: string;
}
