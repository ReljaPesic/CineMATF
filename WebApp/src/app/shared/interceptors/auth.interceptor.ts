import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandlerFn,
  HttpInterceptorFn,
  HttpRequest,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { BehaviorSubject, Observable, catchError, filter, switchMap, take, throwError } from 'rxjs';

import { environment } from '../../../environments/environment';
import { AuthService } from '../../auth/services/auth.service';

const apiHosts = Object.values(environment.api);

let isRefreshing = false;
const refreshedAccessTokenSubject = new BehaviorSubject<string | null>(null);

const addToken = (request: HttpRequest<unknown>, token: string): HttpRequest<unknown> =>
  request.clone({ setHeaders: { Authorization: `Bearer ${token}` } });

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const authService = inject(AuthService);

  const isOurApi = apiHosts.some((base) => request.url.startsWith(base));
  const token = authService.token;

  const outgoing = token && isOurApi ? addToken(request, token) : request;

  return next(outgoing).pipe(
    catchError((error) => {
      const isAuthCall = request.url.includes('/Auth/');
      if (error instanceof HttpErrorResponse && error.status === 401 && isOurApi && !isAuthCall) {
        return handle401Error(request, next, authService);
      }
      return throwError(() => error);
    }),
  );
};

function handle401Error(
  request: HttpRequest<unknown>,
  next: HttpHandlerFn,
  authService: AuthService,
): Observable<HttpEvent<unknown>> {
  if (!isRefreshing) {
    isRefreshing = true;
    refreshedAccessTokenSubject.next(null);

    return authService.refreshToken().pipe(
      switchMap((accessToken: string | null) => {
        isRefreshing = false;

        if (accessToken === null) {
          return throwError(() => new Error('Refresh token flow failed'));
        }

        refreshedAccessTokenSubject.next(accessToken);
        return next(addToken(request, accessToken));
      }),
      catchError((error) => {
        isRefreshing = false;
        return throwError(() => error);
      }),
    );
  }

  return refreshedAccessTokenSubject.pipe(
    filter((token: string | null): token is string => token !== null),
    take(1),
    switchMap((accessToken: string) => next(addToken(request, accessToken))),
  );
}
