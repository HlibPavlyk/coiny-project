import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import {AlbumViewComponent} from "./album/album-view/album-view.component";
import {AlbumComponent} from "./album/album.component";
import {TopBarComponent} from "./top-bar/top-bar.component";

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, AlbumViewComponent, AlbumComponent, TopBarComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'CoinyProject.Client';
}
