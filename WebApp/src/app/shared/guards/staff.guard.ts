import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { AuthService } from '../../auth/services/auth.service';

// Route is for SuperAdmin or CinemaAdmin. Signed-out users go to /login; signed-in
// non-staff are bounced to the screenings list.
export const staffGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isLoggedIn()) {
    return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
  }

  if (!auth.isStaff()) {
    return router.createUrlTree(['/screenings']);
  }

  return true;
};
