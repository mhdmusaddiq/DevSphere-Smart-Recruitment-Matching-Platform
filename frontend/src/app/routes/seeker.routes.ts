import { Routes } from '@angular/router';

import { authGuard } from '../core/guards/auth.guard';
import { roleGuard } from '../core/guards/role.guard';
import { verifiedAccountGuard } from '../core/guards/verified-account.guard';

export const seekerRoutes: Routes = [
  {
    path: 'seeker',
    loadComponent: () => import('../features/seeker/shell/seeker-shell.component').then(component => component.SeekerShellComponent),
    canActivate: [authGuard, verifiedAccountGuard, roleGuard],
    data: { roles: ['JobSeeker'] },
    children: [
      { path: 'dashboard', loadComponent: () => import('../features/seeker/dashboard/seeker-dashboard.component').then(component => component.SeekerDashboardComponent), data: { pageId: 'S01', pageName: 'Job Seeker dashboard' } },
      { path: 'applications', loadComponent: () => import('../features/applications/applications.component').then(component => component.ApplicationsComponent), data: { pageName: 'My applications' } },
      { path: 'applications/:applicationId', loadComponent: () => import('../features/seeker/application-detail/application-detail.component').then(component => component.ApplicationDetailComponent), data: { pageId: 'S05', pageName: 'Application details' } },
      { path: 'profile', loadComponent: () => import('../features/profile/profile.component').then(component => component.ProfileComponent), data: { pageName: 'Career profile' } },
      { path: 'cv', loadComponent: () => import('../features/resume/resume.component').then(component => component.ResumeComponent), data: { pageName: 'CV versions' } },
      { path: 'contact-requests', loadComponent: () => import('../features/seeker/contact-requests/contact-requests.component').then(component => component.ContactRequestsComponent), data: { pageId: 'S08', pageName: 'Contact requests' } },
      { path: 'notifications', loadComponent: () => import('../features/seeker/notifications/notifications.component').then(component => component.NotificationsComponent), data: { pageId: 'S09', pageName: 'Notifications' } }
    ]
  }
];
