import {Component, OnInit} from "@angular/core";
import {AlbumService} from "../../services/album.service";
import {AlbumViewGetDto} from "./album-view-get.model";
import {DatePipe, NgClass, NgForOf, NgIf} from "@angular/common";
import {FormsModule} from "@angular/forms";
import {ActivatedRoute, RouterLink} from "@angular/router";
import {UserModel} from "../../services/user.model";
import {AuthService} from "../../services/auth.service";
import {ItemNoFoundComponent} from "../../shared/item-no-found/item-no-found.component";

@Component({
  selector: 'app-albums-view',
  templateUrl: './albums-view.component.html',
  standalone: true,
  imports: [
    NgClass,
    FormsModule,
    NgForOf,
    DatePipe,
    RouterLink,
    NgIf,
    ItemNoFoundComponent
  ],
  styleUrls: ['./albums-view.component.css']
})
export class AlbumsViewComponent implements OnInit {
  albums: AlbumViewGetDto[] = [];
  page: number = 1;
  size: number = 2;
  sortItem: string = 'time';
  isAscending: boolean = false;
  searchQuery: string = '';
  totalPages: number = 0;

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

    if (this.isCurrentUser) {
      this.sortItem = 'status';
      this.isAscending = true;
    }

    this.getAlbums();
  }

  // Error message field to store any errors
  errorMessage: string | null = null;

  getAlbums(): void {
    // Clear any existing error messages before making a request
    this.errorMessage = null;
    const trimmedQuery = this.searchQuery.trim();

    if (this.isCurrentUser || this.userId) {
      // Fetch albums for the current user or a specific user
      this.albumService.getPagedAlbumsByUser(this.userId, this.page, this.size, this.sortItem, this.isAscending, trimmedQuery)
          .subscribe({
            next: (response) => {
              this.totalPages = response.totalPages;
              this.albums = response.items.map(album => {
                album.currentImageIndex = 0;
                return album;
              });
            },
            error: (error) => {
              this.albums = [];
              console.error('Error fetching user albums:', error);
              this.errorMessage = 'Failed to load albums. Please try again later.';
            }
          });
    } else {
      // Fetch albums for viewing (not specific to a user)
      this.albumService.getPagedAlbumsForView(this.page, this.size, this.sortItem, this.isAscending, trimmedQuery)
          .subscribe({
            next: (response) => {
              this.totalPages = response.totalPages;
              this.albums = response.items.map(album => {
                album.currentImageIndex = 0;
                return album;
              });
            },
            error: (error) => {
              this.albums = [];
              console.error('Error fetching albums:', error);
              this.errorMessage = 'Failed to load albums. Please try again later.';
            }
          });
    }
  }

  setSort(sortType: string, isAscending: boolean): void {
    this.sortItem = sortType;
    this.isAscending = isAscending;
    this.getAlbums(); // Re-fetch albums with new sort settings
  }

  onSearch(): void {
    this.page = 1;
    this.getAlbums();
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

  // Pagination methods
  nextPage(): void {
    if (this.page < this.totalPages) {
      this.page++;
      this.getAlbums();
    }
  }

  previousPage(): void {
    if (this.page > 1) {
      this.page--;
      this.getAlbums();
    }
  }

}
