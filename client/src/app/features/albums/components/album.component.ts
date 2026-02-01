import { Component } from '@angular/core';
import {AlbumsViewComponent} from "@features/albums/components/albums-view/albums-view.component";
import {SectionBarComponent} from "../../../layout/section-bar/section-bar.component";

@Component({
  selector: 'app-album',
  standalone: true,
  imports: [
    AlbumsViewComponent,
    SectionBarComponent
  ],
  templateUrl: './album.component.html',
  styleUrl: './album.component.css'
})
export class AlbumComponent {

}
