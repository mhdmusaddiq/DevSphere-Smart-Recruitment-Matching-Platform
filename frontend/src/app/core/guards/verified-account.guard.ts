import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs';

import { SessionService } from '../auth/session.service';
import { clearInvalidSession, loginRedirect } from './guard-helpers';

export const verifiedAccountGuard: CanActivateFn = (_route, state) => {
  const router = inject(Router);
  const session = inject(SessionService);

  return session.ensureHydrated().pipe(
    map(user => {
      if (!user) {
        return loginRedirect(router, state.url);
      }

      if (user.accountState === 'Disabled') {
        return clearInvalidSession(session, router, state.url);
      }

      return user.accountState === 'PendingEmailVerification' ||
        !user.emailVerified
        ? router.createUrlTree(['/verify-email'])
        : true;
    })
  );
};
