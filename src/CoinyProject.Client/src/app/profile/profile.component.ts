import { Component } from '@angular/core';
import {AlbumsViewComponent} from "../album/albums-view/albums-view.component";
import {SectionBarComponent} from "../section-bar/section-bar.component";
import {StatsComponent} from "./stats/stats.component";

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    AlbumsViewComponent,
    SectionBarComponent,
    StatsComponent
  ],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent {

}
