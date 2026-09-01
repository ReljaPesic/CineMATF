import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';

import { environment } from '../../../environments/environment';
import { AuthService } from '../../auth/services/auth.service';

// Attaches "Authorization: Bearer <token>" to requests aimed at our own APIs
const apiHosts = Object.values(environment.api);
//TODO: add whitelistUrls

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = inject(AuthService).token;
  const isOurApi = apiHosts.some((base) => req.url.startsWith(base));

  if (token && isOurApi) {
    req = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
  }

  return next(req);
};
