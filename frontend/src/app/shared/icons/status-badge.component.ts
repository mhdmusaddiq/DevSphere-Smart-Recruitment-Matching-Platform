import { Component, input } from '@angular/core';

import { StatusIconComponent, StatusTone } from './status-icon.component';

@Component({
  selector: 'apt-status-badge',
  standalone: true,
  imports: [StatusIconComponent],
  host: { '[attr.data-tone]': 'tone()' },
  template: `<apt-status-icon [tone]="tone()" /><span>{{ text() }}</span>`,
  styles: [`
    :host { display: inline-flex; align-items: center; gap: .35rem; padding: .35rem .65rem; border: 1px solid currentColor; border-radius: 999px; color: var(--color-info); background: var(--color-info-soft); font: var(--font-label); }
    :host([data-tone='success']) { color: var(--color-success); background: var(--color-success-soft); }
    :host([data-tone='warning']), :host([data-tone='pending']) { color: var(--color-warning); background: var(--color-warning-soft); }
    :host([data-tone='error']) { color: var(--color-error); background: var(--color-error-soft); }
  `]
})
export class StatusBadgeComponent {
  readonly tone = input.required<StatusTone>();
  readonly text = input.required<string>();
}
