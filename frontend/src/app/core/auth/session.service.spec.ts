import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { API_BASE_URL } from '../config/api-base-url';
import { SessionService } from './session.service';
import { TokenStore } from './token-store.service';

describe('SessionService', () => {
  let session: SessionService;
  let http: HttpTestingController;
  let tokenStore: jasmine.SpyObj<TokenStore>;

  beforeEach(() => {
    tokenStore = jasmine.createSpyObj<TokenStore>('TokenStore', [
      'get', 'set', 'clear', 'hasToken'
    ]);
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: 'https://api.example.test/api' },
        { provide: TokenStore, useValue: tokenStore }
      ]
    });
    session = TestBed.inject(SessionService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('hydrates authoritative identity from /auth/me', () => {
    const user = {
      userId: 'user-1',
      email: 'user@example.test',
      displayName: 'User One',
      role: 'JobSeeker' as const,
      accountState: 'Active' as const,
      emailVerified: true
    };

    session.hydrate().subscribe(result => expect(result).toEqual(user));
    http.expectOne('https://api.example.test/api/auth/me').flush(user);

    expect(session.user()).toEqual(user);
    expect(session.isAuthenticated()).toBeTrue();
  });

  it('does not hydrate when no token exists', () => {
    tokenStore.hasToken.and.returnValue(false);

    session.ensureHydrated().subscribe(user => expect(user).toBeNull());

    http.expectNone('https://api.example.test/api/auth/me');
  });

  it('clears token and in-memory session together', () => {
    session.clear();
    expect(tokenStore.clear).toHaveBeenCalled();
    expect(session.user()).toBeNull();
  });
});
