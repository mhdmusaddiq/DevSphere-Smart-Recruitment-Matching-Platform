import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  NavigationEnd,
  Router,
  RouterLink,
  RouterLinkActive,
  RouterOutlet
} from '@angular/router';
import { filter } from 'rxjs';

import { AccountMenuComponent } from '../../../shared/account/account-menu.component';
import { BrandWordmarkComponent } from '../../../shared/brand/brand-wordmark.component';
import { AppIconComponent, AppIconName } from '../../../shared/icons/app-icon.component';

interface SeekerNavItem {
  label: string;
  path: string;
  icon: AppIconName;
  exact?: boolean;
}

@Component({
  selector: 'app-seeker-shell',
  standalone: true,
  imports: [
    RouterLink,
    RouterLinkActive,
    RouterOutlet,
    AccountMenuComponent,
    BrandWordmarkComponent,
    AppIconComponent
  ],
  templateUrl: './seeker-shell.component.html',
  styleUrl: './seeker-shell.component.css'
})
export class SeekerShellComponent {
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly menuOpen = signal(false);
  readonly currentPageName = signal('Job Seeker workspace');
  readonly navItems: SeekerNavItem[] = [
    { label: 'Dashboard', path: '/seeker/dashboard', icon: 'dashboard', exact: true },
    { label: 'Find Jobs', path: '/jobs', icon: 'search' },
    { label: 'My Applications', path: '/seeker/applications', icon: 'document' },
    { label: 'Profile', path: '/seeker/profile', icon: 'user' },
    { label: 'CV', path: '/seeker/cv', icon: 'briefcase' },
    { label: 'Contact Requests', path: '/seeker/contact-requests', icon: 'company' },
    { label: 'Notifications', path: '/seeker/notifications', icon: 'notification' }
  ];

  constructor() {
    this.updatePageName();
    this.router.events
      .pipe(
        filter((event): event is NavigationEnd => event instanceof NavigationEnd),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => {
        this.updatePageName();
        this.closeMenu();
      });
  }

  toggleMenu(): void {
    this.menuOpen.update(open => !open);
  }

  closeMenu(): void {
    this.menuOpen.set(false);
  }

  private updatePageName(): void {
    const path = this.router.url.split(/[?#]/, 1)[0];
    const pageName = path.startsWith('/seeker/applications/')
      ? 'Application details'
      : new Map<string, string>([
          ['/seeker/dashboard', 'Job Seeker dashboard'],
          ['/seeker/applications', 'My applications'],
          ['/seeker/profile', 'Career profile'],
          ['/seeker/cv', 'CV versions'],
          ['/seeker/contact-requests', 'Contact requests'],
          ['/seeker/notifications', 'Notifications']
        ]).get(path);

    this.currentPageName.set(pageName ?? 'Job Seeker workspace');
  }
}
