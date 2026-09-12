import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Observable, catchError, forkJoin, map, of } from 'rxjs';

import { SessionService } from '../../../core/auth/session.service';
import { CompanyMonogramComponent } from '../../../shared/avatar/company-monogram.component';
import { ErrorStateComponent } from '../../../shared/states/error-state.component';
import { LoadingStateComponent } from '../../../shared/states/loading-state.component';
import { SeekerApiService } from '../../seeker/data/seeker-api.service';
import {
  ApplicationReadiness,
  ApplyDecisionView,
  JobApplicationView,
  MatchCriterionStateValue,
  MatchEligibilityValue,
  MatchResultView,
  ResumeVersionView,
  ResumeView,
  VacancyView
} from '../../seeker/data/seeker.models';

type DetailTab = 'overview' | 'match' | 'requirements' | 'readiness' | 'apply';

interface OptionalResult<T> {
  data: T | null;
  status: number;
  message: string;
}

@Component({
  selector: 'app-job-detail',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    CompanyMonogramComponent,
    ErrorStateComponent,
    LoadingStateComponent
  ],
  templateUrl: './job-detail.component.html',
  styleUrl: './job-detail.component.css'
})
export class JobDetailComponent implements OnInit {
  private readonly api = inject(SeekerApiService);
  private readonly session = inject(SessionService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  vacancyId = '';
  vacancy: VacancyView | null = null;
  match: MatchResultView | null = null;
  readiness: ApplicationReadiness | null = null;
  resume: ResumeView | null = null;
  decision: ApplyDecisionView | null = null;

  loading = true;
  personalizedLoading = false;
  pageError = '';
  matchError = '';
  readinessError = '';
  resumeError = '';
  decisionError = '';
  applyError = '';
  appliedApplication: JobApplicationView | null = null;
  applying = false;
  baselineAcknowledged = false;
  activeTab: DetailTab = 'overview';

  ngOnInit(): void {
    this.vacancyId = this.route.snapshot.paramMap.get('vacancyId') ?? '';
    if (!this.vacancyId) {
      this.pageError = 'This job link is incomplete.';
      this.loading = false;
      return;
    }
    this.load();
  }

  get isJobSeeker(): boolean {
    const user = this.session.user();
    return user?.role === 'JobSeeker' && user.accountState === 'Active' && user.emailVerified;
  }

  get currentResumeVersion(): ResumeVersionView | null {
    if (!this.resume) return null;
    const currentId = this.resume.currentVersionId;
    return this.resume.versions.find(version => version.id === currentId)
      ?? this.resume.versions.find(version => version.isCurrent)
      ?? null;
  }

  get canSubmit(): boolean {
    if (!this.decision?.canSubmit || this.applying) return false;
    return !this.decision.requiresBaselineAcknowledgement || this.baselineAcknowledged;
  }

  load(): void {
    this.loading = true;
    this.pageError = '';

    this.api.getVacancy(this.vacancyId).subscribe({
      next: vacancy => {
        this.vacancy = vacancy;
        this.loading = false;
        if (this.isJobSeeker) this.loadPersonalized();
      },
      error: (error: HttpErrorResponse) => {
        this.loading = false;
        this.pageError = error.status === 404
          ? 'This published role is no longer available.'
          : 'The job detail could not be loaded. Please try again.';
      }
    });
  }

  loadPersonalized(): void {
    this.personalizedLoading = true;
    this.matchError = '';
    this.readinessError = '';
    this.resumeError = '';
    this.decisionError = '';

    forkJoin({
      match: this.optional(this.api.getMatch(this.vacancyId)),
      readiness: this.optional(this.api.getApplicationReadiness()),
      resume: this.optional(this.api.getResume()),
      decision: this.optional(this.api.getApplyDecision(this.vacancyId))
    }).subscribe(result => {
      this.match = result.match.data;
      this.readiness = result.readiness.data;
      this.resume = result.resume.data;
      this.decision = result.decision.data;
      this.matchError = result.match.message;
      this.readinessError = result.readiness.message;
      this.resumeError = result.resume.message;
      this.decisionError = result.decision.message;
      this.personalizedLoading = false;
    });
  }

  showTab(tab: DetailTab): void {
    this.activeTab = tab;
  }

  openApply(): void {
    this.activeTab = 'apply';
  }

  apply(): void {
    if (!this.canSubmit) return;

    this.applying = true;
    this.applyError = '';

    this.api.apply(this.vacancyId, this.baselineAcknowledged).subscribe({
      next: application => {
        this.appliedApplication = application;
        this.applying = false;
        void this.router.navigate(['/seeker/applications', application.id]);
      },
      error: (error: HttpErrorResponse) => {
        this.applying = false;
        const message = this.extractMessage(error);

        if (error.status === 409 && /already|duplicate/i.test(message)) {
          this.applyError = 'You already have an application for this vacancy. Open My Applications to continue.';
          return;
        }

        if (error.status === 409) {
          this.applyError = message || 'Your application preflight changed. The server decision has been refreshed.';
          this.loadPersonalized();
          return;
        }

        this.applyError = message || 'The application could not be submitted. Please try again.';
      }
    });
  }

  isCalculated(): boolean {
    return this.assessmentLabel(this.match?.assessmentStatus) === 'Calculated'
      && this.match?.displayCompatibility !== null
      && this.match?.displayCompatibility !== undefined;
  }

  assessmentLabel(value: MatchResultView['assessmentStatus'] | undefined): string {
    const numeric: Record<number, string> = {
      1: 'Calculated', 2: 'Provisional', 3: 'NotCalculated', 4: 'CalculationFailure'
    };
    return typeof value === 'number' ? numeric[value] ?? 'Unknown' : value ?? 'Unavailable';
  }

  eligibilityLabel(value: MatchEligibilityValue | undefined): string {
    const numeric: Record<number, string> = {
      1: 'MeetsBaseline', 2: 'PendingVerification', 3: 'IncompleteAssessment', 4: 'DoesNotMeetBaseline'
    };
    return typeof value === 'number' ? numeric[value] ?? 'Unknown' : value ?? 'Unavailable';
  }

  criterionStateLabel(value: MatchCriterionStateValue): string {
    const numeric: Record<number, string> = {
      1: 'Met', 2: 'NotMet', 3: 'NotDemonstrated', 4: 'Incomplete', 5: 'PendingVerification', 6: 'NotApplicable'
    };
    return typeof value === 'number' ? numeric[value] ?? 'Unknown' : value;
  }

  experienceLabel(months: number): string {
    if (months < 12) return `${months} month${months === 1 ? '' : 's'}`;
    const years = Math.floor(months / 12);
    const rest = months % 12;
    return rest ? `${years}y ${rest}m` : `${years} year${years === 1 ? '' : 's'}`;
  }

  salaryLabel(): string {
    if (!this.vacancy) return '';
    const min = this.vacancy.salaryMin;
    const max = this.vacancy.salaryMax;
    if (min === null && max === null) return '';
    const format = (value: number) => new Intl.NumberFormat('en-LK', {
      style: 'currency', currency: 'LKR', maximumFractionDigits: 0
    }).format(value);
    if (min !== null && max !== null) return `${format(min)}–${format(max)}`;
    return format(min ?? max ?? 0);
  }

  fileSize(bytes: number): string {
    if (bytes < 1024 * 1024) return `${Math.max(1, Math.round(bytes / 1024))} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }

  private optional<T>(source: Observable<T>): Observable<OptionalResult<T>> {
    return source.pipe(
      map(data => ({ data, status: 200, message: '' })),
      catchError((error: HttpErrorResponse) => of({
        data: null,
        status: error.status,
        message: this.optionalMessage(error)
      }))
    );
  }

  private optionalMessage(error: HttpErrorResponse): string {
    if (error.status === 401) return 'Sign in again to load this personalized section.';
    if (error.status === 403) return 'This account cannot access this personalized section yet.';
    if (error.status === 404) return 'The required candidate information was not found.';
    if (error.status === 409) return this.extractMessage(error) || 'The server reports a current-state conflict.';
    return 'This personalized section is temporarily unavailable.';
  }

  private extractMessage(error: HttpErrorResponse): string {
    const body = error.error as { message?: string; error?: string } | string | null;
    if (typeof body === 'string') return body;
    return body?.message ?? body?.error ?? '';
  }
}