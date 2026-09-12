import { Routes } from '@angular/router';

import { authGuard } from '../core/guards/auth.guard';
import { roleGuard } from '../core/guards/role.guard';
import { verifiedAccountGuard } from '../core/guards/verified-account.guard';
import { AdminShellComponent } from '../features/admin/shell/admin-shell.component';

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
        loadComponent: () =>
          import(
            '../features/admin/company-verifications/admin-verification-queue.component'
          ).then(
            module =>
              module.AdminVerificationQueueComponent
          )
      },
      {
        path: 'company-verifications/:verificationId',
        loadComponent: () =>
          import(
            '../features/admin/company-verifications/admin-verification-detail.component'
          ).then(
            module =>
              module.AdminVerificationDetailComponent
          )
      },
      {
        path: 'catalogue',
        loadComponent: () =>
          import(
            '../features/admin/catalogue/admin-catalogue.component'
          ).then(
            module =>
              module.AdminCatalogueComponent
          )
      },
      {
        path: 'audit-system',
        loadComponent: () =>
          import(
            '../features/admin/audit-system/admin-audit-system.component'
          ).then(
            module =>
              module.AdminAuditSystemComponent
          )
      }
    ]
  }
];
