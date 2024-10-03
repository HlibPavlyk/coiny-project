import { Component } from '@angular/core';
import {AlbumViewComponent} from "../album/album-view/album-view.component";
import {SectionBarComponent} from "../section-bar/section-bar.component";
import {StatsComponent} from "./stats/stats.component";

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    AlbumViewComponent,
    SectionBarComponent,
    StatsComponent
  ],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent {

}
