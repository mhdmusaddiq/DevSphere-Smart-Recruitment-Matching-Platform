import { Routes } from '@angular/router';

import { authGuard } from '../core/guards/auth.guard';
import { roleGuard } from '../core/guards/role.guard';
import { verifiedAccountGuard } from '../core/guards/verified-account.guard';
import { EmployerCompanyComponent } from '../features/employer/company/employer-company.component';
import { EmployerDashboardComponent } from '../features/employer/dashboard/employer-dashboard.component';
import { EmployerProfileComponent } from '../features/employer/profile/employer-profile.component';
import { EmployerShellComponent } from '../features/employer/shell/employer-shell.component';
import { RoutePlaceholderComponent } from '../shared/routes/route-placeholder.component';

export const employerRoutes: Routes = [
  {
    path: 'employer',
    component: EmployerShellComponent,
    canActivate: [authGuard, verifiedAccountGuard, roleGuard],
    data: { roles: ['Employer'] },
    children: [
      { path: 'dashboard', component: EmployerDashboardComponent, data: { pageId: 'E01', pageName: 'Employer dashboard' } },
      { path: 'profile', component: EmployerProfileComponent, data: { pageId: 'E02', pageName: 'Employer profile' } },
      { path: 'company', component: EmployerCompanyComponent, data: { pageId: 'E03', pageName: 'Company and verification' } },
      { path: 'vacancies', component: RoutePlaceholderComponent, data: { pageId: 'E04', pageName: 'Vacancies' } },
      { path: 'vacancies/:vacancyId/policy', component: RoutePlaceholderComponent, data: { pageId: 'E05', pageName: 'Vacancy matching policy' } },
      { path: 'vacancies/:vacancyId/manage', component: RoutePlaceholderComponent, data: { pageId: 'E06', pageName: 'Vacancy management' } },
      { path: 'vacancies/:vacancyId/applicants', component: RoutePlaceholderComponent, data: { pageId: 'E07', pageName: 'Ranked applicants' } },
      { path: 'applications/:applicationId', component: RoutePlaceholderComponent, data: { pageId: 'E08', pageName: 'Applicant details' } },
      { path: 'contact-requests', component: RoutePlaceholderComponent, data: { pageId: 'E09', pageName: 'Contact requests' } },
      { path: 'recruitment/:applicationId', component: RoutePlaceholderComponent, data: { pageId: 'E10', pageName: 'Recruitment workflow' } },
      { path: 'notifications', component: RoutePlaceholderComponent, data: { pageId: 'E11', pageName: 'Notifications' } }
    ]
  }
];
