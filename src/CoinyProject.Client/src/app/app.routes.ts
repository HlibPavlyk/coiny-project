import { Routes } from '@angular/router';
import {InfoComponent} from "./info/info.component";
import {AlbumComponent} from "./album/album.component";
import {LoginComponent} from "./login/login.component";
import {RegisterComponent} from "./register/register.component";
import {AlbumFormComponent} from "./album/album-form/album-form.component";
import {authGuard} from "./guards/auth.guard";
import {ProfileComponent} from "./profile/profile.component";
import {AlbumViewComponent} from "./album/album-view/album-view.component";
import {AlbumElementViewComponent} from "./album-element/album-element-view/album-element-view.component";
import {AlbumElementFormComponent} from "./album-element/album-element-form/album-element-form.component";

export const routes: Routes = [
  { path: '', redirectTo: 'albums', pathMatch: 'full' },
  { path: 'albums', component: AlbumComponent },
  { path: 'info', component: InfoComponent },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  {
    path: 'album-create',
    component: AlbumFormComponent,
    canActivate: [authGuard],
    data: { roles: ['User'] }
  },
  {
    path: 'album-edit/:id',
    component: AlbumFormComponent,
    canActivate: [authGuard],
    data: { roles: ['User'] }
  },
  {
    path: 'profile',
    component: ProfileComponent,
    canActivate: [authGuard],
    data: { roles: ['User'], my: true }
  },
  {
    path: 'profile/:id',
    component: ProfileComponent
  },
  {
    path: 'album/:id',
    component: AlbumViewComponent,
    canActivate: [authGuard],
    data: { roles: ['User'] }
  },
  {
    path: 'album-element/:id',
    component: AlbumElementViewComponent,
    canActivate: [authGuard],
    data: { roles: ['User'] }
  },
  {
    path: 'album-element-create/:id',
    component: AlbumElementFormComponent,
    canActivate: [authGuard],
    data: { roles: ['User'] }
  },
  {
    path: 'album-element-edit/:id',
    component: AlbumElementFormComponent,
    canActivate: [authGuard],
    data: { roles: ['User'] }
  },

]
