import {Component, Input} from '@angular/core';
import {RouterLink} from "@angular/router";

@Component({
  selector: 'app-item-no-found',
  standalone: true,
  imports: [
    RouterLink
  ],
  templateUrl: './item-no-found.component.html',
  styleUrl: './item-no-found.component.css'
})
export class ItemNoFoundComponent {

}
