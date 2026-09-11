import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of } from 'rxjs';

import { API_BASE_URL } from '../config/api-base-url';
import { AuthService } from './auth.service';
import { SessionService } from './session.service';
import { TokenStore } from './token-store.service';

describe('AuthService', () => {
  let auth: AuthService;
  let http: HttpTestingController;
  let tokenStore: jasmine.SpyObj<TokenStore>;
  let session: jasmine.SpyObj<SessionService>;
  let router: jasmine.SpyObj<Router>;

  const user = {
    userId: 'user-1', email: 'user@example.test', displayName: 'User One',
    role: 'JobSeeker' as const, accountState: 'Active' as const,
    emailVerified: true
  };

  beforeEach(() => {
    tokenStore = jasmine.createSpyObj<TokenStore>('TokenStore', ['set']);
    session = jasmine.createSpyObj<SessionService>('SessionService', ['hydrate', 'clear']);
    router = jasmine.createSpyObj<Router>('Router', ['navigate']);
    session.hydrate.and.returnValue(of(user));

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: 'https://api.example.test/api' },
        { provide: TokenStore, useValue: tokenStore },
        { provide: SessionService, useValue: session },
        { provide: Router, useValue: router }
      ]
    });
    auth = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('stores login token then hydrates /auth/me truth', () => {
    auth.login({ email: 'user@example.test', password: 'secret' })
      .subscribe(result => expect(result).toEqual(user));

    http.expectOne('https://api.example.test/api/auth/login').flush({
      token: 'token', email: user.email, displayName: user.displayName
    });

    expect(tokenStore.set).toHaveBeenCalledWith('token');
    expect(session.hydrate).toHaveBeenCalled();
  });

  it('clears local session only after successful server logout', () => {
    auth.logout().subscribe();
    expect(session.clear).not.toHaveBeenCalled();

    http.expectOne('https://api.example.test/api/auth/logout').flush({});

    expect(session.clear).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });
});
