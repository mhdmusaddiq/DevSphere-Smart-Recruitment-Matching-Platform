import { Component } from '@angular/core';

@Component({
  selector: 'apt-empty-state-visual',
  standalone: true,
  template: `
    <svg viewBox="0 0 160 96" role="img" aria-label="Abstract AptLens illustration">
      <circle cx="80" cy="48" r="30" class="halo" />
      <path d="M22 70 55 22l28 48 21-34 34 34Z" class="geometry" />
      <circle cx="116" cy="25" r="7" class="accent" />
    </svg>
  `,
  styles: [`
    :host { display: block; width: min(10rem, 100%); color: var(--color-brand-violet); }
    svg { display: block; width: 100%; }
    .halo { fill: var(--color-surface-tint); }
    .geometry { fill: none; stroke: currentColor; stroke-width: 3; stroke-linecap: round; stroke-linejoin: round; }
    .accent { fill: var(--color-teal); }
  `]
})
export class EmptyStateVisualComponent {}
