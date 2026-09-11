import { Component, computed, input } from '@angular/core';

export type AppIconName =
  | 'briefcase' | 'calendar' | 'check' | 'chevron' | 'clock'
  | 'close' | 'company' | 'dashboard' | 'document' | 'download'
  | 'edit' | 'error' | 'eye' | 'filter' | 'info' | 'location'
  | 'lock' | 'menu' | 'notification' | 'search' | 'upload'
  | 'user' | 'warning';

const ICON_PATHS: Record<AppIconName, string> = {
  briefcase: 'M9 7V5h6v2m-9 0h12a2 2 0 0 1 2 2v9H4V9a2 2 0 0 1 2-2Zm-2 5h10',
  calendar: 'M5 7h14v13H5V7Zm3-3v6m8-6v6M5 11h14',
  check: 'm5 12 4 4L19 6',
  chevron: 'm9 6 6 6-6 6',
  clock: 'M12 21a9 9 0 1 0 0-18 9 9 0 0 0 0 18Zm0-13v5l3 2',
  close: 'M6 6l12 12M18 6 6 18',
  company: 'M4 21V7l8-4 8 4v14M8 10h2m4 0h2m-8 4h2m4 0h2m-5 7v-4h2v4',
  dashboard: 'M4 4h7v7H4V4Zm9 0h7v4h-7V4Zm0 6h7v10h-7V10ZM4 13h7v7H4v-7Z',
  document: 'M6 3h8l4 4v14H6V3Zm8 0v5h4M9 12h6m-6 4h6',
  download: 'M12 3v12m-5-5 5 5 5-5M5 21h14',
  edit: 'm4 20 4-1 11-11-3-3L5 16l-1 4ZM14 7l3 3',
  error: 'M12 21a9 9 0 1 0 0-18 9 9 0 0 0 0 18Zm0-13v5m0 4h.01',
  eye: 'M3 12s3-6 9-6 9 6 9 6-3 6-9 6-9-6-9-6Zm9 3a3 3 0 1 0 0-6 3 3 0 0 0 0 6Z',
  filter: 'M4 5h16l-6 7v6l-4 2v-8L4 5Z',
  info: 'M12 21a9 9 0 1 0 0-18 9 9 0 0 0 0 18Zm0-9v5m0-9h.01',
  location: 'M12 21s6-6 6-12a6 6 0 1 0-12 0c0 6 6 12 6 12Zm0-9a3 3 0 1 0 0-6 3 3 0 0 0 0 6Z',
  lock: 'M6 10h12v11H6V10Zm3 0V7a3 3 0 0 1 6 0v3',
  menu: 'M4 7h16M4 12h16M4 17h16',
  notification: 'M6 17h12l-2-3V9a4 4 0 0 0-8 0v5l-2 3Zm4 3h4',
  search: 'M11 18a7 7 0 1 0 0-14 7 7 0 0 0 0 14Zm5-2 5 5',
  upload: 'M12 21V9m-5 5 5-5 5 5M5 3h14',
  user: 'M12 12a4 4 0 1 0 0-8 4 4 0 0 0 0 8Zm-7 9a7 7 0 0 1 14 0',
  warning: 'M12 3 2 21h20L12 3Zm0 6v5m0 3h.01'
};

@Component({
  selector: 'apt-app-icon',
  standalone: true,
  host: { '[attr.aria-label]': 'label() || null' },
  template: `
    <svg
      [attr.width]="size()"
      [attr.height]="size()"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      stroke-width="1.8"
      stroke-linecap="round"
      stroke-linejoin="round"
      [attr.aria-hidden]="label() ? null : 'true'"
      role="img">
      <path [attr.d]="path()" />
    </svg>
  `,
  styles: [`:host { display: inline-flex; line-height: 0; }`]
})
export class AppIconComponent {
  readonly name = input.required<AppIconName>();
  readonly size = input(20);
  readonly label = input('');
  readonly path = computed(() => ICON_PATHS[this.name()]);
}
