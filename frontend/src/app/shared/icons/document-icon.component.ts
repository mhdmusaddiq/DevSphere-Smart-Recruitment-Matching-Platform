import { Component, computed, input } from '@angular/core';

import { AppIconComponent } from './app-icon.component';

export type DocumentIconVariant = 'pdf' | 'document' | 'evidence';

@Component({
  selector: 'apt-document-icon',
  standalone: true,
  imports: [AppIconComponent],
  host: { '[attr.data-variant]': 'variant()' },
  template: `
    <apt-app-icon name="document" [size]="size()" />
    <span>{{ label() }}</span>
  `,
  styles: [`
    :host { display: inline-flex; align-items: center; gap: .35rem; color: var(--color-brand-navy); font-weight: 800; }
    :host([data-variant='pdf']) { color: var(--color-error); }
    :host([data-variant='evidence']) { color: var(--color-brand-violet); }
    span { font-size: .68rem; letter-spacing: .06em; text-transform: uppercase; }
  `]
})
export class DocumentIconComponent {
  readonly variant = input<DocumentIconVariant>('document');
  readonly size = input(24);
  readonly label = computed(() => this.variant() === 'document' ? 'DOC' : this.variant().toUpperCase());
}
