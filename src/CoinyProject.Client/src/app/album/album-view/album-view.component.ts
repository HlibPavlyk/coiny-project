import {Component, OnInit} from "@angular/core";
import {AlbumService} from "../../services/album.service";
import {AlbumViewGetDto} from "./album-view-get.model";
import {DatePipe, NgClass, NgForOf} from "@angular/common";
import {FormsModule} from "@angular/forms";
import {ActivatedRoute, RouterLink} from "@angular/router";
import {UserModel} from "../../services/user.model";
import {AuthService} from "../../services/auth.service";

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

  userId: string | null = null;
  isCurrentUser: boolean = false;

  user: UserModel | undefined;

  constructor(private authService:AuthService, private albumService: AlbumService, private route: ActivatedRoute) {}

  ngOnInit(): void {
    this.authService.user().subscribe(user => {
      this.user = user;
    });
    // Отримання параметрів із маршруту або queryParams
    this.route.params.subscribe(params => {
      this.userId = params['id'] || null; // якщо передається userId
    });

    this.route.queryParams.subscribe(queryParams => {
      this.isCurrentUser = queryParams['my'] === 'true'; // перевіряємо, чи це альбоми поточного користувача
    });

    this.getAlbums();
  }

  getAlbums(): void {
    // Якщо переглядаються альбоми поточного користувача
    if (this.isCurrentUser || this.userId) {
      this.albumService.getPagedAlbumsByUser(this.userId, this.page, this.size, this.sortItem, this.isAscending).subscribe(response => {
        this.albums = response.items.map(album => {
          album.currentImageIndex = 0;
          return album;
        });
        this.filteredAlbums = this.albums;
      });
    }
    else {
      this.albumService.getPagedAlbumsForView(this.page, this.size, this.sortItem, this.isAscending).subscribe(response => {
        this.albums = response.items.map(album => {
          album.currentImageIndex = 0;
          return album;
        });
        this.filteredAlbums = this.albums;
      });
    }
  }

  setSort(sortType: string, isAscending: boolean): void {
    this.sortItem = sortType === 'rate' ? 'rate' : 'time';
    this.isAscending  = isAscending;
    this.getAlbums();
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

  onImageError(event: Event) {
    const target = event.target as HTMLImageElement;
    target.src = 'no-image.jpg';
  }

}
