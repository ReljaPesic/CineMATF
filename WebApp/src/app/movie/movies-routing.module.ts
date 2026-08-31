import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { MovieListComponent } from './components/movie-list/movie-list.component';
import { MovieDetailComponent } from './components/movie-detail/movie-detail.component';
import { MovieFormComponent } from './components/movie-form/movie-form.component';


const routes: Routes = [
  { path: '', component: MovieListComponent },
  { path: 'new', component: MovieFormComponent },
  { path: ':id', component: MovieDetailComponent },
  { path: ':id/edit', component: MovieFormComponent },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class MoviesRoutingModule {}
