import { Routes } from '@angular/router';
import {InfoComponent} from "./info/info.component";
import {AlbumComponent} from "./album/album.component";

export const routes: Routes = [
  //{ path: '', redirectTo: 'albums', pathMatch: 'full' },
  { path: 'albums', component: AlbumComponent },
  { path: 'info', component: InfoComponent },
  /*{
    path: 'employees',
    component: EmployeeComponent,
    canActivate: [authGuard],
    data: { roles: ['Administrator', 'HRManager', 'ProjectManager'] }
  },*/
]
