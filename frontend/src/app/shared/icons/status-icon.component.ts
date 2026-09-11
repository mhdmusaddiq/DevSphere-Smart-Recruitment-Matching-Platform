import { Component, computed, input } from '@angular/core';

import { AppIconComponent, AppIconName } from './app-icon.component';

export type StatusTone = 'success' | 'warning' | 'error' | 'info' | 'pending';

const STATUS_ICONS: Record<StatusTone, AppIconName> = {
  success: 'check',
  warning: 'warning',
  error: 'error',
  info: 'info',
  pending: 'clock'
};

@Component({
  selector: 'apt-status-icon',
  standalone: true,
  imports: [AppIconComponent],
  host: { '[attr.data-tone]': 'tone()' },
  template: `<apt-app-icon [name]="icon()" [label]="label()" />`,
  styles: [`
    :host { display: inline-flex; }
    :host([data-tone='success']) { color: var(--color-success); }
    :host([data-tone='warning']), :host([data-tone='pending']) { color: var(--color-warning); }
    :host([data-tone='error']) { color: var(--color-error); }
    :host([data-tone='info']) { color: var(--color-info); }
  `]
})
export class StatusIconComponent {
  readonly tone = input.required<StatusTone>();
  readonly label = input('');
  readonly icon = computed(() => STATUS_ICONS[this.tone()]);
}
