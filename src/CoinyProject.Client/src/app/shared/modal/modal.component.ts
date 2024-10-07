import {Component, EventEmitter, Output, Input} from '@angular/core';
import {RouterOutlet} from "@angular/router";
import {AlbumsViewComponent} from "../../album/albums-view/albums-view.component";
import {AlbumComponent} from "../../album/album.component";
import {TopBarComponent} from "../../top-bar/top-bar.component";
import {CommonModule, NgIf} from "@angular/common";
@Component({
  selector: 'app-modal',
  standalone: true,
  imports: [
    NgIf
  ],
  templateUrl: './modal.component.html'
})
export class ModalComponent  {
  @Input() title: string = 'Confirm Action';
  @Input() message: string = 'Are you sure you want to proceed?';
  @Input() isOpen: boolean = false;
  @Input() errorMessage = '';

  @Output() onConfirm: EventEmitter<void> = new EventEmitter<void>();
  @Output() onCancel: EventEmitter<void> = new EventEmitter<void>();

  confirm(): void {
    this.onConfirm.emit();
  }

  cancel(): void {
    this.onCancel.emit();
  }
}
