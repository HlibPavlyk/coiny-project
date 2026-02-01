import { Injectable } from '@angular/core';
import {HttpClient, HttpParams} from "@angular/common/http";
import {Observable} from "rxjs";
import {SearchRequestModel} from "@core/models/api/search-request.model.ts";
import {AlbumViewGetDto} from "@features/albums/components/albums-view/album-view-get.model";
import {AlbumPostDto} from "@features/albums/models/album-create.model";
import {AlbumUpda} from "@features/albums/models/album-update.model";
import {AlbumGetDto} from "@features/albums/components/albums-view/album-get.model";
import {AlbumSearchRequest} from "@core/services/album-search-request.model";
import { environment } from '@env/environment';

@Injectable({
  providedIn: 'root'
})
export class AlbumService {
  private baseUrl = `${environment.apiUrl}/albums`;

  constructor(private http: HttpClient) {
  }

  getPagedAlbumsForView(searchRequest: AlbumSearchRequest): Observable<PaginatedResponse<AlbumViewGetDto>> {
    const request: SearchRequestModel = {
      offset: (searchRequest.page - 1) * searchRequest.size,
      count: searchRequest.size,
      searchText: searchRequest.search || undefined/*,
      sortBy: searchRequest.sortItem ? [{
        field: searchRequest.sortItem,
        direction: searchRequest.isAscending ? 'Asc' : 'Desc'
      }] : undefined*/
    };
    return this.http.post<PaginatedResponse<AlbumViewGetDto>>(`${this.baseUrl}/search`, request);
  }

  getPagedAlbumsByUser(userId: string, searchRequest: AlbumSearchRequest): Observable<PaginatedResponse<AlbumViewGetDto>> {
    const request: SearchRequestModel = {
      offset: (searchRequest.page - 1) * searchRequest.size,
      count: searchRequest.size,
      searchText: searchRequest.search || undefined,
      sortBy: searchRequest.sortItem ? [{
        field: searchRequest.sortItem,
        direction: searchRequest.isAscending ? 'Asc' : 'Desc'
      }] : undefined
    };
    return this.http.post<PaginatedResponse<AlbumViewGetDto>>(`${environment.apiUrl}/users/${userId}/albums/search`, request);
  }

  // Create a new album
  addAlbum(album: AlbumPostDto): Observable<AlbumGetDto> {
    return this.http.post<AlbumGetDto>(this.baseUrl, album);
  }

  // Update an existing album
  updateAlbum(id: string, album: AlbumUpda): Observable<AlbumGetDto> {
    return this.http.patch<AlbumGetDto>(`${this.baseUrl}/${id}`, album);
  }

  // Get album by ID
  getAlbumById(id: string): Observable<AlbumGetDto> {
    return this.http.get<AlbumGetDto>(`${this.baseUrl}/${id}`);
  }

  // Deactivate album by ID
  deactivateAlbum(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/deactivate`, null);
  }

  // Activate album by ID
  activateAlbum(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/activate`, null);
  }
}
