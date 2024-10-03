import { Component } from '@angular/core';
import {AlbumViewComponent} from "../album/album-view/album-view.component";
import {SectionBarComponent} from "../section-bar/section-bar.component";

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    AlbumViewComponent,
    SectionBarComponent
  ],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent {

}
