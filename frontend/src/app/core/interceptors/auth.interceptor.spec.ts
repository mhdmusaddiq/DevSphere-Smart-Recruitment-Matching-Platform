import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';

import { SessionService } from '../auth/session.service';
import { TokenStore } from '../auth/token-store.service';
import { TransportErrorPresenter } from '../errors/transport-error-presenter.service';
import { authInterceptor } from './auth.interceptor';

describe('authInterceptor', () => {
  let http: HttpClient;
  let controller: HttpTestingController;
  let tokenStore: jasmine.SpyObj<TokenStore>;
  let session: jasmine.SpyObj<SessionService>;
  let router: jasmine.SpyObj<Router>;
  let presenter: TransportErrorPresenter;

  beforeEach(() => {
    tokenStore = jasmine.createSpyObj<TokenStore>('TokenStore', ['get']);
    session = jasmine.createSpyObj<SessionService>('SessionService', ['clear']);
    router = jasmine.createSpyObj<Router>('Router', ['navigate'], { url: '/seeker/profile' });
    router.navigate.and.resolveTo(true);
    tokenStore.get.and.returnValue('access-token');

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        { provide: TokenStore, useValue: tokenStore },
        { provide: SessionService, useValue: session },
        { provide: Router, useValue: router }
      ]
    });
    http = TestBed.inject(HttpClient);
    controller = TestBed.inject(HttpTestingController);
    presenter = TestBed.inject(TransportErrorPresenter);
  });

  afterEach(() => controller.verify());

  it('attaches the bearer token', () => {
    http.get('/api/value').subscribe();
    const request = controller.expectOne('/api/value');
    expect(request.request.headers.get('Authorization')).toBe('Bearer access-token');
    request.flush({});
  });

  it('clears session and safely routes to login on 401', () => {
    http.get('/api/private').subscribe({ error: () => undefined });
    controller.expectOne('/api/private').flush(null, {
      status: 401,
      statusText: 'Unauthorized'
    });

    expect(session.clear).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/login'], {
      queryParams: { returnUrl: '/seeker/profile' }
    });
  });

  it('preserves session and routes to 403 on forbidden', () => {
    http.get('/api/private').subscribe({ error: () => undefined });
    controller.expectOne('/api/private').flush(null, {
      status: 403,
      statusText: 'Forbidden'
    });

    expect(session.clear).not.toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/403']);
  });

  [404, 409].forEach(status => {
    it(`returns ${status} to the caller without global navigation`, () => {
      let actualStatus = 0;
      http.get('/api/resource').subscribe({
        error: error => actualStatus = error.status
      });
      controller.expectOne('/api/resource').flush(null, {
        status,
        statusText: 'Caller owned'
      });

      expect(actualStatus).toBe(status);
      expect(router.navigate).not.toHaveBeenCalled();
      expect(presenter.failure()).toBeNull();
    });
  });

  it('preserves Retry-After metadata on 429 without redirecting', () => {
    http.get('/api/throttled').subscribe({ error: () => undefined });
    controller.expectOne('/api/throttled').flush(null, {
      status: 429,
      statusText: 'Too Many Requests',
      headers: { 'Retry-After': '12' }
    });

    expect(presenter.failure()).toEqual(jasmine.objectContaining({
      kind: 'throttled',
      retryAfterSeconds: 12
    }));
    expect(router.navigate).not.toHaveBeenCalled();
    expect(session.clear).not.toHaveBeenCalled();
  });
});
