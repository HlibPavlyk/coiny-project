import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import {TailwindTestComponent} from "./tailwind-test/tailwind-test.component";

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, TailwindTestComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'CoinyProject.Client';
}
