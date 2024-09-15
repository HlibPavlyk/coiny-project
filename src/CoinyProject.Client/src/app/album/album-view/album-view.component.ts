import {Component, OnInit} from '@angular/core';
import {FormsModule} from "@angular/forms";
import {NgClass, NgForOf, NgOptimizedImage} from "@angular/common";
import {AlbumGetDto} from "./album-get.model";

@Component({
  selector: 'app-album-view',
  standalone: true,
  imports: [
    FormsModule,
    NgForOf,
    NgOptimizedImage,
    NgClass
  ],
  templateUrl: './album-view.component.html',
  styleUrl: './album-view.component.css'
})
export class AlbumViewComponent implements OnInit {
  albums: AlbumGetDto[] = [
    {
      id: '1',
      name: 'Album 1',
      description: 'This is the first album-view description.',
      rate: 5,
      imagesUrls: ['photos/798ca71f-c09b-a237-1b53-02e7cf89e91a.png'],
    },
    {
      id: '2',
      name: 'Album 2',
      description: 'This is the second album-view descripііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііііon.',
      rate: 4,
      imagesUrls: ['https://via.placeholder.com/150'],
    },{
      id: '1',
      name: 'Album 1',
      description: 'This is the first album-view descriptionфівввввввввввввввввіфвфііііііііііііііііііііііііііііііііііііііііііііііііі.',
      rate: 5,
      imagesUrls: ['photos/798ca71f-c09b-a237-1b53-02e7cf89e91a.png'],
    },{
      id: '1',
      name: 'Album 1',
      description: 'This is the first album-view descriptionфівввввввввввввввввввввввввввввввввввв.',
      rate: 5,
      imagesUrls: ['photos/798ca71f-c09b-a237-1b53-02e7cf89e91a.png'],
    },
    // Додаємо більше альбомів
  ];

  filteredAlbums: AlbumGetDto[] = [];
  searchQuery: string = '';
  sortType: string = 'popularity'; // Default sorting by popularity

  ngOnInit() {
    this.filteredAlbums = [...this.albums]; // Ініціалізація зі всіма альбомами
  }

  setSort(sortType: string) {
    this.sortType = sortType;
    this.sortAlbums();
  }

  searchAlbums() {
    // Фільтрація альбомів за пошуковим запитом
    this.filteredAlbums = this.albums.filter((album) =>
      album.name.toLowerCase().includes(this.searchQuery.toLowerCase()) ||
      (album.description && album.description.toLowerCase().includes(this.searchQuery.toLowerCase()))
    );
    this.sortAlbums(); // Оновлюємо сортування після пошуку
  }

  sortAlbums() {
    // Сортування альбомів залежно від обраного типу сортування
    if (this.sortType === 'popularity') {
      this.filteredAlbums.sort((a, b) => b.rate - a.rate);
    } else if (this.sortType === 'newest') {
      // Якщо у вас є дата релізу альбому, можна реалізувати сортування за новизною
      // this.filteredAlbums.sort((a, b) => new Date(b.releaseDate).getTime() - new Date(a.releaseDate).getTime());
    }
  }
}
