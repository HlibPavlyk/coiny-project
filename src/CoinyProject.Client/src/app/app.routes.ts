import { Routes } from '@angular/router';
import {InfoComponent} from "./info/info.component";
import {AlbumComponent} from "./album/album.component";
import {LoginComponent} from "./login/login.component";
import {RegisterComponent} from "./register/register.component";

export const routes: Routes = [
  //{ path: '', redirectTo: 'albums', pathMatch: 'full' },
  { path: 'albums', component: AlbumComponent },
  { path: 'info', component: InfoComponent },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  /*{
    path: 'employees',
    component: EmployeeComponent,
    canActivate: [authGuard],
    data: { roles: ['Administrator', 'HRManager', 'ProjectManager'] }
  },*/
]
