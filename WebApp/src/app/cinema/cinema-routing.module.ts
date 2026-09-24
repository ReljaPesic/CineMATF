import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { CinemaListComponent } from './components/cinema-list/cinema-list.component';
import { CinemaFormComponent } from './components/cinema-form/cinema-form.component';
import { CinemaDetailComponent } from './components/cinema-detail/cinema-detail.component';
import { HallSeatsComponent } from './components/hall-seats/hall-seats.component';
import { RegisterCinemaAdminComponent } from './components/register-cinema-admin/register-cinema-admin.component';
import { CinemaAdminListComponent } from './components/cinema-admin-list/cinema-admin-list.component';
import { CinemaAdminEditComponent } from './components/cinema-admin-edit/cinema-admin-edit.component';
import { staffGuard } from '../shared/guards/staff.guard';
import { superAdminGuard } from '../shared/guards/super-admin.guard';

const routes: Routes = [
  { path: '', component: CinemaListComponent, canActivate: [superAdminGuard] },
  { path: 'new', component: CinemaFormComponent, canActivate: [superAdminGuard] },
  { path: 'admins', component: CinemaAdminListComponent, canActivate: [superAdminGuard] },
  { path: 'admins/new', component: RegisterCinemaAdminComponent, canActivate: [superAdminGuard] },
  { path: 'admins/:username/edit', component: CinemaAdminEditComponent, canActivate: [superAdminGuard] },
  { path: ':id/edit', component: CinemaFormComponent, canActivate: [staffGuard] },
  { path: ':id', component: CinemaDetailComponent },
  { path: ':id/halls/:hallId/seats', component: HallSeatsComponent, canActivate: [staffGuard] }
]

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class CinemaRoutingModule {}
