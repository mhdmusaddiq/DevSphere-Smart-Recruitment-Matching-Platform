import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { CompanyMonogramComponent } from '../../shared/avatar/company-monogram.component';
import { EmptyStateVisualComponent } from '../../shared/states/empty-state-visual.component';
import { ErrorStateComponent } from '../../shared/states/error-state.component';
import { LoadingStateComponent } from '../../shared/states/loading-state.component';
import { SeekerWorkflowApiService } from '../seeker/data/seeker-workflow-api.service';
import { SeekerApplication } from '../seeker/data/seeker-workflow.models';

type ApplicationFilter =
  | 'All'
  | 'Active'
  | 'Shortlisted'
  | 'Selected'
  | 'Rejected'
  | 'Withdrawn';

@Component({
  selector: 'app-applications',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    CompanyMonogramComponent,
    EmptyStateVisualComponent,
    ErrorStateComponent,
    LoadingStateComponent
  ],
  templateUrl: './applications.component.html',
  styleUrl: './applications.component.css'
})
export class ApplicationsComponent implements OnInit {
  private readonly api = inject(SeekerWorkflowApiService);

  readonly filters: ApplicationFilter[] = [
    'All',
    'Active',
    'Shortlisted',
    'Selected',
    'Rejected',
    'Withdrawn'
  ];

  applications: SeekerApplication[] = [];
  filter: ApplicationFilter = 'All';
  resumeVersionNumbers = new Map<string, number>();

  loading = true;
  refreshing = false;
  withdrawingId: string | null = null;
  withdrawCandidate: SeekerApplication | null = null;

  errorMessage = '';
  staleMessage = '';
  actionMessage = '';

  ngOnInit(): void {
    this.loadApplications();
  }

  get filteredApplications(): SeekerApplication[] {
    if (this.filter === 'All') {
      return this.applications;
    }

    if (this.filter === 'Active') {
      return this.applications.filter(application =>
        ['Submitted', 'Screening', 'UnderReview', 'Shortlisted']
          .includes(application.status)
      );
    }

    return this.applications.filter(
      application => application.status === this.filter
    );
  }

  loadApplications(showLoader = true): void {
    if (showLoader && this.applications.length === 0) {
      this.loading = true;
    } else {
      this.refreshing = true;
    }

    this.errorMessage = '';
    this.actionMessage = '';

    this.api
      .getApplications()
      .pipe(finalize(() => {
        this.loading = false;
        this.refreshing = false;
      }))
      .subscribe({
        next: applications => {
          this.applications = applications;
          this.staleMessage = '';

          if (applications.some(application => Boolean(application.resumeVersionId))) {
            this.loadResumeLabels();
          } else {
            this.resumeVersionNumbers.clear();
          }
        },
        error: (error: HttpErrorResponse) => {
          const message = this.errorText(
            error,
            'Unable to load your applications.'
          );

          if (this.applications.length > 0) {
            this.staleMessage =
              'Could not refresh your applications. Previously loaded information may be out of date.';
            return;
          }

          this.errorMessage = message;
        }
      });
  }

  setFilter(filter: ApplicationFilter): void {
    this.filter = filter;
  }

  isCalculated(application: SeekerApplication): boolean {
    return application.frozenAssessmentStatus
      .toLowerCase() === 'calculated'
      && application.displayCompatibility !== null;
  }

  canWithdraw(application: SeekerApplication): boolean {
    return !this.staleMessage
      && ['Submitted', 'Screening', 'UnderReview', 'Shortlisted']
        .includes(application.status);
  }

  resumeLabel(application: SeekerApplication): string | null {
    if (!application.resumeVersionId) {
      return null;
    }

    const version = this.resumeVersionNumbers.get(application.resumeVersionId);
    return version ? `CV version ${version}` : 'Submitted CV linked';
  }

  askWithdraw(application: SeekerApplication): void {
    if (!this.canWithdraw(application) || this.withdrawingId) {
      return;
    }

    this.withdrawCandidate = application;
  }

  cancelWithdraw(): void {
    this.withdrawCandidate = null;
  }

  confirmWithdraw(): void {
    const application = this.withdrawCandidate;

    if (!application || !this.canWithdraw(application)) {
      return;
    }

    this.withdrawingId = application.id;
    this.actionMessage = '';

    this.api
      .withdraw(application.id)
      .pipe(finalize(() => {
        this.withdrawingId = null;
        this.withdrawCandidate = null;
      }))
      .subscribe({
        next: updated => {
          this.applications = this.applications.map(item =>
            item.id === updated.id ? updated : item
          );
          this.actionMessage = 'Application withdrawn.';
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 409) {
            this.loadApplications(false);
            this.actionMessage =
              'The application changed before withdrawal. We refreshed the current server status.';
            return;
          }

          this.actionMessage = this.errorText(
            error,
            'Unable to withdraw this application.'
          );
        }
      });
  }

  private loadResumeLabels(): void {
    this.api.getResume().subscribe({
      next: resume => {
        this.resumeVersionNumbers = new Map(
          resume.versions.map(version => [version.id, version.versionNumber])
        );
      },
      error: () => {
        this.resumeVersionNumbers.clear();
      }
    });
  }

  private errorText(error: HttpErrorResponse, fallback: string): string {
    if (error.status === 401 || error.status === 403) {
      return 'Your Job Seeker session cannot access these applications.';
    }

    return typeof error.error?.message === 'string'
      ? error.error.message
      : fallback;
  }
}