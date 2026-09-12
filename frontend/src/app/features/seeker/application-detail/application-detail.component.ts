import { CommonModule } from '@angular/common';
import { HttpErrorResponse, HttpResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { Resume, ResumeVersion } from '../../../core/models/resume.model';
import { CompanyMonogramComponent } from '../../../shared/avatar/company-monogram.component';
import { ErrorStateComponent } from '../../../shared/states/error-state.component';
import { LoadingStateComponent } from '../../../shared/states/loading-state.component';
import { SeekerWorkflowApiService } from '../data/seeker-workflow-api.service';
import {
  ApplicationSnapshot,
  ApplicationStatusHistory,
  CandidateWorkflowSummary,
  ContactRequestView,
  InterviewSlotView,
  InterviewView,
  SeekerApplication
} from '../data/seeker-workflow.models';

type ContactAction = 'Accepted' | 'Declined' | 'Revoked';

@Component({
  selector: 'app-application-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    CompanyMonogramComponent,
    ErrorStateComponent,
    LoadingStateComponent
  ],
  templateUrl: './application-detail.component.html',
  styleUrl: './application-detail.component.css'
})
export class ApplicationDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(SeekerWorkflowApiService);

  applicationId = '';
  application: SeekerApplication | null = null;
  snapshot: ApplicationSnapshot | null = null;
  history: ApplicationStatusHistory[] = [];
  workflow: CandidateWorkflowSummary | null = null;
  contactRequest: ContactRequestView | null = null;
  resume: Resume | null = null;

  loading = true;
  withdrawing = false;
  downloading = false;
  contactBusy = false;

  coreError = '';
  snapshotError = '';
  historyError = '';
  workflowError = '';
  contactError = '';
  resumeError = '';
  actionMessage = '';

  withdrawOpen = false;
  pendingContactAction: ContactAction | null = null;

  ngOnInit(): void {
    this.applicationId = this.route.snapshot.paramMap.get('applicationId') ?? '';

    if (!this.applicationId) {
      this.loading = false;
      this.coreError = 'Application not found.';
      return;
    }

    this.loadApplication();
  }

  get submittedResumeVersion(): ResumeVersion | null {
    const versionId = this.snapshot?.resumeVersionId ?? this.application?.resumeVersionId;

    if (!versionId || !this.resume) {
      return null;
    }

    return this.resume.versions.find(version => version.id === versionId) ?? null;
  }

  get frozenCompatibility(): number | null {
    if (
      this.snapshot?.compatibilityStatus.toLowerCase() !== 'calculated'
      || this.snapshot.displayCompatibilityScore === null
    ) {
      return null;
    }

    return this.snapshot.displayCompatibilityScore;
  }

  get matchedSkills(): string[] {
    return this.safeJsonList(this.snapshot?.matchedSkillsJson);
  }

  get gapSkills(): string[] {
    return this.safeJsonList(this.snapshot?.gapSkillsJson);
  }

  canWithdraw(): boolean {
    return !!this.application
      && ['Submitted', 'Screening', 'UnderReview', 'Shortlisted']
        .includes(this.application.status);
  }

  loadApplication(): void {
    this.loading = true;
    this.coreError = '';
    this.actionMessage = '';

    this.api
      .getApplication(this.applicationId)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: application => {
          this.application = application;
          this.loadSecondary();
        },
        error: (error: HttpErrorResponse) => {
          this.coreError = error.status === 404 || error.status === 403
            ? 'This application is unavailable or does not belong to this account.'
            : this.errorText(error, 'Unable to load this application.');
        }
      });
  }

  openWithdraw(): void {
    if (this.canWithdraw()) {
      this.withdrawOpen = true;
    }
  }

  closeWithdraw(): void {
    if (!this.withdrawing) {
      this.withdrawOpen = false;
    }
  }

  confirmWithdraw(): void {
    if (!this.application || !this.canWithdraw()) {
      return;
    }

    this.withdrawing = true;
    this.actionMessage = '';

    this.api
      .withdraw(this.application.id)
      .pipe(finalize(() => {
        this.withdrawing = false;
        this.withdrawOpen = false;
      }))
      .subscribe({
        next: updated => {
          this.application = updated;
          this.actionMessage = 'Application withdrawn.';
          this.reloadHistory();
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 409) {
            this.loadApplication();
            this.actionMessage =
              'The application status changed before withdrawal. Current server state has been refreshed.';
            return;
          }

          this.actionMessage = this.errorText(
            error,
            'Unable to withdraw this application.'
          );
        }
      });
  }

  askContactAction(action: ContactAction): void {
    if (!this.contactRequest || this.contactBusy) {
      return;
    }

    const allowed =
      (this.contactRequest.status === 'Pending'
        && (action === 'Accepted' || action === 'Declined'))
      || (this.contactRequest.status === 'Accepted' && action === 'Revoked');

    if (allowed) {
      this.pendingContactAction = action;
    }
  }

  cancelContactAction(): void {
    if (!this.contactBusy) {
      this.pendingContactAction = null;
    }
  }

  confirmContactAction(): void {
    const request = this.contactRequest;
    const action = this.pendingContactAction;

    if (!request || !action) {
      return;
    }

    this.contactBusy = true;
    this.contactError = '';

    this.api
      .updateContactStatus(request.id, action)
      .pipe(finalize(() => {
        this.contactBusy = false;
        this.pendingContactAction = null;
      }))
      .subscribe({
        next: updated => {
          this.contactRequest = updated;
          this.actionMessage =
            action === 'Accepted'
              ? 'Direct contact sharing is now allowed while the server relationship remains valid.'
              : action === 'Declined'
                ? 'Contact request declined.'
                : 'Direct contact sharing revoked.';
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 409) {
            this.contactError =
              'This contact request changed before your decision. Current server state has been refreshed.';
            this.reloadContact();
            return;
          }

          this.contactError = this.errorText(
            error,
            'Unable to update this contact request.'
          );
        }
      });
  }

  downloadSubmittedResume(): void {
    const versionId = this.snapshot?.resumeVersionId ?? this.application?.resumeVersionId;

    if (!versionId || this.downloading) {
      return;
    }

    this.downloading = true;
    this.resumeError = '';

    this.api
      .downloadResumeVersion(versionId)
      .pipe(finalize(() => (this.downloading = false)))
      .subscribe({
        next: response => this.saveDownload(
          response,
          this.submittedResumeVersion?.originalFileName ?? 'submitted-cv.pdf'
        ),
        error: (error: HttpErrorResponse) => {
          this.resumeError = this.errorText(
            error,
            'Unable to download the submitted CV version.'
          );
        }
      });
  }

  slotsFor(interview: InterviewView): InterviewSlotView[] {
    return this.workflow?.interviewSlots.filter(
      slot => slot.interviewId === interview.id
    ) ?? [];
  }

  private loadSecondary(): void {
    this.reloadSnapshot();
    this.reloadHistory();
    this.reloadWorkflow();
    this.reloadContact();
    this.reloadResume();
  }

  private reloadSnapshot(): void {
    this.snapshotError = '';

    this.api.getSnapshot(this.applicationId).subscribe({
      next: snapshot => (this.snapshot = snapshot),
      error: (error: HttpErrorResponse) => {
        this.snapshot = null;
        this.snapshotError = error.status === 404
          ? 'The immutable application-time match snapshot is unavailable.'
          : this.errorText(error, 'Unable to load the application snapshot.');
      }
    });
  }

  private reloadHistory(): void {
    this.historyError = '';

    this.api.getStatusHistory(this.applicationId).subscribe({
      next: history => (this.history = history),
      error: (error: HttpErrorResponse) => {
        this.history = [];
        this.historyError = this.errorText(
          error,
          'Unable to load status history.'
        );
      }
    });
  }

  private reloadWorkflow(): void {
    this.workflowError = '';

    this.api.getWorkflow(this.applicationId).subscribe({
      next: workflow => (this.workflow = workflow),
      error: (error: HttpErrorResponse) => {
        this.workflow = null;
        this.workflowError = this.errorText(
          error,
          'Workflow details are temporarily unavailable.'
        );
      }
    });
  }

  private reloadContact(): void {
    this.contactError = '';

    this.api.getContactRequests().subscribe({
      next: requests => {
        this.contactRequest = requests.find(
          request => request.jobApplicationId === this.applicationId
        ) ?? null;
      },
      error: (error: HttpErrorResponse) => {
        this.contactRequest = null;
        this.contactError = this.errorText(
          error,
          'Contact request status is temporarily unavailable.'
        );
      }
    });
  }

  private reloadResume(): void {
    this.resumeError = '';

    this.api.getResume().subscribe({
      next: resume => (this.resume = resume),
      error: (error: HttpErrorResponse) => {
        this.resume = null;

        if (error.status !== 404) {
          this.resumeError = this.errorText(
            error,
            'CV version metadata is temporarily unavailable.'
          );
        }
      }
    });
  }

  private safeJsonList(value: string | null | undefined): string[] {
    if (!value) {
      return [];
    }

    try {
      const parsed: unknown = JSON.parse(value);
      return Array.isArray(parsed)
        ? parsed.filter((item): item is string => typeof item === 'string')
        : [];
    } catch {
      return [];
    }
  }

  private saveDownload(response: HttpResponse<Blob>, fileName: string): void {
    if (!response.body) {
      this.resumeError = 'The CV download returned no file.';
      return;
    }

    const objectUrl = URL.createObjectURL(response.body);
    const anchor = document.createElement('a');
    anchor.href = objectUrl;
    anchor.download = fileName;
    anchor.click();
    URL.revokeObjectURL(objectUrl);
  }

  private errorText(error: HttpErrorResponse, fallback: string): string {
    return typeof error.error?.message === 'string'
      ? error.error.message
      : fallback;
  }
}