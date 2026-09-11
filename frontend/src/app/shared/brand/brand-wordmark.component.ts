import { Component, input } from '@angular/core';

@Component({
  selector: 'apt-brand-wordmark',
  standalone: true,
  template: `
    <span class="brand" aria-label="AptLens">
      <span class="mark" aria-hidden="true">A</span>
      <span class="copy">
        <strong>Apt<span>Lens</span></strong>
        @if (showTagline()) {
          <small>See the fit. Know the why.</small>
        }
      </span>
    </span>
  `,
  styles: [`
    :host { display: inline-flex; }
    .brand { display: inline-flex; align-items: center; gap: .65rem; color: var(--color-brand-navy); }
    .mark { display: grid; width: 2.25rem; aspect-ratio: 1; place-items: center; color: white; border-radius: .7rem; background: linear-gradient(135deg, var(--color-brand-violet), var(--color-brand-navy)); font-weight: 800; }
    .copy { display: grid; line-height: 1.05; }
    strong { font-size: 1.15rem; letter-spacing: -.025em; }
    strong span { color: var(--color-brand-violet); }
    small { margin-top: .25rem; color: var(--color-text-muted); font-size: .68rem; }
  `]
})
export class BrandWordmarkComponent {
  readonly showTagline = input(false);
}
