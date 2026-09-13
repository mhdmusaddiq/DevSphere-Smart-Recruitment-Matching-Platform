import { Component, input } from '@angular/core';

@Component({
  selector: 'apt-brand-wordmark',
  standalone: true,
  template: `
    <span class="brand" aria-label="AptLens">
      <img class="mark" src="/assets/aptlens/brand/aptlens-mark.svg" alt="" aria-hidden="true" />
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
    .mark { display: block; width: 2.25rem; height: 2.25rem; object-fit: contain; }
    .copy { display: grid; line-height: 1.05; }
    strong { color: var(--color-text); font-size: 1.15rem; letter-spacing: -.035em; }
    strong span { color: var(--color-brand-violet); }
    small { margin-top: .25rem; color: var(--color-text-muted); font-size: .68rem; }
  `]
})
export class BrandWordmarkComponent {
  readonly showTagline = input(false);
}
