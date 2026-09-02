import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { MoviesRoutingModule } from './movies-routing.module';
import { MovieListComponent } from './components/movie-list/movie-list.component';
import { MovieDetailComponent } from './components/movie-detail/movie-detail.component';
import { MoviePosterComponent } from './components/movie-poster/movie-poster.component';
import { MovieFormComponent } from './components/movie-form/movie-form.component';

// Component for movie features that are displayed to user

@NgModule({
  declarations: [
    MovieListComponent,
    MovieDetailComponent,
    MovieFormComponent,
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MoviesRoutingModule,
    MoviePosterComponent,
  ],
})
export class MoviesModule {}
