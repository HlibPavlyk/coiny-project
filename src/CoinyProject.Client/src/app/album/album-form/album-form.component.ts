import {Component, OnInit} from '@angular/core';
import {AlbumService} from "../../services/album.service";
import {ActivatedRoute, Router} from "@angular/router";
import {FormsModule} from "@angular/forms";
import {NgForOf} from "@angular/common";

@Component({
  selector: 'app-album-form',
  standalone: true,
  imports: [
    FormsModule,
    NgForOf
  ],
  templateUrl: './album-form.component.html',
  styleUrl: './album-form.component.css'
})
export class AlbumFormComponent implements OnInit {
  album: any = { name: '', description: '', imagesUrls: [] };
  isEditMode = false; // Toggle between add and update mode
  albumId: string | null = null; // Album ID for edit mode

  constructor(
    private albumService: AlbumService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.albumId = this.route.snapshot.paramMap.get('id');
    if (this.albumId) {
      this.isEditMode = true;
      this.loadAlbumData(this.albumId);
    }
  }

  // Load album data for editing
  loadAlbumData(id: string): void {
    this.albumService.getAlbumById(id).subscribe((album) => {
      this.album = album;
    });
  }

  // Handle file selection
  onPhotosSelected(event: any): void {
    const files = event.target.files;
    if (files) {
      // Handle file uploads here, currently just storing filenames
      this.album.imagesUrls = Array.from(files).map((file: any) => file.name);
    }
  }

  // Submit form data (create or update album)
  onSubmit(): void {
    if (this.isEditMode && this.albumId) {
      this.albumService.updateAlbum(this.albumId, this.album).subscribe({
        next: () => this.router.navigate(['/albums']),
        error: (err) => console.error('Update failed', err),
      });
    } else {
      this.albumService.addAlbum(this.album).subscribe({
        next: () => this.router.navigate(['/albums']),
        error: (err) => console.error('Creation failed', err),
      });
    }
  }

  // Handle the "Create and Add" button
  onCreateAndAdd(): void {
    this.albumService.addAlbum(this.album).subscribe({
      next: () => {
        this.router.navigate(['/albums/new']); // Redirect to add new album again
      },
      error: (err) => console.error('Creation failed', err),
    });
  }

  // Handle cancel button
  onCancel(): void {
    this.router.navigate(['/albums']);
  }
}
