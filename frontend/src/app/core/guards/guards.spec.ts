import { TestBed } from '@angular/core/testing';
import {
  ActivatedRouteSnapshot,
  CanActivateFn,
  Router,
  RouterStateSnapshot,
  UrlTree,
  provideRouter
} from '@angular/router';
import { Observable, firstValueFrom, isObservable, of } from 'rxjs';

import { AuthenticatedUser } from '../auth/auth.models';
import { SessionService } from '../auth/session.service';
import { TokenStore } from '../auth/token-store.service';
import { authGuard } from './auth.guard';
import { guestOnlyGuard } from './guest-only.guard';
import { roleGuard } from './role.guard';
import { verifiedAccountGuard } from './verified-account.guard';

describe('Shared Core guards', () => {
  let router: Router;
  let tokenStore: jasmine.SpyObj<TokenStore>;
  let session: jasmine.SpyObj<SessionService>;

  const activeSeeker: AuthenticatedUser = {
    userId: 'user-1',
    email: 'seeker@example.test',
    displayName: 'Seeker One',
    role: 'JobSeeker',
    accountState: 'Active',
    emailVerified: true
  };

  beforeEach(() => {
    tokenStore = jasmine.createSpyObj<TokenStore>('TokenStore', [
      'hasToken', 'clear'
    ]);
    session = jasmine.createSpyObj<SessionService>('SessionService', [
      'ensureHydrated', 'clear'
    ]);
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        { provide: TokenStore, useValue: tokenStore },
        { provide: SessionService, useValue: session }
      ]
    });
    router = TestBed.inject(Router);
  });

  it('sends a no-token protected request to login with returnUrl', async () => {
    tokenStore.hasToken.and.returnValue(false);

    const result = await runGuard(authGuard, {}, '/seeker/profile');

    expect(url(result)).toBe('/login?returnUrl=%2Fseeker%2Fprofile');
  });

  it('routes pending accounts to verification', async () => {
    session.ensureHydrated.and.returnValue(of({
      ...activeSeeker,
      accountState: 'PendingEmailVerification',
      emailVerified: false
    }));

    const result = await runGuard(
      verifiedAccountGuard,
      {},
      '/seeker/profile'
    );

    expect(url(result)).toBe('/verify-email');
  });

  it('routes a valid wrong-role session to 403 without clearing it', async () => {
    session.ensureHydrated.and.returnValue(of(activeSeeker));

    const result = await runGuard(
      roleGuard,
      { data: { roles: ['Employer'] } },
      '/employer/dashboard'
    );

    expect(url(result)).toBe('/403');
    expect(session.clear).not.toHaveBeenCalled();
  });

  it('allows the correct active role', async () => {
    session.ensureHydrated.and.returnValue(of(activeSeeker));

    const result = await runGuard(
      roleGuard,
      { data: { roles: ['JobSeeker'] } },
      '/seeker/dashboard'
    );

    expect(result).toBeTrue();
  });

  it('redirects an authenticated guest away from login to role home', async () => {
    tokenStore.hasToken.and.returnValue(true);
    session.ensureHydrated.and.returnValue(of(activeSeeker));

    const result = await runGuard(guestOnlyGuard, {}, '/login');

    expect(url(result)).toBe('/seeker/dashboard');
  });

  function runGuard(
    guard: CanActivateFn,
    route: Partial<ActivatedRouteSnapshot>,
    requestedUrl: string
  ): Promise<boolean | UrlTree> {
    const result = TestBed.runInInjectionContext(() => guard(
      route as ActivatedRouteSnapshot,
      { url: requestedUrl } as RouterStateSnapshot
    ));

    if (isObservable(result)) {
      return firstValueFrom(result as Observable<boolean | UrlTree>);
    }

    return Promise.resolve(result as boolean | UrlTree);
  }

  function url(value: boolean | UrlTree): string {
    expect(value instanceof UrlTree).toBeTrue();
    return router.serializeUrl(value as UrlTree);
  }
});
