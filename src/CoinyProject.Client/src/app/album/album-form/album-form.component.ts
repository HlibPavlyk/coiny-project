import {Component, OnInit} from '@angular/core';
import {AlbumService} from "../../services/album.service";
import {ActivatedRoute, Router} from "@angular/router";
import {FormsModule} from "@angular/forms";
import {NgForOf, NgIf} from "@angular/common";
import {AlbumPostDto} from "./album-post.model";
import {AlbumPatchDto} from "./album-patch.model";

@Component({
  selector: 'app-album-form',
  standalone: true,
  imports: [
    FormsModule,
    NgForOf,
    NgIf
  ],
  templateUrl: './album-form.component.html',
  styleUrl: './album-form.component.css'
})
export class AlbumFormComponent implements OnInit {
  album: AlbumPostDto = { name: '', description: ''};
  startAlbum: AlbumPostDto = { name: '', description: ''};
  isEditMode = false; // Toggle between add and update mode
  albumId: string | null = null; // Album ID for edit mode
  errorMessage = '';

  constructor(
    private albumService: AlbumService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.albumId = this.route.snapshot. paramMap.get('id');
    if (this.albumId) {
      this.isEditMode = true;
      this.loadAlbumData(this.albumId);
    }
  }

  // Load album data for editing
  loadAlbumData(id: string): void {
    this.albumService.getAlbumById(id).subscribe({
      next: (album) => {
      this.album = { name: album.name, description: album.description };
      this.startAlbum = { ...this.album };
    },
      error: (err) => {
        this.errorMessage = `Failed to load album data: ${err.status}`;
        console.error('Failed to load album data', err.message());
      },
    });
    }

  // Submit form data (create or update album)
  onSubmit(): void {
    // Оголошуємо об'єкт для часткового оновлення
    const partialAlbum: AlbumPatchDto = {
      name: null,
      description: null,
    };

    // Перевіряємо, чи змінилася назва альбому, і якщо так — додаємо її в об'єкт
    if (this.startAlbum.name !== this.album.name) {
      partialAlbum.name = this.album.name;
    }

    // Перевіряємо, чи змінився опис альбому, і якщо так — додаємо його в об'єкт
    if (this.startAlbum.description !== this.album.description) {
      partialAlbum.description = this.album.description;
    }

    if (this.isEditMode && this.albumId) {
      this.albumService.updateAlbum(this.albumId, partialAlbum).subscribe({
        next: (response) => {
          console.log('Update successful ', response);
          this.router.navigate(['/albums'])
        },
        error: (err) => {
          console.error('Update failed', err);
          this.errorMessage = `Failed to update album: ${err.status}`;
        },
      });
    } else {
      this.albumService.addAlbum(this.album).subscribe({
        next: (response) => {
          console.log('Adding successful ', response);
          this.router.navigate(['/albums'])
        },
        error: (err) => {
          console.error('Adding failed', err);
          this.errorMessage = `Failed to add album: ${err.status}`;
        },
      });
    }
  }


  onCreateAndFill(): void {
    // Якщо опис пустий, встановлюємо його в null
    if (!this.album.description) {
      this.album.description = null;
    }

    this.albumService.addAlbum(this.album).subscribe({
      next: () => {
        this.router.navigate(['/info']); // Redirect to add new album again
      },
      error: (err) => {
        console.error('Adding failed', err);
        this.errorMessage = `Failed to add album: ${err.status}`;
      },
    });
  }


  // Handle cancel button
  onCancel(): void {
    this.router.navigate(['/albums']);
  }
}
