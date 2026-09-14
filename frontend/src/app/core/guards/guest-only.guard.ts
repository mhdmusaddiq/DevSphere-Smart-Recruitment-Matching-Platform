import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs';

import { PublicRole } from '../auth/auth.models';
import { SessionService } from '../auth/session.service';
import { TokenStore } from '../auth/token-store.service';

const ROLE_HOME: Record<PublicRole, string> = {
  JobSeeker: '/seeker/dashboard',
  Employer: '/employer/dashboard',
  Admin: '/admin/dashboard'
};

export const guestOnlyGuard: CanActivateFn = () => {
  const router = inject(Router);
  const tokenStore = inject(TokenStore);
  const session = inject(SessionService);

  if (!tokenStore.hasToken()) {
    return true;
  }

  return session.ensureHydrated().pipe(
    map(user => {
      if (!user) {
        return true;
      }

      if (user.accountState === 'PendingEmailVerification') {
        return router.createUrlTree(['/verify-email']);
      }

      if (user.accountState === 'Disabled') {
        session.clear();
        return true;
      }

      return router.createUrlTree([ROLE_HOME[user.role]]);
    })
  );
};
