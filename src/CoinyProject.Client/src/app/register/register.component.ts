import { Component } from '@angular/core';
import {Router} from "@angular/router";
import {AuthService} from "../services/auth.service";
import {FormsModule} from "@angular/forms";
import {RegisterDto} from "./register.module";
import {LoginRequestDto} from "../login/login-request.module";
import {CookieService} from "ngx-cookie-service";
import {NgIf} from "@angular/common";

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    FormsModule,
    NgIf
  ],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {
  registerDto: RegisterDto = {
    username: '',
    email: '',
    password: ''
  };
  errorMessage = '';

  constructor(private authService: AuthService, private router: Router, private cookieService: CookieService) {}

  // Method triggered when the form is submitted
  onSubmit() {
    this.authService.register(this.registerDto).subscribe({
      next: (response) => {
        console.log('Registration successful');
        // Auto-login after successful registration
        this.loginAfterRegister();
      },
      error: (err) => {
        this.errorMessage = `Login or password is incorrect (${err.status})`;
        console.error(`${this.errorMessage} - ${err.message}`)}
    });
  }

  // Method to automatically log in the user after registration
  loginAfterRegister() {
    const loginDto: LoginRequestDto = {
      emailOrUsername: this.registerDto.email, // Assuming the API accepts email or username for login
      password: this.registerDto.password,
    };

    this.authService.login(loginDto).subscribe({
      next: (response) => {
        console.log('Login successful after registration');
        this.cookieService.set('Authorization', `Bearer  ${response.token}`,
        undefined, '/', undefined, true, 'Strict');
        this.authService.setUser({
          id: response.id,
          username: response.username,
          email: response.email,
          roles: response.roles
        });

        this.router.navigate(['']);
      },
      error: (error) => {
        console.error('Auto-login failed:', error);
        // Handle login error here
      }
    });
  }

  // Method to navigate to login page
  goToLogin() {
    this.router.navigate(['/login']);
  }
}
