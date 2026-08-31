import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { ScreeningRoutingModule } from './screening-routing.module';
import { ScreeningListComponent } from './components/screening-list/screening-list.component';
import { ScreeningDetailComponent } from './components/screening-detail/screening-detail.component';
import { MoviePosterComponent } from '../movie/components/movie-poster/movie-poster.component';
import { ScreeningFormComponent } from './components/screening-form/screening-form.component';

@NgModule({
  declarations: [
    ScreeningListComponent,
    ScreeningDetailComponent,
    ScreeningFormComponent
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
