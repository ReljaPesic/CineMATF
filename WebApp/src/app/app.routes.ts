import { Routes } from '@angular/router';

export const routes: Routes = [
  // Land on the movies list by default
  { path: '', redirectTo: 'movies', pathMatch: 'full' },
  {
    path: 'movies',
    loadChildren: () => import('./movie/movies.module').then((m) => m.MoviesModule),
  },
  {
    path: 'cinemas',
    loadChildren: () => import('./cinema/cinema.module').then((m) => m.CinemaModule),
  },
  {
    path: 'screenings',
    loadChildren: () => import('./screening/screening.module').then((m) => m.ScreeningModule),
  },
  // Unknown URL -> back to the list.
  { path: '**', redirectTo: 'screenings' },
];
