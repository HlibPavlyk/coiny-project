import {Component, OnInit} from '@angular/core';
import {AlbumGetDto} from "../albums-view/album-get.model";
import {ActivatedRoute, Router, RouterLink} from "@angular/router";
import {AlbumService} from "../../services/album.service";
import {DatePipe, NgIf} from "@angular/common";
import {ModalComponent} from "../../shared/modal/modal.component";
import {AuthService} from "../../services/auth.service";
import {UserModel} from "../../services/user.model";

@Component({
  selector: 'app-album-view',
  standalone: true,
  imports: [
    RouterLink,
    DatePipe,
    NgIf,
    ModalComponent
  ],
  templateUrl: './album-view.component.html',
  styleUrl: './album-view.component.css'
})
export class AlbumViewComponent implements OnInit {
  album: AlbumGetDto | null = null;
  isMyAlbum: boolean = false;
  isModalOpen = false;
  errorMessage = '';

  user: UserModel | undefined;

  constructor(
      private route: ActivatedRoute,
      private albumService: AlbumService,
      private router: Router,
      private authService: AuthService
  ) {}

  ngOnInit(): void {
    const albumId = this.route.snapshot.paramMap.get('id');

    if (albumId) {
      this.albumService.getAlbumById(albumId).subscribe({
        next: (album) => {
          this.album = album;
          this.isMyAlbum = this.ifCurrentUserIsOwner();
        },
        error: (err) => {
          this.errorMessage = `Failed to load album (${err.status})`;
          console.error('Failed to load album:', err);
        },
      });
    }
  }

  // Handle Edit button

  ifCurrentUserIsOwner(): boolean {
    this.authService.user().subscribe(user => {
      this.user = user;
    });

    if (this.user && this.album) {
      return this.album.author.id === this.user.id;
    }
    return false;
  }
  editAlbum(): void {
    if (this.album) {
      this.router.navigate(['/album-edit', this.album.id]).then(r => console.log('Edit album:', r));
    }
  }

  openModal(): void {
    this.isModalOpen = true;
  }

  closeModal(): void {
    this.isModalOpen = false;
  }

  confirmDelete(): void {
    if (this.album) {
      this.albumService.deactivateAlbum(this.album.id).subscribe({
        next: () => {
          this.router.navigate(['/albums']).then(() => console.log('Album deactivated'));
          this.closeModal();
        },
        error: (err) => {
          this.errorMessage = `Failed to deactivate album (${err.status})`;
          console.error('Failed to deactivate album:', err);
        },
      });
    }
  }
}
