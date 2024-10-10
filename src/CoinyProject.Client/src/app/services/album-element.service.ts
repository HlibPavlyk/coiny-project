import { Injectable } from '@angular/core';
import {HttpClient, HttpParams} from "@angular/common/http";
import {Observable} from "rxjs";
import {PagedResponse} from "./paged-response.model";
import {AlbumViewGetDto} from "../album/albums-view/album-view-get.model";
import {AlbumPostDto} from "../album/album-form/album-post.model";
import {AlbumPatchDto} from "../album/album-form/album-patch.model";
import {AlbumGetDto} from "../album/albums-view/album-get.model";
import {AlbumElementGetModel} from "../album-element/album-elements/album-element-get.model";
import {AlbumElementViewGetModel} from "../album-element/album-element-view/album-element-view-get.model";

@Injectable({
  providedIn: 'root'
})
export class AlbumElementService {
  private baseUrl = 'https://localhost:7218/api/album-elements'; // Задайте вашу URL

  constructor(private http: HttpClient) {
  }

  getPagedAlbumsElements(albumId: string, page: number, size: number, sortItem: string, isAscending: boolean, search: string | null = null): Observable<PagedResponse<AlbumElementGetModel>> {
    let params = new HttpParams();
    params = params.append('page', page.toString());
    params = params.append('size', size.toString(),);
    params = params.append('sortItem', sortItem);
    params = params.append('isAscending', isAscending.toString());

    if (search) {
      params = params.append('search', search);
    }
    params = params.append('addAuth', 'true');

    return this.http.get<PagedResponse<AlbumElementGetModel>>(`${this.baseUrl}/by-album/${albumId}`, {params});
  }

  // Get album element by ID
  getAlbumElementById(id: string): Observable<AlbumElementViewGetModel> {
    const params = {
      addAuth: true.toString()
    };
    return this.http.get<AlbumElementViewGetModel>(`${this.baseUrl}/${id}`, {params});
  }
  // Create a new album
  addAlbum(album: AlbumPostDto): Observable<AlbumGetDto> {
    return this.http.post<AlbumGetDto>(`${this.baseUrl}?addAuth=true`, album);
  }

  // Update an existing album
  updateAlbum(id: string, album: AlbumPatchDto): Observable<AlbumGetDto> {
    return this.http.patch<AlbumGetDto>(`${this.baseUrl}/${id}?addAuth=true`, album);
  }


  // Deactivate album by ID
  deleteAlbumElement(id: string): Observable<void> {
    const params = {
      addAuth: true.toString()
    };
      return this.http.delete<void>(`${this.baseUrl}/${id}`, {params});
    }

    activateAlbum(id: string): Observable<void> {
    const params = {
      addAuth: true.toString()
    };
      return this.http.post<void>(`${this.baseUrl}/${id}/activate`, null, {params});
    }
}
