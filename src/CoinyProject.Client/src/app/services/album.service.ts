import { Injectable } from '@angular/core';
import {HttpClient, HttpParams} from "@angular/common/http";
import {Observable} from "rxjs";
import {PagedResponse} from "./paged-response.model";
import {AlbumViewGetDto} from "../album/albums-view/album-view-get.model";
import {AlbumPostDto} from "../album/album-form/album-post.model";
import {AlbumPatchDto} from "../album/album-form/album-patch.model";
import {AlbumGetDto} from "../album/albums-view/album-get.model";

@Injectable({
  providedIn: 'root'
})
export class AlbumService {
  private baseUrl = 'https://localhost:7218/api/albums'; // Задайте вашу URL

  constructor(private http: HttpClient) {
  }

  getPagedAlbumsForView(page: number, size: number, sortItem: string, isAscending: boolean): Observable<PagedResponse<AlbumViewGetDto>> {
    const params = {
      page: page.toString(),
      size: size.toString(),
      sortItem: sortItem,
      isAscending: isAscending.toString(),
    };
    return this.http.get<PagedResponse<AlbumViewGetDto>>(this.baseUrl, {params});
  }

  getPagedAlbumsByUser(userId: string | null, page: number, size: number, sortItem: string, isAscending: boolean): Observable<PagedResponse<AlbumViewGetDto>> {
    let params = new HttpParams();
    if (userId) {
      params = params.append('userId', userId);
    }
    else {
      params = params.append('addAuth', 'true');
    }

    params = params.append('page', page.toString());
    params = params.append('size', size.toString(),);
    params = params.append('sortItem', sortItem);
    params = params.append('isAscending', isAscending.toString());

    return this.http.get<PagedResponse<AlbumViewGetDto>>(`${this.baseUrl}/by-user`, { params });
  }

  // Create a new album
  addAlbum(album: AlbumPostDto): Observable<AlbumGetDto> {
    return this.http.post<AlbumGetDto>(`${this.baseUrl}?addAuth=true`, album);
  }

  // Update an existing album
  updateAlbum(id: string, album: AlbumPatchDto): Observable<AlbumGetDto> {
    return this.http.patch<AlbumGetDto>(`${this.baseUrl}/${id}?addAuth=true`, album);
  }

  // Get album by ID
  getAlbumById(id: string): Observable<AlbumGetDto> {
    const params = {
      addAuth: true.toString()
    };
    return this.http.get<AlbumGetDto>(`${this.baseUrl}/${id}`, {params});
  }

  // Deactivate album by ID
  deactivateAlbum(id: string): Observable<void> {
    const params = {
      addAuth: true.toString()
    };
      return this.http.post<void>(`${this.baseUrl}/${id}/deactivate`, null, {params});
    }
}
