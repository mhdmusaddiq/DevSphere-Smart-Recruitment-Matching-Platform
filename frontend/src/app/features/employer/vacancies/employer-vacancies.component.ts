import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';

import { EmployerApiService } from '../data-access/employer-api.service';
import { EmployerVacancy } from '../data-access/employer.models';

type VacancyLifecycleFilter =
  | 'All'
  | 'Draft'
  | 'Published'
  | 'Closed';

@Component({
  selector: 'app-employer-vacancies',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './employer-vacancies.component.html',
  styleUrl: './employer-vacancies.component.css'
})
export class EmployerVacanciesComponent implements OnInit {
  private readonly employerApi = inject(EmployerApiService);

  readonly lifecycleFilters: VacancyLifecycleFilter[] = [
    'All',
    'Draft',
    'Published',
    'Closed'
  ];

  vacancies: EmployerVacancy[] = [];
  selectedFilter: VacancyLifecycleFilter = 'All';

  loading = true;
  actionVacancyId: string | null = null;
  errorMessage = '';
  actionMessage = '';

  ngOnInit(): void {
    this.loadVacancies();
  }

  get filteredVacancies(): EmployerVacancy[] {
    if (this.selectedFilter === 'All') {
      return this.vacancies;
    }

    return this.vacancies.filter(
      vacancy =>
        vacancy.lifecycleStatus === this.selectedFilter
    );
  }

  setFilter(filter: VacancyLifecycleFilter): void {
    this.selectedFilter = filter;
  }

  loadVacancies(): void {
    this.loading = true;
    this.errorMessage = '';

    this.employerApi.getVacancies().subscribe({
      next: vacancies => {
        this.vacancies = vacancies;
        this.loading = false;
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage = this.getLoadErrorMessage(error);
        this.loading = false;
      }
    });
  }

  publishVacancy(vacancy: EmployerVacancy): void {
    if (
      vacancy.lifecycleStatus !== 'Draft' ||
      this.actionVacancyId
    ) {
      return;
    }

    this.actionVacancyId = vacancy.id;
    this.errorMessage = '';
    this.actionMessage = '';

    this.employerApi.publishVacancy(vacancy.id).subscribe({
      next: updated => {
        this.replaceVacancy(updated);
        this.actionMessage = 'Vacancy published successfully.';
        this.actionVacancyId = null;
      },
      error: (error: HttpErrorResponse) => {
        this.actionVacancyId = null;
        this.handleLifecycleActionError(
          error,
          'The vacancy could not be published.'
        );
      }
    });
  }

  closeVacancy(vacancy: EmployerVacancy): void {
    if (
      vacancy.lifecycleStatus !== 'Published' ||
      this.actionVacancyId
    ) {
      return;
    }

    this.actionVacancyId = vacancy.id;
    this.errorMessage = '';
    this.actionMessage = '';

    this.employerApi.closeVacancy(vacancy.id).subscribe({
      next: updated => {
        this.replaceVacancy(updated);
        this.actionMessage = 'Vacancy closed successfully.';
        this.actionVacancyId = null;
      },
      error: (error: HttpErrorResponse) => {
        this.actionVacancyId = null;
        this.handleLifecycleActionError(
          error,
          'The vacancy could not be closed.'
        );
      }
    });
  }

  formatClosingDate(value: string | null): string {
    if (!value) {
      return '—';
    }

    const date = new Date(value);

    if (Number.isNaN(date.getTime())) {
      return '—';
    }

    return new Intl.DateTimeFormat('en', {
      day: '2-digit',
      month: 'short'
    }).format(date);
  }

  private replaceVacancy(updated: EmployerVacancy): void {
    this.vacancies = this.vacancies.map(vacancy =>
      vacancy.id === updated.id
        ? updated
        : vacancy
    );
  }

  private handleLifecycleActionError(
    error: HttpErrorResponse,
    fallback: string
  ): void {
    if (error.status === 409) {
      const conflictMessage =
        this.readServerMessage(error) ||
        'The vacancy state changed. The latest server state has been reloaded.';

      this.reloadVacanciesAfterConflict(conflictMessage);
      return;
    }

    if (error.status === 403) {
      this.errorMessage =
        this.readServerMessage(error) ||
        'You are not allowed to perform this vacancy action.';
      return;
    }

    if (error.status === 404) {
      this.errorMessage =
        this.readServerMessage(error) ||
        'The vacancy could not be found.';
      return;
    }

    this.errorMessage =
      this.readServerMessage(error) ||
      fallback;
  }

  private reloadVacanciesAfterConflict(message: string): void {
    this.loading = true;

    this.employerApi.getVacancies().subscribe({
      next: vacancies => {
        this.vacancies = vacancies;
        this.errorMessage = message;
        this.loading = false;
      },
      error: () => {
        this.errorMessage = message;
        this.loading = false;
      }
    });
  }
  private getLoadErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 0) {
      return 'Unable to reach the vacancy service.';
    }

    if (error.status === 401) {
      return 'Your session is not authorized to load vacancies.';
    }

    if (error.status === 403) {
      return 'You do not have access to employer vacancies.';
    }

    return (
      this.readServerMessage(error) ||
      'Employer vacancies could not be loaded.'
    );
  }

  private readServerMessage(
    error: HttpErrorResponse
  ): string {
    const message = error.error?.message;

    return typeof message === 'string'
      ? message
      : '';
  }
}
