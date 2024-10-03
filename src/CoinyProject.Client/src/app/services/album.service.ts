import { Injectable } from '@angular/core';
import {HttpClient} from "@angular/common/http";
import {Observable} from "rxjs";
import {PagedResponse} from "./paged-response.model";
import {AlbumViewGetDto} from "../album/album-view/album-view-get.model";
import {AlbumPostDto} from "../album/album-form/album-post.model";
import {AlbumPatchDto} from "../album/album-form/album-patch.model";
import {AlbumGetDto} from "../album/album-view/album-get.model";

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

  getPagedAlbumsByCurrentUser(page: number, size: number, sortItem: string, isAscending: boolean): Observable<PagedResponse<AlbumViewGetDto>> {
    const params = {
      page: page.toString(),
      size: size.toString(),
      sortItem: sortItem,
      isAscending: isAscending.toString(),
      addAuth: 'true'
    };
    return this.http.get<PagedResponse<AlbumViewGetDto>>(`${this.baseUrl}/my`, {params});
  }

  getPagedAlbumsByUserId(userId: string, page: number, size: number, sortItem: string, isAscending: boolean): Observable<PagedResponse<AlbumViewGetDto>> {
    const params = {
      page: page.toString(),
      size: size.toString(),
      sortItem: sortItem,
      isAscending: isAscending.toString(),
    };
    return this.http.get<PagedResponse<AlbumViewGetDto>>(`${this.baseUrl}/by-user/${userId}`, {params});
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
    return this.http.get<AlbumGetDto>(`${this.baseUrl}/${id}`);
  }
}
