import { Routes } from '@angular/router';

import { authGuard } from '../core/guards/auth.guard';
import { roleGuard } from '../core/guards/role.guard';
import { verifiedAccountGuard } from '../core/guards/verified-account.guard';
import { EmployerCompanyComponent } from '../features/employer/company/employer-company.component';
import { EmployerDashboardComponent } from '../features/employer/dashboard/employer-dashboard.component';
import { EmployerProfileComponent } from '../features/employer/profile/employer-profile.component';
import { EmployerVacanciesComponent } from '../features/employer/vacancies/employer-vacancies.component';
import { EmployerVacancyEditorComponent } from '../features/employer/vacancy-editor/employer-vacancy-editor.component';
import { EmployerVacancyManagementComponent } from '../features/employer/vacancy-management/employer-vacancy-management.component';
import { EmployerRankedApplicantsComponent } from '../features/employer/ranked-applicants/employer-ranked-applicants.component';
import { EmployerApplicantDetailComponent } from '../features/employer/applicant-detail/employer-applicant-detail.component';
import { EmployerShellComponent } from '../features/employer/shell/employer-shell.component';

import { EmployerContactRequestsComponent } from '../features/employer/contact-requests/employer-contact-requests.component';
import { EmployerRecruitmentWorkflowComponent } from '../features/employer/recruitment-workflow/employer-recruitment-workflow.component';
import { EmployerNotificationsComponent } from '../features/employer/notifications/employer-notifications.component';
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
      { path: 'vacancies', component: EmployerVacanciesComponent, data: { pageId: 'E04', pageName: 'Vacancies' } },
      { path: 'vacancies/new', component: EmployerVacancyEditorComponent, data: { pageId: 'E05', pageName: 'Create vacancy' } },
      { path: 'vacancies/:vacancyId/edit', component: EmployerVacancyEditorComponent, data: { pageId: 'E05', pageName: 'Edit vacancy' } },
      { path: 'vacancies/:vacancyId/policy', component: EmployerVacancyEditorComponent, data: { pageId: 'E05', pageName: 'Matching policy' } },
      { path: 'vacancies/:vacancyId', component: EmployerVacancyManagementComponent, data: { pageId: 'E06', pageName: 'Vacancy management' } },
      { path: 'vacancies/:vacancyId/manage', component: EmployerVacancyManagementComponent, data: { pageId: 'E06', pageName: 'Vacancy management' } },
      { path: 'vacancies/:vacancyId/applicants', component: EmployerRankedApplicantsComponent, data: { pageId: 'E07', pageName: 'Ranked applicants' } },
      { path: 'vacancies/:vacancyId/applicants/:applicationId', component: EmployerApplicantDetailComponent, data: { pageId: 'E08', pageName: 'Applicant details' } },
      { path: 'applications/:applicationId', component: EmployerApplicantDetailComponent, data: { pageId: 'E08', pageName: 'Applicant details' } },
      { path: 'contact-requests', component: EmployerContactRequestsComponent, data: { pageId: 'E09', pageName: 'Contact requests' } },
      { path: 'recruitment/:applicationId', component: EmployerRecruitmentWorkflowComponent, data: { pageId: 'E10', pageName: 'Recruitment workflow' } },
      { path: 'applications/:applicationId/workflow', component: EmployerRecruitmentWorkflowComponent, data: { pageId: 'E10', pageName: 'Recruitment workflow' } },
      { path: 'notifications', component: EmployerNotificationsComponent, data: { pageId: 'E11', pageName: 'Notifications' } }
    ]
  }
];
