import { Component, input, output } from '@angular/core';

import { AppIconComponent } from '../icons/app-icon.component';

@Component({
  selector: 'apt-error-state',
  standalone: true,
  imports: [AppIconComponent],
  host: { role: 'alert' },
  template: `
    <apt-app-icon name="error" [size]="28" />
    <div>
      <strong>{{ title() }}</strong>
      <p>{{ message() }}</p>
      @if (retryAfterSeconds() !== null) {
        <small>Try again in {{ retryAfterSeconds() }} seconds.</small>
      }
    </div>
    @if (retryable()) {
      <button type="button" (click)="retry.emit()">Try again</button>
    }
  `,
  styles: [`
    :host { display: grid; grid-template-columns: auto 1fr auto; align-items: start; gap: var(--space-3); padding: var(--space-4); color: var(--color-error); background: var(--color-error-soft); border: 1px solid currentColor; border-radius: var(--radius-md); }
    strong, p { display: block; margin: 0; }
    p, small { color: var(--color-text); }
    button { padding: .55rem .8rem; border: 1px solid currentColor; border-radius: var(--radius-sm); color: inherit; background: var(--color-surface); cursor: pointer; font-weight: 700; }
  `]
})
export class ErrorStateComponent {
  readonly title = input('Something went wrong');
  readonly message = input('Please try again.');
  readonly retryAfterSeconds = input<number | null>(null);
  readonly retryable = input(true);
  readonly retry = output<void>();
}
