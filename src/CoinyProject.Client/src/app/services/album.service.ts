import { Injectable } from '@angular/core';
import {HttpClient} from "@angular/common/http";
import {Observable} from "rxjs";
import {PagedResponse} from "./paged-response.model";
import {AlbumViewGetDto} from "../album/album-view/album-get.model";

@Injectable({
  providedIn: 'root'
})
export class AlbumService {
  private baseUrl = 'https://localhost:7218/api/albums'; // Задайте вашу URL

  constructor(private http: HttpClient) {}

  getPagedAlbums(page: number, size: number, sortItem: string, isAscending: boolean): Observable<PagedResponse<AlbumViewGetDto>> {
    const params = {
      page: page.toString(),
      size: size.toString(),
      sortItem: sortItem,
      isAscending: isAscending.toString()
    };
    return this.http.get<PagedResponse<AlbumViewGetDto>>(this.baseUrl, { params });
  }
  // Create a new album
  addAlbum(album: any): Observable<any> {
    return this.http.post(this.baseUrl, album);
  }

  // Update an existing album
  updateAlbum(id: string, album: any): Observable<any> {
    return this.http.patch(`${this.baseUrl}/${id}`, album);
  }

  // Get album by ID
  getAlbumById(id: string): Observable<any> {
    return this.http.get(`${this.baseUrl}/${id}`);
  }
}
