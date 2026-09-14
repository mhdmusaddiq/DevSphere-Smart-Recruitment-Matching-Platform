import { Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

import { AppIconComponent, AppIconName } from '../icons/app-icon.component';
import { BrandWordmarkComponent } from '../brand/brand-wordmark.component';

@Component({
  selector: 'app-system-status-page',
  standalone: true,
  imports: [AppIconComponent, BrandWordmarkComponent, RouterLink],
  template: `
    <main class="system-shell">
      <apt-brand-wordmark />
      <apt-app-icon [name]="icon" [size]="40" />
      <p class="code">{{ code }}</p>
      <h1>{{ title }}</h1>
      <p>{{ message }}</p>
      <a routerLink="/jobs">Browse jobs</a>
    </main>
  `,
  styles: [`
    :host { display: grid; min-height: 100vh; place-items: center; padding: var(--space-6); }
    .system-shell { display: grid; width: min(36rem, 100%); justify-items: center; gap: var(--space-4); padding: var(--space-8); text-align: center; background: var(--color-surface); border: 1px solid var(--color-border); border-radius: var(--radius-xl); }
    .code { margin: 0; color: var(--color-brand-violet); font: var(--font-label); letter-spacing: .12em; }
    h1, p { margin: 0; }
    a { color: var(--color-brand-violet); font-weight: 700; }
  `]
})
export class SystemStatusPageComponent {
  private readonly route = inject(ActivatedRoute);

  readonly code = this.route.snapshot.data['code'] as string ?? '';
  readonly title = this.route.snapshot.data['title'] as string ?? '';
  readonly message = this.route.snapshot.data['message'] as string ?? '';
  readonly icon: AppIconName = this.code === '403' ? 'lock' : 'search';
}
