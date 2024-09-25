import { Component } from '@angular/core';
import {Router} from "@angular/router";
import {AuthService} from "../services/auth.service";
import {FormsModule} from "@angular/forms";
import {LoginRequestDto} from "./login-request.module";

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
    emailOrName: '',
    password: ''
  };

  constructor(private authService: AuthService, private router: Router) {}

  onSubmit() {
    this.authService.login(this.loginDto).subscribe(
      (token: string) => {
        // Save token in localStorage or some other place
        localStorage.setItem('authToken', token);
        this.router.navigate(['/home']); // Navigate to home or some other page
      },
      (error) => {
        alert('Login failed: ' + error.error); // Show error message
      }
    );
  }

  goToRegister() {
    this.router.navigate(['/register']); // Навігація на сторінку реєстрації
  }
}
