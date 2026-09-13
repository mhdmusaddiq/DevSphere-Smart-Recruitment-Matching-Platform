import { Routes } from '@angular/router';

import { authGuard } from '../core/guards/auth.guard';
import { roleGuard } from '../core/guards/role.guard';
import { verifiedAccountGuard } from '../core/guards/verified-account.guard';
export const employerRoutes: Routes = [
  {
    path: 'employer',
    loadComponent: () =>
      import('../features/employer/shell/employer-shell.component').then(
        (module) => module.EmployerShellComponent,
      ),
    canActivate: [authGuard, verifiedAccountGuard, roleGuard],
    data: { roles: ['Employer'] },
    children: [
      {
        path: 'dashboard',
        loadComponent: () =>
          import('../features/employer/dashboard/employer-dashboard.component').then(
            (module) => module.EmployerDashboardComponent,
          ),
        data: { pageId: 'E01', pageName: 'Employer dashboard' },
      },
      {
        path: 'profile',
        loadComponent: () =>
          import('../features/employer/profile/employer-profile.component').then(
            (module) => module.EmployerProfileComponent,
          ),
        data: { pageId: 'E02', pageName: 'Employer profile' },
      },
      {
        path: 'company',
        loadComponent: () =>
          import('../features/employer/company/employer-company.component').then(
            (module) => module.EmployerCompanyComponent,
          ),
        data: { pageId: 'E03', pageName: 'Company and verification' },
      },
      {
        path: 'vacancies',
        loadComponent: () =>
          import('../features/employer/vacancies/employer-vacancies.component').then(
            (module) => module.EmployerVacanciesComponent,
          ),
        data: { pageId: 'E04', pageName: 'Vacancies' },
      },
      {
        path: 'vacancies/new',
        loadComponent: () =>
          import('../features/employer/vacancy-editor/employer-vacancy-editor.component').then(
            (module) => module.EmployerVacancyEditorComponent,
          ),
        data: { pageId: 'E05', pageName: 'Create vacancy' },
      },
      {
        path: 'vacancies/:vacancyId/edit',
        loadComponent: () =>
          import('../features/employer/vacancy-editor/employer-vacancy-editor.component').then(
            (module) => module.EmployerVacancyEditorComponent,
          ),
        data: { pageId: 'E05', pageName: 'Edit vacancy' },
      },
      {
        path: 'vacancies/:vacancyId/policy',
        loadComponent: () =>
          import('../features/employer/vacancy-editor/employer-vacancy-editor.component').then(
            (module) => module.EmployerVacancyEditorComponent,
          ),
        data: { pageId: 'E05', pageName: 'Matching policy' },
      },
      {
        path: 'vacancies/:vacancyId',
        loadComponent: () =>
          import('../features/employer/vacancy-management/employer-vacancy-management.component').then(
            (module) => module.EmployerVacancyManagementComponent,
          ),
        data: { pageId: 'E06', pageName: 'Vacancy management' },
      },
      {
        path: 'vacancies/:vacancyId/manage',
        loadComponent: () =>
          import('../features/employer/vacancy-management/employer-vacancy-management.component').then(
            (module) => module.EmployerVacancyManagementComponent,
          ),
        data: { pageId: 'E06', pageName: 'Vacancy management' },
      },
      {
        path: 'vacancies/:vacancyId/applicants',
        loadComponent: () =>
          import('../features/employer/ranked-applicants/employer-ranked-applicants.component').then(
            (module) => module.EmployerRankedApplicantsComponent,
          ),
        data: { pageId: 'E07', pageName: 'Ranked applicants' },
      },
      {
        path: 'vacancies/:vacancyId/applicants/:applicationId',
        loadComponent: () =>
          import('../features/employer/applicant-detail/employer-applicant-detail.component').then(
            (module) => module.EmployerApplicantDetailComponent,
          ),
        data: { pageId: 'E08', pageName: 'Applicant details' },
      },
      {
        path: 'applications/:applicationId',
        loadComponent: () =>
          import('../features/employer/applicant-detail/employer-applicant-detail.component').then(
            (module) => module.EmployerApplicantDetailComponent,
          ),
        data: { pageId: 'E08', pageName: 'Applicant details' },
      },
      {
        path: 'contact-requests',
        loadComponent: () =>
          import('../features/employer/contact-requests/employer-contact-requests.component').then(
            (module) => module.EmployerContactRequestsComponent,
          ),
        data: { pageId: 'E09', pageName: 'Contact requests' },
      },
      {
        path: 'recruitment/:applicationId',
        loadComponent: () =>
          import('../features/employer/recruitment-workflow/employer-recruitment-workflow.component').then(
            (module) => module.EmployerRecruitmentWorkflowComponent,
          ),
        data: { pageId: 'E10', pageName: 'Recruitment workflow' },
      },
      {
        path: 'applications/:applicationId/workflow',
        loadComponent: () =>
          import('../features/employer/recruitment-workflow/employer-recruitment-workflow.component').then(
            (module) => module.EmployerRecruitmentWorkflowComponent,
          ),
        data: { pageId: 'E10', pageName: 'Recruitment workflow' },
      },
      {
        path: 'notifications',
        loadComponent: () =>
          import('../features/employer/notifications/employer-notifications.component').then(
            (module) => module.EmployerNotificationsComponent,
          ),
        data: { pageId: 'E11', pageName: 'Notifications' },
      },
    ],
  },
];
