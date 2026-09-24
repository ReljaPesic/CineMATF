import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { AuthService } from '../../auth/services/auth.service';

// Route is for SuperAdmin only. Signed-out users go to /login; signed-in
// non-SuperAdmins are bounced to the screenings list.
export const superAdminGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isLoggedIn()) {
    return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
  }

  if (!auth.isSuperAdmin()) {
    return router.createUrlTree(['/screenings']);
  }

  return true;
};
