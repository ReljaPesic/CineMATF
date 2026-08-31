import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { ScreeningRoutingModule } from './screening-routing.module';
import { ScreeningListComponent } from './components/screening-list/screening-list.component';
import { ScreeningDetailComponent } from './components/screening-detail/screening-detail.component';
import { MoviePosterComponent } from '../movies/components/movie-poster/movie-poster.component';

@NgModule({
  declarations: [
    ScreeningListComponent,
    ScreeningDetailComponent,
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    ScreeningRoutingModule,
    MoviePosterComponent,
  ],
})
export class ScreeningModule {}
