import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { CinemaRoutingModule } from './cinema-routing.module';
import { CinemaListComponent } from './components/cinema-list/cinema-list.component';
import { CinemaFormComponent } from './components/cinema-form/cinema-form.component';
import { CinemaDetailComponent } from './components/cinema-detail/cinema-detail.component';
import { HallSeatsComponent } from './components/hall-seats/hall-seats.component';

@NgModule({
  declarations: [
    CinemaListComponent,
    CinemaFormComponent,
    CinemaDetailComponent,
    HallSeatsComponent,
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    CinemaRoutingModule,
  ],
})
export class CinemaModule {}
