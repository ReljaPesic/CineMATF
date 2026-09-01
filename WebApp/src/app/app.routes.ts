import { Routes } from '@angular/router';

import { authGuard } from './shared/guards/auth.guard';

export const routes: Routes = [
  // Land on the movies list by default
  { path: '', redirectTo: 'screenings', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () =>
      import('./auth/components/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'profile',
    loadChildren: () => import('./user/user.module').then((m) => m.UserModule),
    canActivate: [authGuard],
  },
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
