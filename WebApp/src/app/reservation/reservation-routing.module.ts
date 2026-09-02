import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { ReservationListComponent } from './components/reservation-list/reservation-list.component';
import { ReservationDetailComponent } from './components/reservation-detail/reservation-detail.component';
import { ReservationFormComponent } from './components/reservation-form/reservation-form.component';
import { adminGuard } from '../shared/guards/admin.guard';
import { authGuard } from '../shared/guards/auth.guard';

const routes: Routes = [
  // The full list is an admin view; booking / viewing one reservation is for
  // any signed-in user.
  { path: '', component: ReservationListComponent, canActivate: [adminGuard] },
  { path: 'new', component: ReservationFormComponent, canActivate: [authGuard] },
  { path: ':id', component: ReservationDetailComponent, canActivate: [authGuard] },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ReservationRoutingModule {}
