import { Injectable } from '@angular/core';
import {HttpClient, HttpParams} from "@angular/common/http";
import {Observable} from "rxjs";
import {PagedResponse} from "./paged-response.model";
import {PaginatedResponse} from "../models/api/pagination-response.model";
import {SearchRequestModel} from "../models/api\search-request.model.ts";
import {AlbumViewGetDto} from "@features/albums/components/albums-view/album-view-get.model";
import {AlbumPostDto} from "@features/albums/models/album-create.model";
import {AlbumUpda} from "@features/albums/models/album-update.model";
import {AlbumGetDto} from "@features/albums/components/albums-view/album-get.model";
import {AlbumElementGetModel} from "../../features/album-element/album-elements/album-element-get.model";
import {AlbumElementViewGetModel} from "../../features/album-element/album-element-view/album-element-view-get.model";
import {AlbumElementPostModel} from "../../features/album-element/album-element-form/album-element-post.model";
import {AlbumElementPatchModel} from "../../features/album-element/album-element-form/album-element-patch.model";
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AlbumElementService {
  constructor(private http: HttpClient) {
  }

  getPagedAlbumsElements(albumId: string, page: number, size: number, sortItem: string, isAscending: boolean, search: string | null = null): Observable<PaginatedResponse<AlbumElementGetModel>> {
    const request: SearchRequestModel = {
      offset: (page - 1) * size,
      count: size,
      searchText: search || undefined,
      sortBy: sortItem ? [{
        field: sortItem,
        direction: isAscending ? 'Asc' : 'Desc'
      }] : undefined
    };
    return this.http.post<PaginatedResponse<AlbumElementGetModel>>(`${environment.apiUrl}/albums/${albumId}/elements/search`, request);
  }

  // Get album element by ID
  getAlbumElementById(albumId: string, id: string): Observable<AlbumElementViewGetModel> {
    return this.http.get<AlbumElementViewGetModel>(`${environment.apiUrl}/albums/${albumId}/elements/${id}`);
  }

  // Create a new album element
  addAlbumElement(albumId: string, element: FormData): Observable<AlbumElementGetModel> {
    return this.http.post<AlbumElementGetModel>(`${environment.apiUrl}/albums/${albumId}/elements`, element);
  }

  // Update an existing album element
  updateAlbumElement(albumId: string, id: string, element: FormData): Observable<AlbumElementGetModel> {
    return this.http.patch<AlbumElementGetModel>(`${environment.apiUrl}/albums/${albumId}/elements/${id}`, element);
  }

  // Delete album element by ID
  deleteAlbumElement(albumId: string, id: string): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/albums/${albumId}/elements/${id}`);
  }

  // Move album element to different album
  moveAlbumElement(albumId: string, id: string, targetAlbumId: string): Observable<void> {
    return this.http.patch<void>(`${environment.apiUrl}/albums/${albumId}/elements/${id}/move`, { targetAlbumId });
  }

  // Activate album element
  activateAlbumElement(albumId: string, id: string): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/albums/${albumId}/elements/${id}/activate`, null);
  }

  // Deactivate album element
  deactivateAlbumElement(albumId: string, id: string): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/albums/${albumId}/elements/${id}/deactivate`, null);
  }
}
