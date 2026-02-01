import {Component, OnInit} from '@angular/core';
import {AlbumElementPostModel} from "./album-element-post.model";
import {ActivatedRoute, Router} from "@angular/router";
import {AlbumElementService} from "../../../core/services/album-element.service";
import {Location, NgIf} from "@angular/common";
import {FormsModule} from "@angular/forms";

@Component({
  selector: 'app-album-element-form',
  standalone: true,
  imports: [
    NgIf,
    FormsModule
  ],
  templateUrl: './album-element-form.component.html',
  styleUrl: './album-element-form.component.css'
})
export class AlbumElementFormComponent implements OnInit {
  albumElement: AlbumElementPostModel = { name: '', albumId: '', photo: null, description : null };
  startAlbumElement: AlbumElementPostModel = { name: '', albumId: '', photo: null, description : null };
  previewUrl: string | ArrayBuffer | null = null;
  isEditMode = false;
  errorMessage = '';
  elementId: string | null = null;
  albumId: string | null = null;

  constructor(
    private albumElementService: AlbumElementService,
    private router: Router,
    private route: ActivatedRoute,
    private location: Location
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      this.elementId = params.get('id');
      const currentRoute = this.router.url;

      if (currentRoute.includes('album-element-edit')) {
        this.isEditMode = true;
        this.loadAlbumElementData(this.elementId!);
      } else if (currentRoute.includes('album-element-create')) {
        this.albumId = this.elementId; // In this case, the parameter represents the album ID
      }
    });
  }

  loadAlbumElementData(id: string): void {
    // TODO: Backend requires albumId, need to get from query params
    const albumId = this.route.snapshot.queryParamMap.get('albumId');

    if (!albumId) {
      this.errorMessage = 'Album ID is required. Please navigate from an album page.';
      return;
    }

    this.albumElementService.getAlbumElementById(albumId, id).subscribe({
      next: (element) => {
        this.albumElement = {
          name: element.name,
          description: element.description,
          albumId: element.album.id,
          photo: null, // Photo is undefined in the GET response
        };
        this.albumId = element.album.id; // Store albumId for later use
        this.previewUrl = element.imageUrl; // Use the image URL from the response
        this.startAlbumElement = { ...this.albumElement };
      },
      error: (err) => {
        this.errorMessage = `Failed to load album element data: ${err.status}`;
        console.error('Error loading album element data:', err);
      },
    });
  }



  onFileChange(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.albumElement.photo = file;
      const reader = new FileReader();
      reader.onload = () => this.previewUrl = reader.result;
      reader.readAsDataURL(file);
    }
  }

  onSubmit(): void {
    const formData = new FormData();

    // Додаємо лише змінені значення до formData
    if (this.startAlbumElement.name !== this.albumElement.name) {
      formData.append('name', this.albumElement.name);
    }
    if (this.startAlbumElement.description !== this.albumElement.description) {
      formData.append('description', this.albumElement.description ?? '');
    }
    if (this.albumElement.photo) {
      formData.append('photo', this.albumElement.photo);
    }

    if (this.isEditMode && this.elementId && this.albumId) {
      // Update mode
      this.albumElementService.updateAlbumElement(this.albumId, this.elementId, formData).subscribe({
        next: () => this.goBack(),
        error: (err) => {
          console.error('Update failed:', err);
          this.errorMessage = `Failed to update element: ${err.status}`;
        },
      });
    } else if (this.albumId) {
      // Create mode
      this.albumElementService.addAlbumElement(this.albumId, formData).subscribe({
        next: () => this.goBack(),
        error: (err) => {
          console.error('Adding failed:', err);
          this.errorMessage = `Failed to add element: ${err.status}`;
        },
      });
    } else {
      this.errorMessage = 'Album ID is required';
    }
  }


  goBack(): void {
    this.location.back();
    console.log('Redirected back');
  }
}
