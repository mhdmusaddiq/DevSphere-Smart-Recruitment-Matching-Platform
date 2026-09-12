import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';

import { EmployerApiService } from '../data-access/employer-api.service';
import {
  EmployerJobApplication,
  EmployerVacancy,
  MatchingPolicyRevisionDto
} from '../data-access/employer.models';

@Component({
  selector: 'app-employer-vacancy-management',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './employer-vacancy-management.component.html',
  styleUrl: './employer-vacancy-management.component.css'
})
export class EmployerVacancyManagementComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly api = inject(EmployerApiService);

  vacancyId = '';

  vacancy: EmployerVacancy | null = null;
  policy: MatchingPolicyRevisionDto | null = null;
  applications: EmployerJobApplication[] = [];

  loading = true;
  actionLoading = false;

  errorMessage = '';
  successMessage = '';

  ngOnInit(): void {
    this.vacancyId =
      this.route.snapshot.paramMap.get('vacancyId') ?? '';

    if (!this.vacancyId) {
      this.loading = false;
      this.errorMessage = 'Vacancy identifier is missing.';
      return;
    }

    this.load();
  }

  get isDraft(): boolean {
    return this.vacancy?.lifecycleStatus === 'Draft';
  }

  get isPublished(): boolean {
    return this.vacancy?.lifecycleStatus === 'Published';
  }

  get isClosed(): boolean {
    return this.vacancy?.lifecycleStatus === 'Closed';
  }

  get applicationCount(): number {
    return this.applications.length;
  }

  countStatus(status: string): number {
    return this.applications.filter(
      application =>
        application.status.toLowerCase() === status.toLowerCase()
    ).length;
  }

  load(): void {
    this.loading = true;
    this.errorMessage = '';

    forkJoin({
      vacancy: this.api.getVacancy(this.vacancyId),
      policy: this.api.getCurrentMatchingPolicy(this.vacancyId),
      applications: this.api.getVacancyApplications(this.vacancyId)
    }).subscribe({
      next: ({ vacancy, policy, applications }) => {
        this.vacancy = vacancy;
        this.policy = policy;
        this.applications = applications;
        this.loading = false;
      },
      error: (error: HttpErrorResponse) => {
        this.loading = false;

        this.errorMessage =
          error.status === 404
            ? 'Vacancy could not be found.'
            : 'Unable to load vacancy management data.';
      }
    });
  }

  edit(): void {
    if (!this.isDraft) {
      return;
    }

    void this.router.navigate([
      '/employer/vacancies',
      this.vacancyId,
      'edit'
    ]);
  }

  publish(): void {
    if (!this.isDraft || this.actionLoading) {
      return;
    }

    this.actionLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.api.publishVacancy(this.vacancyId).subscribe({
      next: vacancy => {
        this.vacancy = vacancy;
        this.actionLoading = false;
        this.successMessage = 'Vacancy published.';
      },
      error: error => this.handleLifecycleError(error)
    });
  }

  close(): void {
    if (!this.isPublished || this.actionLoading) {
      return;
    }

    this.actionLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.api.closeVacancy(this.vacancyId).subscribe({
      next: vacancy => {
        this.vacancy = vacancy;
        this.actionLoading = false;
        this.successMessage = 'Vacancy closed.';
      },
      error: error => this.handleLifecycleError(error)
    });
  }

  private handleLifecycleError(
    error: HttpErrorResponse
  ): void {
    this.actionLoading = false;

    if (error.status === 409) {
      this.errorMessage =
        'The vacancy changed on the server. Current data has been reloaded.';

      this.api.getVacancy(this.vacancyId).subscribe({
        next: vacancy => {
          this.vacancy = vacancy;
        }
      });

      return;
    }

    this.errorMessage =
      error.error?.message ??
      'The vacancy action could not be completed.';
  }
}