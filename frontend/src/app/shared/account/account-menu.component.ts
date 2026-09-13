import { Component, computed, inject, input, signal } from '@angular/core';

import { AuthService } from '../../core/auth/auth.service';
import { SessionService } from '../../core/auth/session.service';
import { IdentityAvatarComponent } from '../avatar/identity-avatar.component';
import { AppIconComponent } from '../icons/app-icon.component';

@Component({
  selector: 'apt-account-menu',
  standalone: true,
  imports: [IdentityAvatarComponent, AppIconComponent],
  template: `
    <details class="account-menu">
      <summary aria-label="Open account menu">
        <apt-identity-avatar [displayName]="displayName()" [size]="34" />
        @if (showIdentity()) {
          <span class="identity-copy">
            <strong>{{ displayName() }}</strong>
            <small>{{ roleLabel() }}</small>
          </span>
        }
        <apt-app-icon class="chevron" name="chevron" [size]="15" />
      </summary>

      <div class="menu-panel">
        <div class="account-copy">
          <strong>{{ displayName() }}</strong>
          <span>{{ email() }}</span>
        </div>
        <button type="button" [disabled]="loggingOut()" (click)="logout()">
          <apt-app-icon name="logout" [size]="18" />
          {{ loggingOut() ? 'Signing out…' : 'Sign out' }}
        </button>
      </div>
    </details>
  `,
  styles: [`
    :host { display: inline-flex; min-width: 0; }
    .account-menu { position: relative; }
    summary { min-height: 42px; box-sizing: border-box; display: flex; align-items: center; gap: .55rem; border: 1px solid var(--color-border); border-radius: 999px; background: #fff; padding: .2rem .55rem .2rem .25rem; color: var(--color-text); cursor: pointer; list-style: none; }
    summary::-webkit-details-marker { display: none; }
    summary:focus-visible, button:focus-visible { outline: 3px solid var(--color-brand-violet); outline-offset: 2px; }
    .identity-copy { display: grid; min-width: 0; gap: .05rem; text-align: left; }
    .identity-copy strong { max-width: 150px; overflow: hidden; color: var(--color-text); font-size: .72rem; text-overflow: ellipsis; white-space: nowrap; }
    .identity-copy small { color: var(--color-text-muted); font-size: .62rem; }
    .chevron { transform: rotate(90deg); color: var(--color-text-muted); }
    .menu-panel { position: absolute; top: calc(100% + .5rem); right: 0; z-index: 80; width: min(280px, calc(100vw - 2rem)); box-sizing: border-box; border: 1px solid var(--color-border); border-radius: var(--radius-md); background: #fff; padding: .7rem; box-shadow: var(--shadow-md); }
    .account-copy { display: grid; gap: .18rem; border-bottom: 1px solid var(--color-border); padding: .25rem .3rem .7rem; }
    .account-copy strong, .account-copy span { overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
    .account-copy strong { font-size: .82rem; }
    .account-copy span { color: var(--color-text-muted); font-size: .72rem; }
    button { width: 100%; min-height: 42px; margin-top: .55rem; display: flex; align-items: center; gap: .55rem; border: 0; border-radius: .6rem; background: transparent; padding: .55rem .65rem; color: var(--color-danger); font: inherit; font-size: .82rem; font-weight: 750; cursor: pointer; }
    button:hover { background: var(--color-error-soft); }
    button:disabled { cursor: wait; opacity: .65; }
    @media (max-width: 560px) { .identity-copy { display: none; } summary { padding-right: .4rem; } }
  `]
})
export class AccountMenuComponent {
  private readonly auth = inject(AuthService);
  private readonly session = inject(SessionService);

  readonly showIdentity = input(true);
  readonly loggingOut = signal(false);
  readonly displayName = computed(() =>
    this.session.user()?.displayName?.trim() || 'Account'
  );
  readonly email = computed(() => this.session.user()?.email ?? '');
  readonly roleLabel = computed(() => {
    const role = this.session.user()?.role;
    return role === 'JobSeeker' ? 'Job Seeker' : role ?? 'Signed in';
  });

  logout(): void {
    if (this.loggingOut()) {
      return;
    }

    this.loggingOut.set(true);
    this.auth.logout().subscribe({
      error: () => this.loggingOut.set(false)
    });
  }
}
