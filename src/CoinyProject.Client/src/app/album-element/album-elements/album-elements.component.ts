import {Component, Input, OnInit} from '@angular/core';
import {DatePipe, NgClass, NgForOf, NgIf} from "@angular/common";
import {ItemNoFoundComponent} from "../../shared/item-no-found/item-no-found.component";
import {FormsModule, ReactiveFormsModule} from "@angular/forms";
import {ActivatedRoute, RouterLink} from "@angular/router";
import {AlbumElementGetModel} from "./album-element-get.model";
import {AlbumElementService} from "../../services/album-element.service";

@Component({
  selector: 'app-album-elements',
  standalone: true,
  imports: [
    DatePipe,
    ItemNoFoundComponent,
    NgForOf,
    NgIf,
    ReactiveFormsModule,
    NgClass,
    FormsModule,
    RouterLink
  ],
  templateUrl: './album-elements.component.html',
  styleUrl: './album-elements.component.css'
})
export class AlbumElementsComponent implements OnInit {
  elements: AlbumElementGetModel [] = [];
  page: number = 1;
  size: number = 3;
  sortItem: string = 'time';
  isAscending: boolean = false;
  searchQuery: string = '';
  totalPages: number = 0;

  albumId: string | null = null;
  errorMessage: string | null = null;


  @Input() isCurrentUser: boolean = false;

  constructor(private albumElementService: AlbumElementService, private route: ActivatedRoute) {}

  ngOnInit(): void {
    this.albumId = this.route.snapshot.paramMap.get('id');

    this.getAlbumElements();
  }

  getAlbumElements(): void {
    // Clear any existing error messages before making a request
    this.errorMessage = null;
    const trimmedQuery = this.searchQuery.trim();

    if (this.albumId) {
      // Fetch albums for the current user or a specific user
      this.albumElementService.getPagedAlbumsElements(this.albumId, this.page, this.size, this.sortItem, this.isAscending, trimmedQuery)
        .subscribe({
          next: (response) => {
            this.totalPages = response.totalPages;
            this.elements = response.items;
          },
          error: (error) => {
            console.error('Error fetching album elements:', error);
            this.errorMessage = 'Failed to load album elements. Please try again later.';
          }
        });
    }
  }

  setSort(sortType: string, isAscending: boolean): void {
    this.sortItem = sortType;
    this.isAscending = isAscending;
    this.getAlbumElements(); // Re-fetch albums with new sort settings
  }

  onSearch(): void {
    this.page = 1;
    this.getAlbumElements();
  }

  // Pagination methods
  nextPage(): void {
    if (this.page < this.totalPages) {
      this.page++;
      this.getAlbumElements();
    }
  }

  previousPage(): void {
    if (this.page > 1) {
      this.page--;
      this.getAlbumElements();
    }
  }

}
