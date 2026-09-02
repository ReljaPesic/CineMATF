import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { ReservationListComponent } from './components/reservation-list/reservation-list.component';
import { ReservationDetailComponent } from './components/reservation-detail/reservation-detail.component';
import { ReservationFormComponent } from './components/reservation-form/reservation-form.component';

const routes: Routes = [
  { path: '', component: ReservationListComponent },
  { path: 'new', component: ReservationFormComponent },
  { path: ':id', component: ReservationDetailComponent },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ReservationRoutingModule {}
