import { Injectable } from '@angular/core';
import {Observable} from "rxjs";
import {UserStatsModel} from "../../features/profile/stats/stats.module";
import {HttpClient, HttpParams} from "@angular/common/http";
import {UsernameModel} from "./username.model";
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private baseUrl = `${environment.apiUrl}/users`;

  constructor(private http: HttpClient) {
  }

  getUserStats(id: string): Observable<UserStatsModel> {
    return this.http.get<UserStatsModel>(`${this.baseUrl}/${id}/profile`);
  }

  getUsernameById(id: string): Observable<UsernameModel> {
    return this.http.get<UsernameModel>(`${this.baseUrl}/${id}/name`);
  }
}
