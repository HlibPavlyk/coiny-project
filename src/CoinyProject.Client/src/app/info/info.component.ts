import { Component } from '@angular/core';

@Component({
  selector: 'app-info',
  standalone: true,
  imports: [],
  templateUrl: './info.component.html',
  styleUrl: './info.component.css'
})
export class InfoComponent {
  projectTitle = 'COINY Project';
  projectDescription = `COINY is a platform designed to make it easier for users to explore and manage various items such as albums, auctions, and community posts. The platform offers a clean interface, dynamic navigation, and efficient management features.`;
  projectUsage = `To navigate through the platform, use the top navigation bar where you can switch between sections like "Home," "Albums," "Community," and "Auctions." Users can also log in, switch languages, and create new albums directly from the navigation interface. The project focuses on simplicity, ease of use, and efficient item management.`;
}
