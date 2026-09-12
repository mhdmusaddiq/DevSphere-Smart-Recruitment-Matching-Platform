import { Routes } from '@angular/router';

import { authGuard } from '../core/guards/auth.guard';
import { roleGuard } from '../core/guards/role.guard';
import { verifiedAccountGuard } from '../core/guards/verified-account.guard';
import { AdminShellComponent } from '../features/admin/shell/admin-shell.component';
import { RoutePlaceholderComponent } from '../shared/routes/route-placeholder.component';

export const adminRoutes: Routes = [
  {
    path: 'admin',
    component: AdminShellComponent,
    canActivate: [authGuard, verifiedAccountGuard, roleGuard],
    data: { roles: ['Admin'] },
    children: [
      {
        path: '',
        pathMatch: 'full',
        redirectTo: 'dashboard'
      },
      {
        path: 'dashboard',
        loadComponent: () =>
          import(
            '../features/admin/dashboard/admin-dashboard.component'
          ).then(module => module.AdminDashboardComponent)
      },
      {
        path: 'users',
        loadComponent: () =>
          import(
            '../features/admin/users/admin-users.component'
          ).then(module => module.AdminUsersComponent)
      },
      {
        path: 'company-verifications',
        component: RoutePlaceholderComponent,
        data: {
          pageId: 'AD03',
          pageName: 'Company verification'
        }
      },
      {
        path: 'company-verifications/:verificationId',
        component: RoutePlaceholderComponent,
        data: {
          pageId: 'AD03',
          pageName: 'Verification details'
        }
      },
      {
        path: 'catalogue',
        component: RoutePlaceholderComponent,
        data: {
          pageId: 'AD04',
          pageName: 'Catalogue management'
        }
      },
      {
        path: 'audit-system',
        component: RoutePlaceholderComponent,
        data: {
          pageId: 'AD05',
          pageName: 'Audit and system'
        }
      }
    ]
  }
];
