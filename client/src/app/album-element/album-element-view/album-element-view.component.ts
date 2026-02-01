import {Component, OnInit} from '@angular/core';
import {AlbumGetDto} from "../../album/albums-view/album-get.model";
import {UserModel} from "../../services/user.model";
import {ActivatedRoute, Router, RouterLink} from "@angular/router";
import {AlbumService} from "../../services/album.service";
import {AuthService} from "../../services/auth.service";
import {AlbumElementsComponent} from "../album-elements/album-elements.component";
import {DatePipe, NgClass, NgIf} from "@angular/common";
import {ItemNoFoundComponent} from "../../shared/item-no-found/item-no-found.component";
import {ModalComponent} from "../../shared/modal/modal.component";
import {AlbumElementService} from "../../services/album-element.service";
import {AlbumElementViewGetModel} from "./album-element-view-get.model";

@Component({
  selector: 'app-album-element-view',
  standalone: true,
  imports: [
    AlbumElementsComponent,
    DatePipe,
    ItemNoFoundComponent,
    ModalComponent,
    NgIf,
    NgClass,
    RouterLink
  ],
  templateUrl: './album-element-view.component.html',
  styleUrl: './album-element-view.component.css'
})
export class AlbumElementViewComponent implements OnInit {
  element: AlbumElementViewGetModel | null = null;
  isMyAlbumElement: boolean = false;
  isModalOpen = false;
  errorMessage = '';

  user: UserModel | undefined;

  constructor(
    private route: ActivatedRoute,
    private albumService: AlbumService,
    private albumElementService: AlbumElementService,
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    const albumElementId = this.route.snapshot.paramMap.get('id');

    if (albumElementId) {
      this.albumElementService.getAlbumElementById(albumElementId).subscribe({
        next: (element) => {
          this.element = element;
          this.isMyAlbumElement = this.ifCurrentUserIsOwner();
        },
        error: (err) => {
          this.errorMessage = `Failed to load album element (${err.status})`;
          console.error('Failed to load album element:', err);
        },
      });
    }
  }

  // Handle Edit button

  ifCurrentUserIsOwner(): boolean {
    this.authService.user().subscribe(user => {
      this.user = user;
    });

    if (this.user && this.element) {
      return this.element.album.author.id === this.user.id;
    }
    return false;
  }
  editAlbumElement(): void {
    if (this.element) {
      this.router.navigate(['/album-element-edit', this.element.id]).then(r =>
        console.log('Edit album element:', r));
    }
  }

  openModal(): void {
    this.isModalOpen = true;
  }

  closeModal(): void {
    this.isModalOpen = false;
  }

  confirmDelete(): void {
    if (this.element) {
      this.albumElementService.deleteAlbumElement(this.element.id).subscribe({
        next: () => {
          this.router.navigate(['/profile'], { queryParams: { my: true } }).then(() => {
            console.log('Album element deleted');
          });

          this.closeModal();
        },
        error: (err) => {
          this.errorMessage = `Failed to delete album element(${err.status})`;
          console.error('Failed to delete album element:', err);
        },
      });
    }
  }
}
