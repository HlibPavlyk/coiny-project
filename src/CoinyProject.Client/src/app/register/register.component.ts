import { Component } from '@angular/core';
import {Router} from "@angular/router";
import {AuthService} from "../services/auth.service";
import {FormsModule} from "@angular/forms";
import {RegisterDto} from "./register.module";

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    FormsModule
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

  constructor(private authService: AuthService, private router: Router) {}

  onSubmit() {
    this.authService.register(this.registerDto).subscribe(
      (response: any) => {
        alert('Registration successful');
        this.router.navigate(['/login']); // Navigate to login after successful registration
      },
      (error) => {
        alert('Registration failed: ' + error.error); // Show error message
      }
    );
  }
}
