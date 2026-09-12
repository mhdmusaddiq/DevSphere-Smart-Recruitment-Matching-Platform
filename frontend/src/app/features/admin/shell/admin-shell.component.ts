import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  signal
} from '@angular/core';
import {
  RouterLink,
  RouterLinkActive,
  RouterOutlet
} from '@angular/router';

import {
  SessionService
} from '../../../core/auth/session.service';
import {
  CompanyMonogramComponent
} from '../../../shared/avatar/company-monogram.component';
import {
  IdentityAvatarComponent
} from '../../../shared/avatar/identity-avatar.component';
import {
  BrandWordmarkComponent
} from '../../../shared/brand/brand-wordmark.component';
import {
  AppIconComponent,
  AppIconName
} from '../../../shared/icons/app-icon.component';

interface AdminNavItem {
  label: string;
  path: string;
  icon: AppIconName;
}

@Component({
  selector: 'app-admin-shell',
  standalone: true,
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    BrandWordmarkComponent,
    IdentityAvatarComponent,
    CompanyMonogramComponent,
    AppIconComponent
  ],
  templateUrl: './admin-shell.component.html',
  styleUrl: './admin-shell.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdminShellComponent {
  private readonly session = inject(SessionService);

  readonly menuOpen = signal(false);

  readonly displayName = computed(
    () =>
      this.session.user()?.displayName?.trim() ||
      'Administrator'
  );

  readonly navItems: AdminNavItem[] = [
    {
      label: 'Dashboard',
      path: '/admin/dashboard',
      icon: 'dashboard'
    },
    {
      label: 'Users',
      path: '/admin/users',
      icon: 'user'
    },
    {
      label: 'Company verification',
      path: '/admin/company-verifications',
      icon: 'company'
    },
    {
      label: 'Catalogue',
      path: '/admin/catalogue',
      icon: 'briefcase'
    },
    {
      label: 'Audit & system',
      path: '/admin/audit-system',
      icon: 'document'
    }
  ];

  toggleMenu(): void {
    this.menuOpen.update(value => !value);
  }

  closeMenu(): void {
    this.menuOpen.set(false);
  }
}
