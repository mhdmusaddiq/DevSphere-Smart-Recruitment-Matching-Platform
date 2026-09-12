import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { catchError, forkJoin, of } from 'rxjs';

import { EmployerApiService } from '../data-access/employer-api.service';
import {
  EmployerApplicationDetail,
  EmployerApplicationSnapshot,
  EmployerApplicationStatusHistory,
  EmployerContactRequest
} from '../data-access/employer.models';

@Component({
  selector: 'app-employer-applicant-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './employer-applicant-detail.component.html',
  styleUrl: './employer-applicant-detail.component.css'
})
export class EmployerApplicantDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(EmployerApiService);

  applicationId = '';
  vacancyId = '';

  applicant: EmployerApplicationDetail | null = null;
  snapshot: EmployerApplicationSnapshot | null = null;
  history: EmployerApplicationStatusHistory[] = [];
  contactRequest: EmployerContactRequest | null = null;

  loading = true;
  busy = false;
  resumeBusy = false;
  errorMessage = '';
  successMessage = '';
  supplementaryWarning = '';

  ngOnInit(): void {
    this.applicationId =
      this.route.snapshot.paramMap.get('applicationId') ?? '';

    this.vacancyId =
      this.route.snapshot.paramMap.get('vacancyId') ??
      this.route.snapshot.queryParamMap.get('vacancyId') ??
      '';

    if (!this.applicationId) {
      this.loading = false;
      this.errorMessage = 'Application identifier is missing.';
      return;
    }

    this.load();
  }

  load(clearFeedback = true): void {
    this.loading = true;
    this.errorMessage = '';
    this.supplementaryWarning = '';

    if (clearFeedback) {
      this.successMessage = '';
    }

    forkJoin({
      applicant: this.api.getEmployerApplication(
        this.applicationId
      ),
      snapshot: this.api.getApplicationSnapshot(
        this.applicationId
      ).pipe(
        catchError(() => {
          this.supplementaryWarning =
            'Some frozen snapshot details are unavailable.';
          return of(null);
        })
      ),
      history: this.api.getApplicationStatusHistory(
        this.applicationId
      ).pipe(
        catchError(() => {
          this.supplementaryWarning =
            'Some application history details are unavailable.';
          return of([]);
        })
      ),
      contacts: this.api.getEmployerContactRequests().pipe(
        catchError(() => {
          this.supplementaryWarning =
            'Contact-consent status is temporarily unavailable.';
          return of([]);
        })
      )
    }).subscribe({
      next: result => {
        this.applicant = result.applicant;
        this.snapshot = result.snapshot;
        this.history = result.history;
        this.contactRequest =
          result.contacts.find(
            request =>
              request.jobApplicationId === this.applicationId
          ) ?? null;

        this.vacancyId =
          this.vacancyId || result.applicant.vacancyId;

        this.loading = false;
      },
      error: (error: HttpErrorResponse) => {
        this.loading = false;
        this.applicant = null;

        this.errorMessage =
          error.status === 403
            ? 'You are not authorized to review this application.'
            : error.status === 404
              ? 'Applicant could not be found.'
              : 'Unable to load applicant detail.';
      }
    });
  }

  assessmentLabel(
    value: string | number
  ): string {
    const labels: Record<string, string> = {
      '1': 'Calculated',
      '2': 'Provisional',
      '3': 'NotCalculated',
      '4': 'CalculationFailure'
    };

    return labels[String(value)] ?? String(value);
  }

  eligibilityLabel(
    value: string | number
  ): string {
    const labels: Record<string, string> = {
      '1': 'MeetsBaseline',
      '2': 'PendingVerification',
      '3': 'IncompleteAssessment',
      '4': 'DoesNotMeetBaseline'
    };

    return labels[String(value)] ?? String(value);
  }

  hasCalculatedScore(): boolean {
    return (
      this.applicant !== null &&
      this.assessmentLabel(
        this.applicant.assessmentStatus
      ) === 'Calculated' &&
      this.applicant.displayCompatibility !== null
    );
  }

  get availableStatusTransitions(): string[] {
    switch (this.applicant?.status) {
      case 'Submitted':
        return [
          'Screening',
          'UnderReview',
          'Shortlisted',
          'Rejected'
        ];
      case 'Screening':
        return [
          'UnderReview',
          'Shortlisted',
          'Rejected'
        ];
      case 'UnderReview':
        return ['Shortlisted', 'Rejected'];
      case 'Shortlisted':
        return ['Selected', 'Rejected'];
      default:
        return [];
    }
  }

  updateStatus(status: string): void {
    if (
      this.busy ||
      !this.availableStatusTransitions.includes(status)
    ) {
      return;
    }

    this.busy = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.api.updateApplicationStatus(
      this.applicationId,
      status
    ).subscribe({
      next: () => {
        this.busy = false;
        this.successMessage =
          `Application status updated to ${status}.`;
        this.load(false);
      },
      error: (error: HttpErrorResponse) => {
        this.busy = false;

        this.errorMessage =
          error.status === 409
            ? 'Application status changed. Refresh and try again.'
            : error.status === 400
              ? 'That application status transition is not allowed.'
              : error.status === 403
                ? 'You are not authorized to update this application.'
                : error.status === 404
                  ? 'Application could not be found.'
                  : 'Unable to update application status.';
      }
    });
  }

  requestContact(): void {
    if (this.busy || this.contactRequest) {
      return;
    }

    this.busy = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.api.sendContactRequest(
      this.applicationId
    ).subscribe({
      next: request => {
        this.busy = false;
        this.contactRequest = request;
        this.successMessage =
          'Contact request created.';
      },
      error: (error: HttpErrorResponse) => {
        this.busy = false;

        this.errorMessage =
          error.status === 409
            ? 'A contact request already exists for this application.'
            : error.status === 403
              ? 'You are not authorized to request contact.'
              : error.status === 404
                ? 'Application could not be found.'
                : 'Unable to create contact request.';
      }
    });
  }

  downloadResume(): void {
    if (
      this.resumeBusy ||
      !this.applicant?.resumeVersionId
    ) {
      return;
    }

    this.resumeBusy = true;
    this.errorMessage = '';

    this.api.downloadApplicationResume(
      this.applicationId
    ).subscribe({
      next: blob => {
        this.resumeBusy = false;

        const href =
          URL.createObjectURL(blob);

        const anchor =
          document.createElement('a');

        anchor.href = href;
        anchor.download =
          `application-${this.applicationId}-resume`;

        anchor.click();
        URL.revokeObjectURL(href);
      },
      error: (error: HttpErrorResponse) => {
        this.resumeBusy = false;

        this.errorMessage =
          error.status === 403
            ? 'You are not authorized to download this resume.'
            : error.status === 404
              ? 'The submitted resume is unavailable.'
              : 'Unable to download the submitted resume.';
      }
    });
  }
}
