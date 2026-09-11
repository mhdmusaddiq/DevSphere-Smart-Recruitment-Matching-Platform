import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs';

import { PublicRole } from '../auth/auth.models';
import { SessionService } from '../auth/session.service';
import { clearInvalidSession, loginRedirect } from './guard-helpers';

export const roleGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const session = inject(SessionService);
  const roles = (route.data['roles'] ?? []) as PublicRole[];

  return session.ensureHydrated().pipe(
    map(user => {
      if (!user) {
        return loginRedirect(router, state.url);
      }

      if (user.accountState !== 'Active') {
        return user.accountState === 'PendingEmailVerification'
          ? router.createUrlTree(['/verify-email'])
          : clearInvalidSession(session, router, state.url);
      }

      return roles.includes(user.role)
        ? true
        : router.createUrlTree(['/403']);
    })
  );
};
