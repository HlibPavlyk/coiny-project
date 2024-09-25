import { Component } from '@angular/core';
import {Router} from "@angular/router";
import {AuthService} from "../services/auth.service";
import {FormsModule} from "@angular/forms";
import {LoginRequestDto} from "./login-request.module";
import {CookieService} from "ngx-cookie-service";

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    FormsModule
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent  {
  loginDto: LoginRequestDto = {
    emailOrUsername: '',
    password: ''
  };
  errorMessage = '';

  constructor(private authService: AuthService, private cookieService: CookieService, private router: Router) {}

  onLoginFormSubmit() {
    this.authService.login(this.loginDto)
      .subscribe({
        next: (response) => {
          console.log(response);
          this.cookieService.set('Authorization', `Bearer  ${response.token}`,
            undefined, '/', undefined, true, 'Strict');
          this.authService.setUser({
            username: response.username,
            email: response.email,
            roles: response.roles
          });

          this.router.navigate(['']);
        },
        error: (err) => {
          this.errorMessage = `Incorrect login or password (${err.status})`;
          console.error(`${this.errorMessage} - ${err.message}`);
        }
      });
  }


  goToRegister() {
    this.router.navigate(['/register']); // Навігація на сторінку реєстрації
  }
}
