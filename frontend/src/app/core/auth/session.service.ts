import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, catchError, finalize, of, shareReplay, tap } from 'rxjs';

import { API_BASE_URL, apiUrl } from '../config/api-base-url';
import { AuthenticatedUser } from './auth.models';
import { TokenStore } from './token-store.service';

@Injectable({ providedIn: 'root' })
export class SessionService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);
  private readonly tokenStore = inject(TokenStore);
  private readonly userState = signal<AuthenticatedUser | null>(null);
  private hydrationRequest: Observable<AuthenticatedUser | null> | null = null;

  readonly user = this.userState.asReadonly();
  readonly isAuthenticated = computed(() => this.userState() !== null);

  hydrate(): Observable<AuthenticatedUser> {
    return this.http
      .get<AuthenticatedUser>(apiUrl(this.baseUrl, '/auth/me'))
      .pipe(tap(user => this.userState.set(user)));
  }

  ensureHydrated(): Observable<AuthenticatedUser | null> {
    const currentUser = this.userState();

    if (currentUser) {
      return of(currentUser);
    }

    if (!this.tokenStore.hasToken()) {
      return of(null);
    }

    if (!this.hydrationRequest) {
      this.hydrationRequest = this.hydrate().pipe(
        catchError(() => of(null)),
        finalize(() => this.hydrationRequest = null),
        shareReplay({ bufferSize: 1, refCount: false })
      );
    }

    return this.hydrationRequest;
  }

  clear(): void {
    this.tokenStore.clear();
    this.userState.set(null);
    this.hydrationRequest = null;
  }
}
