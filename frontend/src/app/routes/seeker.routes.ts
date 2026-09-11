import { Routes } from '@angular/router';

import { authGuard } from '../core/guards/auth.guard';
import { roleGuard } from '../core/guards/role.guard';
import { verifiedAccountGuard } from '../core/guards/verified-account.guard';
import { RoutePlaceholderComponent } from '../shared/routes/route-placeholder.component';

export const seekerRoutes: Routes = [
  {
    path: 'seeker',
    canActivate: [authGuard, verifiedAccountGuard, roleGuard],
    data: { roles: ['JobSeeker'] },
    children: [
      { path: 'dashboard', component: RoutePlaceholderComponent, data: { pageId: 'S01', pageName: 'Job Seeker dashboard' } },
      { path: 'applications', loadComponent: () => import('../features/applications/applications.component').then(component => component.ApplicationsComponent) },
      { path: 'applications/:applicationId', component: RoutePlaceholderComponent, data: { pageId: 'S05', pageName: 'Application details' } },
      { path: 'profile', loadComponent: () => import('../features/profile/profile.component').then(component => component.ProfileComponent) },
      { path: 'cv', loadComponent: () => import('../features/resume/resume.component').then(component => component.ResumeComponent) },
      { path: 'contact-requests', component: RoutePlaceholderComponent, data: { pageId: 'S08', pageName: 'Contact requests' } },
      { path: 'notifications', component: RoutePlaceholderComponent, data: { pageId: 'S09', pageName: 'Notifications' } }
    ]
  }
];
