import { Routes } from '@angular/router';

import { authGuard } from '../core/guards/auth.guard';
import { roleGuard } from '../core/guards/role.guard';
import { verifiedAccountGuard } from '../core/guards/verified-account.guard';
import { RoutePlaceholderComponent } from '../shared/routes/route-placeholder.component';

export const adminRoutes: Routes = [
  {
    path: 'admin',
    canActivate: [authGuard, verifiedAccountGuard, roleGuard],
    data: { roles: ['Admin'] },
    children: [
      { path: 'dashboard', component: RoutePlaceholderComponent, data: { pageId: 'AD01', pageName: 'Admin dashboard' } },
      { path: 'users', component: RoutePlaceholderComponent, data: { pageId: 'AD02', pageName: 'User accounts' } },
      { path: 'company-verifications', component: RoutePlaceholderComponent, data: { pageId: 'AD03', pageName: 'Company verification' } },
      { path: 'company-verifications/:verificationId', component: RoutePlaceholderComponent, data: { pageId: 'AD03', pageName: 'Verification details' } },
      { path: 'catalogue', component: RoutePlaceholderComponent, data: { pageId: 'AD04', pageName: 'Catalogue management' } },
      { path: 'audit-system', component: RoutePlaceholderComponent, data: { pageId: 'AD05', pageName: 'Audit and system' } }
    ]
  }
];
