import { Component, input } from '@angular/core';

@Component({
  selector: 'apt-loading-state',
  standalone: true,
  host: { role: 'status', 'aria-live': 'polite' },
  template: `<span class="spinner" aria-hidden="true"></span><span>{{ message() }}</span>`,
  styles: [`
    :host { display: flex; align-items: center; justify-content: center; gap: var(--space-3); padding: var(--space-6); color: var(--color-text-muted); }
    .spinner { width: 1.25rem; aspect-ratio: 1; border: 2px solid var(--color-border); border-top-color: var(--color-brand-violet); border-radius: 50%; animation: spin .8s linear infinite; }
    @keyframes spin { to { transform: rotate(360deg); } }
    @media (prefers-reduced-motion: reduce) { .spinner { animation: none; border-top-color: var(--color-border-strong); } }
  `]
})
export class LoadingStateComponent {
  readonly message = input('Loading…');
}
