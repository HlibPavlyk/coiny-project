import { Component } from '@angular/core';
import {AlbumViewComponent} from "./album-view/album-view.component";
import {SectionBarComponent} from "../section-bar/section-bar.component";

@Component({
  selector: 'app-album',
  standalone: true,
  imports: [
    AlbumViewComponent,
    SectionBarComponent
  ],
  templateUrl: './album.component.html',
  styleUrl: './album.component.css'
})
export class AlbumComponent {

}
