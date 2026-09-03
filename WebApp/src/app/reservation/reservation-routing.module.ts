import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { ReservationListComponent } from './components/reservation-list/reservation-list.component';
import { ReservationDetailComponent } from './components/reservation-detail/reservation-detail.component';
import { ReservationFormComponent } from './components/reservation-form/reservation-form.component';
import { authGuard } from '../shared/guards/auth.guard';

const routes: Routes = [
  { path: '', component: ReservationListComponent, canActivate: [authGuard] },
  { path: 'new', component: ReservationFormComponent, canActivate: [authGuard] },
  { path: ':id', component: ReservationDetailComponent, canActivate: [authGuard] },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ReservationRoutingModule {}
