import {Component, OnInit} from "@angular/core";
import {AlbumService} from "../../services/album.service";
import {AlbumViewGetDto} from "./album-get.model";
import {DatePipe, NgClass, NgForOf} from "@angular/common";
import {FormsModule} from "@angular/forms";
import {RouterLink} from "@angular/router";

@Component({
  selector: 'app-album-view',
  templateUrl: './album-view.component.html',
  standalone: true,
  imports: [
    NgClass,
    FormsModule,
    NgForOf,
    DatePipe,
    RouterLink
  ],
  styleUrls: ['./album-view.component.css']
})
export class AlbumViewComponent implements OnInit {
  albums: AlbumViewGetDto[] = [];
  filteredAlbums: AlbumViewGetDto[] = [];
  page: number = 1;
  size: number = 10;
  sortItem: string = 'time';
  isAscending: boolean = false;
  searchQuery: string = '';

  constructor(private albumService: AlbumService) {}

  ngOnInit(): void {
    this.getPagedAlbums();
  }

  getPagedAlbums(): void {
    this.albumService.getPagedAlbums(this.page, this.size, this.sortItem, this.isAscending)
      .subscribe((response) => {
        this.albums = response.items.map(album => {
          album.currentImageIndex = 0;
          return album;
        });

        this.filteredAlbums = this.albums;
      });
  }

  setSort(sortType: string, isAscending: boolean): void {
    this.sortItem = sortType === 'rate' ? 'rate' : 'time';
    this.isAscending  = isAscending;
    this.getPagedAlbums();
  }

  searchAlbums(): void {
    this.filteredAlbums = this.albums.filter(album =>
      album.name.toLowerCase().includes(this.searchQuery.toLowerCase()));
  }

  getPreviousImage(album: any) {
    album.currentImageIndex = (album.currentImageIndex > 0)
      ? album.currentImageIndex - 1
      : album.imagesUrls.length - 1;
  }

  getNextImage(album: any) {
    album.currentImageIndex = (album.currentImageIndex < album.imagesUrls.length - 1)
      ? album.currentImageIndex + 1
      : 0;
  }

  goToImage(album: any, index: number) {
    if (index >= 0 && index < album.imagesUrls.length) {
      album.currentImageIndex = index;
    }
  }
}
