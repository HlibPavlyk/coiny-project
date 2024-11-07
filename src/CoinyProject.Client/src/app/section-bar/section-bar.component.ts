import { Component } from '@angular/core';
import {NgForOf} from "@angular/common";

@Component({
  selector: 'app-section-bar',
  standalone: true,
  imports: [
    NgForOf
  ],
  templateUrl: './section-bar.component.html',
  styleUrl: './section-bar.component.css'
})
export class SectionBarComponent {
  sections = [
    {
      id: 1,
      name: 'Dashboard',
      description: 'Overview of system metrics and status.',
    },
    {
      id: 2,
      name: 'User Management',
      description: 'Manage users, roles, and permissions.',
    },
    {
      id: 3,
      name: 'Settings',
      description: 'Configure system settings and preferences.',
    },
    {
      id: 4,
      name: 'Reports',
      description: 'View and generate system reports.',
    },
    {
      id: 5,
      name: 'Notifications',
      description: 'Manage alerts and notification settings.',
    },
    // Додайте інші розділи
  ];

  navigateToSection(section: any) {
    // Логіка для навігації до відповідного розділу
    console.log('Navigating to section:', section.name);
    // Наприклад, можна реалізувати навігацію за допомогою Angular Router:
    // this.router.navigate([`/section/${section.id}`]);
  }
}
