import { Routes } from '@angular/router';

export const routes: Routes = [
  // Land on the movies list by default
  { path: '', redirectTo: 'movies', pathMatch: 'full' },

  {
    path: 'movies',
    loadChildren: () => import('./movies/movies.module').then((m) => m.MoviesModule),
  },

  // Unknown URL -> back to the list.
  { path: '**', redirectTo: 'movies' },
];
