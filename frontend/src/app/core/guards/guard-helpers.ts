import { Router, UrlTree } from '@angular/router';

import { SessionService } from '../auth/session.service';
import { safeInternalReturnUrl } from '../auth/safe-return-url';

export function loginRedirect(router: Router, requestedUrl: string): UrlTree {
  const returnUrl = safeInternalReturnUrl(requestedUrl);

  return router.createUrlTree(['/login'], {
    queryParams: returnUrl === '/login' ? {} : { returnUrl }
  });
}

export function clearInvalidSession(
  session: SessionService,
  router: Router,
  requestedUrl: string
): UrlTree {
  session.clear();
  return loginRedirect(router, requestedUrl);
}
