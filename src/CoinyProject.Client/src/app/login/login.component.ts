import { Component } from '@angular/core';
import {ActivatedRoute, Router} from "@angular/router";
import {AuthService} from "../services/auth.service";
import {FormsModule} from "@angular/forms";
import {LoginRequestDto} from "./login-request.module";
import {CookieService} from "ngx-cookie-service";
import {NgIf} from "@angular/common";

@Component({
  selector: 'app-login',
  standalone: true,
    imports: [
        FormsModule,
        NgIf
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

  constructor(private authService: AuthService, private cookieService: CookieService, private router: Router,  private route: ActivatedRoute) {}

  onLoginFormSubmit() {
    this.authService.login(this.loginDto)
      .subscribe({
        next: (response) => {
          console.log(response);
          this.cookieService.set('Authorization', `Bearer  ${response.token}`,
            undefined, '/', undefined, true, 'Strict');
          this.authService.setUser({
            id: response.id,
            username: response.username,
            email: response.email,
            roles: response.roles
          });
          // Get the returnUrl from query parameters or use a default
          const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') || '/';

          // Navigate to the returnUrl
          this.router.navigateByUrl(returnUrl).then(r => console.log(`Navigated to ${returnUrl}`));
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
