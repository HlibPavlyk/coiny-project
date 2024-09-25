import {Component, OnInit} from '@angular/core';
import {NgClass, NgForOf, NgIf, NgOptimizedImage} from "@angular/common";
import {Router} from "@angular/router";
import {UserModel} from "../services/user.module";
import {AuthService} from "../services/auth.service";

@Component({
  selector: 'app-top-bar',
  standalone: true,
  imports: [
    NgOptimizedImage,
    NgForOf,
    NgIf,
    NgClass
  ],
  templateUrl: './top-bar.component.html',
  styleUrl: './top-bar.component.css'
})
export class TopBarComponent implements OnInit{

  user: UserModel | undefined;

  // Variable to toggle language dropdown
  isLanguageDropdownOpen: boolean = false;

  // Default page
  activePage: string = 'Home';

  // Current selected language
  currentLanguage: string = 'EN';

  // Array of available languages
  availableLanguages: string[] = ['UA', 'EN'];

  constructor(protected authService: AuthService, private router: Router) { }

  ngOnInit() {
    this.authService.user().subscribe(user => {
      if (user) {
        this.user = user;
        console.log(`User is logged in as ${user.email}`);
      }
    });
    this.user = this.authService.getUser();
  }

  onLogout(): void{
    this.user = undefined;
    this.authService.logout();
    this.router.navigate(['login']);
    console.log('User is logged out');
  }

  // Toggle the language dropdown
  toggleLanguageDropdown() {
    this.isLanguageDropdownOpen = !this.isLanguageDropdownOpen;
  }

  // Set the selected language
  selectLanguage(language: string) {
    this.currentLanguage = language;
    this.isLanguageDropdownOpen = false; // Close dropdown after selection
  }

  // Method to handle login click (you can customize the logic here)
  handleLoginClick() {
    console.log('Login clicked');
  }

  // Method to handle the create album button click
  handleCreateAlbumClick() {
    console.log('Create Album clicked');
  }

  // Method to handle navigation clicks (for Home, Albums, etc.)
  navigateTo(page: string) {
    this.activePage = page;
    this.router.navigate([`/${page.toLowerCase()}`])
      .then(r => console.log('Navigating to Home'));

  }

}
