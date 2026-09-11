import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, map, switchMap, tap } from 'rxjs';

import { API_BASE_URL, apiUrl } from '../config/api-base-url';
import {
  AuthenticatedUser,
  LoginRequest,
  LoginResponse
} from './auth.models';
import { SessionService } from './session.service';
import { TokenStore } from './token-store.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly baseUrl = inject(API_BASE_URL);
  private readonly tokenStore = inject(TokenStore);
  private readonly session = inject(SessionService);

  login(request: LoginRequest): Observable<AuthenticatedUser> {
    return this.http
      .post<LoginResponse>(apiUrl(this.baseUrl, '/auth/login'), request)
      .pipe(
        tap(response => this.tokenStore.set(response.token)),
        switchMap(() => this.session.hydrate())
      );
  }

  logout(): Observable<void> {
    return this.http
      .post<unknown>(apiUrl(this.baseUrl, '/auth/logout'), {})
      .pipe(
        tap(() => {
          this.session.clear();
          void this.router.navigate(['/login']);
        }),
        map(() => undefined)
      );
  }
}
