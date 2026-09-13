import {
  Component,
  computed,
  DestroyRef,
  inject,
  signal
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  ActivatedRoute,
  NavigationEnd,
  Router,
  RouterLink,
  RouterLinkActive,
  RouterOutlet
} from '@angular/router';
import { filter } from 'rxjs';

import { SessionService } from '../../../core/auth/session.service';
import { BrandWordmarkComponent } from '../../../shared/brand/brand-wordmark.component';

@Component({
  selector: 'app-employer-shell',
  standalone: true,
  imports: [
    RouterLink,
    RouterLinkActive,
    RouterOutlet,
    BrandWordmarkComponent
  ],
  templateUrl: './employer-shell.component.html',
  styleUrl: './employer-shell.component.css'
})
export class EmployerShellComponent {
  readonly session = inject(SessionService);

  private readonly router = inject(Router);
  private readonly activatedRoute = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  readonly mobileNavigationOpen = signal(false);
  readonly currentPageName = signal('Employer workspace');

  readonly initials = computed(() => {
    const user = this.session.user();
    const source =
      user?.displayName?.trim() ||
      user?.email?.trim() ||
      'Employer';

    return source
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map(part => part.charAt(0).toUpperCase())
      .join('');
  });

  constructor() {
    this.updateCurrentPageName();

    this.router.events
      .pipe(
        filter(
          (event): event is NavigationEnd =>
            event instanceof NavigationEnd
        ),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => {
        this.updateCurrentPageName();
        this.closeMobileNavigation();
      });
  }

  toggleMobileNavigation(): void {
    this.mobileNavigationOpen.update(open => !open);
  }

  closeMobileNavigation(): void {
    this.mobileNavigationOpen.set(false);
  }

  private updateCurrentPageName(): void {
    let route = this.activatedRoute;

    while (route.firstChild) {
      route = route.firstChild;
    }

    const pageName = route.snapshot?.data?.['pageName'];

    this.currentPageName.set(
      typeof pageName === 'string'
        ? pageName
        : 'Employer workspace'
    );
  }
}
