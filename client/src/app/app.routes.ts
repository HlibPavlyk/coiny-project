import { Routes } from '@angular/router';
import {InfoComponent} from "./features/info/info.component";
import {AlbumComponent} from "@features/albums/components/album.component";
import {LoginComponent} from "./features/login/login.component";
import {RegisterComponent} from "./features/register/register.component";
import {AlbumFormComponent} from "@features/albums/components/album-form/album-form.component";
import {authGuard} from "./core/guards/auth.guard";
import {ProfileComponent} from "./features/profile/profile.component";
import {AlbumViewComponent} from "@features/albums/components/album-view/album-view.component";
import {AlbumElementViewComponent} from "./features/album-element/album-element-view/album-element-view.component";
import {AlbumElementFormComponent} from "./features/album-element/album-element-form/album-element-form.component";

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
