import { Component, computed, input } from '@angular/core';

import { initials } from './initials';

@Component({
  selector: 'apt-identity-avatar',
  standalone: true,
  host: {
    '[style.--avatar-size]': 'size() + "px"',
    '[attr.aria-label]': 'accessibleLabel()'
  },
  template: `{{ value() }}`,
  styles: [`
    :host { display: inline-grid; width: var(--avatar-size); aspect-ratio: 1; place-items: center; flex: 0 0 auto; border-radius: 50%; color: white; background: var(--color-brand-violet); font-size: calc(var(--avatar-size) * .36); font-weight: 800; }
  `]
})
export class IdentityAvatarComponent {
  readonly displayName = input('');
  readonly size = input(40);
  readonly value = computed(() => initials(this.displayName()));
  readonly accessibleLabel = computed(() =>
    this.displayName().trim()
      ? `${this.displayName()} avatar`
      : 'Account avatar'
  );
}
