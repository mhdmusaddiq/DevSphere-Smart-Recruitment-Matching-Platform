import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs';

import { SessionService } from '../auth/session.service';
import { TokenStore } from '../auth/token-store.service';
import { clearInvalidSession, loginRedirect } from './guard-helpers';

export const authGuard: CanActivateFn = (_route, state) => {
  const router = inject(Router);
  const tokenStore = inject(TokenStore);
  const session = inject(SessionService);

  if (!tokenStore.hasToken()) {
    return loginRedirect(router, state.url);
  }

  return session.ensureHydrated().pipe(
    map(user => {
      if (!user) {
        return loginRedirect(router, state.url);
      }

      return user.accountState === 'Disabled'
        ? clearInvalidSession(session, router, state.url)
        : true;
    })
  );
};
