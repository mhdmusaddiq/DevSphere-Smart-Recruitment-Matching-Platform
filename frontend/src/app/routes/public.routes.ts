import { Routes } from '@angular/router';

import { guestOnlyGuard } from '../core/guards/guest-only.guard';
import { RoutePlaceholderComponent } from '../shared/routes/route-placeholder.component';

export const publicRoutes: Routes = [
  { path: 'jobs', loadComponent: () => import('../features/jobs/jobs.component').then(component => component.JobsComponent) },
  { path: 'jobs/:vacancyId', component: RoutePlaceholderComponent, data: { pageId: 'S03', pageName: 'Job details' } },
  { path: 'login', component: RoutePlaceholderComponent, canActivate: [guestOnlyGuard], data: { pageId: 'A02', pageName: 'Sign in' } },
  { path: 'register', component: RoutePlaceholderComponent, canActivate: [guestOnlyGuard], data: { pageId: 'A03', pageName: 'Create account' } },
  { path: 'verify-email', component: RoutePlaceholderComponent, data: { pageId: 'A04', pageName: 'Verify email' } },
  { path: 'forgot-password', component: RoutePlaceholderComponent, data: { pageId: 'A05', pageName: 'Forgot password' } },
  { path: 'reset-password', component: RoutePlaceholderComponent, data: { pageId: 'A05', pageName: 'Reset password' } }
];
