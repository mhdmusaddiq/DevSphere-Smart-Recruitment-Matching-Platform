import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

import { SessionService } from '../auth/session.service';
import { TokenStore } from '../auth/token-store.service';
import { safeInternalReturnUrl } from '../auth/safe-return-url';
import { TransportErrorPresenter } from '../errors/transport-error-presenter.service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const tokenStore = inject(TokenStore);
  const session = inject(SessionService);
  const router = inject(Router);
  const errors = inject(TransportErrorPresenter);
  const token = tokenStore.get();
  const authenticatedRequest = token
    ? request.clone({
        setHeaders: { Authorization: `Bearer ${token}` }
      })
    : request;

  return next(authenticatedRequest).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && token) {
        session.clear();
        const returnUrl = safeInternalReturnUrl(router.url);
        void router.navigate(['/login'], {
          queryParams: returnUrl === '/login' ? {} : { returnUrl }
        });
      } else if (error.status === 403) {
        void router.navigate(['/403']);
      } else if (error.status === 429) {
        errors.present({
          kind: 'throttled',
          message: 'Too many requests. Please wait before trying again.',
          retryAfterSeconds: parseRetryAfter(error.headers.get('Retry-After'))
        });
      } else if (error.status === 0 || error.status >= 500) {
        errors.present({
          kind: error.status === 0 ? 'network' : 'server',
          message: error.status === 0
            ? 'The service could not be reached.'
            : 'The service is temporarily unavailable.',
          retryAfterSeconds: null
        });
      }

      return throwError(() => error);
    })
  );
};

function parseRetryAfter(value: string | null): number | null {
  if (!value) {
    return null;
  }

  const seconds = Number(value);

  if (Number.isFinite(seconds) && seconds >= 0) {
    return Math.ceil(seconds);
  }

  const retryDate = Date.parse(value);

  return Number.isNaN(retryDate)
    ? null
    : Math.max(0, Math.ceil((retryDate - Date.now()) / 1000));
}
