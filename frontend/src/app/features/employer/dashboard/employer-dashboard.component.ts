import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';

import { EmployerApiService } from '../data-access/employer-api.service';
import {
  CompanyProfile,
  EmployerDashboard,
  EmployerProfile,
  EmployerVacancy
} from '../data-access/employer.models';

interface EmployerNextAction {
  title: string;
  description: string;
  route: string;
  label: string;
}

@Component({
  selector: 'app-employer-dashboard',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './employer-dashboard.component.html',
  styleUrl: './employer-dashboard.component.css'
})
export class EmployerDashboardComponent implements OnInit {
  private readonly employerApi = inject(EmployerApiService);

  dashboard: EmployerDashboard | null = null;
  profile: EmployerProfile | null = null;
  companies: CompanyProfile[] = [];
  vacancies: EmployerVacancy[] = [];

  loading = true;
  errorMessage = '';

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.loading = true;
    this.errorMessage = '';

    forkJoin({
      dashboard: this.employerApi.getDashboard(),
      profile: this.employerApi.getProfile(),
      companies: this.employerApi.getCompanies(),
      vacancies: this.employerApi.getVacancies()
    }).subscribe({
      next: ({ dashboard, profile, companies, vacancies }) => {
        this.dashboard = dashboard;
        this.profile = profile;
        this.companies = companies;
        this.vacancies = vacancies;
        this.loading = false;
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage = this.getErrorMessage(error);
        this.loading = false;
      }
    });
  }

  get primaryCompany(): CompanyProfile | null {
    return this.companies[0] ?? null;
  }

  get vacancyPreview(): EmployerVacancy[] {
    return this.vacancies.slice(0, 3);
  }

  get nextAction(): EmployerNextAction {
    const company = this.primaryCompany;

    if (!company) {
      return {
        title: 'Create your company profile',
        description:
          'Add your company information before continuing the employer setup.',
        route: '/employer/company',
        label: 'Create company'
      };
    }

    if (!this.profile) {
      return {
        title: 'Complete your employer profile',
        description:
          'Add your employer contact and workplace information.',
        route: '/employer/profile',
        label: 'Complete profile'
      };
    }

    if (
      company.verificationStatus === 'Draft' ||
      company.verificationStatus === 'Rejected' ||
      company.verificationStatus === 'NeedsMoreInformation'
    ) {
      return {
        title: 'Submit company verification',
        description:
          'Provide current verification evidence so your company can be reviewed.',
        route: '/employer/company',
        label: 'Manage verification'
      };
    }

    if (company.verificationStatus === 'PendingReview') {
      return {
        title: 'Verification is pending review',
        description:
          'Your evidence has been submitted. You can review your current company status.',
        route: '/employer/company',
        label: 'View company'
      };
    }

    if (
      company.membershipStatus === 'Revoked' ||
      company.verificationStatus !== 'Verified'
    ) {
      return {
        title: 'Review company trust status',
        description:
          'Your current company trust state needs attention before hiring can continue.',
        route: '/employer/company',
        label: 'Manage company'
      };
    }

    if (this.vacancies.length === 0) {
      return {
        title: 'Create your first vacancy',
        description:
          'Your company is ready. Add a vacancy to begin the hiring workflow.',
        route: '/employer/vacancies',
        label: 'Create vacancy'
      };
    }

    const draftVacancy = this.vacancies.find(
      (vacancy) => vacancy.lifecycleStatus === 'Draft'
    );

    if (draftVacancy) {
      return {
        title: 'Complete your draft vacancy',
        description:
          'Finish the vacancy and matching policy before moving to applicant review.',
        route: `/employer/vacancies/${draftVacancy.id}/edit`,
        label: 'Continue vacancy'
      };
    }

    return {
      title: 'Manage your vacancies',
      description:
        'Review your current vacancies and continue the hiring workflow.',
      route: '/employer/vacancies',
      label: 'Manage vacancies'
    };
  }

  private getErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 0) {
      return 'Unable to reach the server. Please try again.';
    }

    if (error.status === 401) {
      return 'Your session is no longer valid. Please sign in again.';
    }

    if (error.status === 403) {
      return 'You do not have permission to view the employer dashboard.';
    }

    return 'We could not load your employer dashboard. Please try again.';
  }
}
