import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { ReservationRoutingModule } from './reservation-routing.module';
import { ReservationListComponent } from './components/reservation-list/reservation-list.component';
import { ReservationDetailComponent } from './components/reservation-detail/reservation-detail.component';
import { ReservationFormComponent } from './components/reservation-form/reservation-form.component';
import { MoviePosterComponent } from '../movie/components/movie-poster/movie-poster.component';

@NgModule({
  declarations: [ReservationListComponent, ReservationDetailComponent, ReservationFormComponent],
  imports: [
    CommonModule,
    FormsModule,
    ReservationRoutingModule,
    MoviePosterComponent,
  ],
})
export class ReservationModule {}
