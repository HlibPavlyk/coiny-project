import { Injectable } from '@angular/core';
import {HttpClient} from "@angular/common/http";
import {BehaviorSubject, Observable} from "rxjs";
import {RegisterDto} from "../register/register.module";
import {LoginRequestDto} from "../login/login-request.module";
import {CookieService} from "ngx-cookie-service";
import {UserModel} from "./user.module";
import {LoginResponseModel} from "../login/login-response.module";

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'https://localhost:7218/api/auth';
  private userSubject = new BehaviorSubject<UserModel | undefined>(this.getUser());
  user$ = this.userSubject.asObservable();


  constructor(private http: HttpClient, private cookieService: CookieService) { }

  register(registerDto: RegisterDto): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/register`, registerDto);
  }

  login(loginDto: LoginRequestDto): Observable<LoginResponseModel> {
    return this.http.post<LoginResponseModel>(`${this.apiUrl}/login`, loginDto);
  }

  setUser(user: UserModel): void {
    localStorage.setItem('user-name', user.username);
    localStorage.setItem('user-email', user.email);
    localStorage.setItem('user-roles', user.roles.join());
    this.userSubject.next(user);
  }

  isTokenExpired(token: string): boolean {
    const expiry = (JSON.parse(atob(token.split('.')[1]))).exp;
    return (Math.floor((new Date).getTime() / 1000)) >= expiry;
  }

  user(): Observable<UserModel | undefined> {
    return this.user$;
  }

  getUser(): UserModel | undefined {
    const username = localStorage.getItem('user-name');
    const email = localStorage.getItem('user-email');
    const roles = localStorage.getItem('user-roles');

    if (username && email && roles) {
      return {
        username,
        email,
        roles: roles.split(',')
      };
    }
    return undefined;
  }


  logout(): void {
    localStorage.clear();
    this.cookieService.delete('Authorization', '/');
    this.userSubject.next(undefined);
  }

  hasRoles(roles: string[]): boolean {
    const user = this.getUser();
    if (user && user.roles){
      return roles.some(role => user.roles.includes(role));
    } else {
      return false;
    }
}

}
