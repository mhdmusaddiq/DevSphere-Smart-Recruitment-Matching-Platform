import { Component, computed, input } from '@angular/core';

import { initials } from './initials';

@Component({
  selector: 'apt-company-monogram',
  standalone: true,
  host: {
    '[style.--monogram-size]': 'size() + "px"',
    '[attr.aria-label]': 'accessibleLabel()'
  },
  template: `{{ value() }}`,
  styles: [`
    :host { display: inline-grid; width: var(--monogram-size); aspect-ratio: 1; place-items: center; flex: 0 0 auto; border: 1px solid var(--color-border); border-radius: var(--radius-md); color: var(--color-brand-navy); background: var(--color-surface-tint); font-size: calc(var(--monogram-size) * .32); font-weight: 800; }
  `]
})
export class CompanyMonogramComponent {
  readonly companyName = input('');
  readonly size = input(44);
  readonly value = computed(() => initials(this.companyName()));
  readonly accessibleLabel = computed(() =>
    this.companyName().trim()
      ? `${this.companyName()} monogram`
      : 'Company monogram'
  );
}
