import { Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

import { BrandWordmarkComponent } from '../brand/brand-wordmark.component';
import { EmptyStateVisualComponent } from '../states/empty-state-visual.component';

@Component({
  selector: 'app-route-placeholder',
  standalone: true,
  imports: [BrandWordmarkComponent, EmptyStateVisualComponent, RouterLink],
  template: `
    <main class="placeholder-shell">
      <apt-brand-wordmark [showTagline]="true" />
      <apt-empty-state-visual />
      <p class="eyebrow">{{ pageId }}</p>
      <h1>{{ pageName }}</h1>
      <p>This route is reserved for its owning feature team.</p>
      <a routerLink="/jobs">Return to jobs</a>
    </main>
  `,
  styles: [`
    :host { display: grid; min-height: 100vh; place-items: center; padding: var(--space-6); }
    .placeholder-shell { display: grid; width: min(38rem, 100%); justify-items: center; gap: var(--space-4); padding: var(--space-8); text-align: center; background: var(--color-surface); border: 1px solid var(--color-border); border-radius: var(--radius-xl); box-shadow: var(--shadow-md); }
    .eyebrow { margin: 0; color: var(--color-brand-violet); font: var(--font-label); letter-spacing: .1em; }
    h1, p { margin: 0; }
    a { color: var(--color-brand-violet); font-weight: 700; }
  `]
})
export class RoutePlaceholderComponent {
  private readonly route = inject(ActivatedRoute);

  readonly pageId = this.route.snapshot.data['pageId'] as string ?? 'Shared Core';
  readonly pageName = this.route.snapshot.data['pageName'] as string ?? 'Coming soon';
}
