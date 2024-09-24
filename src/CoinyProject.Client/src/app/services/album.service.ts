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
}
