import { Injectable } from '@angular/core';
import {Observable} from "rxjs";
import {UserStatsModel} from "../profile/stats/stats.module";
import {HttpClient, HttpParams} from "@angular/common/http";

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private baseUrl = 'https://localhost:7218/api/users'; // Задайте вашу URL

  constructor(private http: HttpClient) {
  }

  getUserStats(id: string| null): Observable<UserStatsModel> {
    let params = new HttpParams();
    if (id) {
      params = params.append('id', id);
    }
    else {
      params = params.append('addAuth', 'true');
    }
    return this.http.get<UserStatsModel>(`${this.baseUrl}/stats`, {params} ); // Adjust the endpoint accordingly
  }

}
