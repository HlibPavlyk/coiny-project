import { Component } from '@angular/core';
import {NgClass, NgForOf, NgIf, NgOptimizedImage} from "@angular/common";

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
export class TopBarComponent {

  // Variable to toggle language dropdown
  isLanguageDropdownOpen: boolean = false;

  // Default page
  activePage: string = 'Home';

  // Current selected language
  currentLanguage: string = 'EN';

  // Array of available languages
  availableLanguages: string[] = ['UA', 'EN'];

  constructor() {}

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
    console.log(`Navigating to ${page}`);
  }

}
